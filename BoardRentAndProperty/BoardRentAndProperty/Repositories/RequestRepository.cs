using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private const int NewRequestId = 0;
        private const int MissingForeignKeyId = 0;
        private const string ConnectionStringName = "BoardRent";

        private readonly string boardRentConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString ?? string.Empty;

        private const string BaseRequestSelectQuery =
            "SELECT r.*, renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
            "g.name AS game_name, g.image AS game_image, " +
            "offeringUser.display_name AS offering_user_display_name " +
            "FROM Requests r " +
            "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
            "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
            "LEFT JOIN Games g ON g.game_id = r.game_id " +
            "LEFT JOIN Users offeringUser ON offeringUser.Id = r.offering_user_id";

        private static Request ReadFullRequestFromReader(SqlDataReader databaseReader)
        {
            var requestedGame = new Game
            {
                Id = (int)databaseReader["game_id"],
                Name = databaseReader["game_name"] as string ?? string.Empty,
                Image = databaseReader["game_image"] as byte[] ?? Array.Empty<byte>()
            };
            var renter = new Account { PamUserId = (int)databaseReader["renter_id"], DisplayName = databaseReader["renter_display_name"] as string ?? string.Empty };
            var owner = new Account { PamUserId = (int)databaseReader["owner_id"], DisplayName = databaseReader["owner_display_name"] as string ?? string.Empty };
            var requestStatus = (RequestStatus)(int)databaseReader["status"];
            Account? offeringAccount = null;
            var offeringUserIdValue = databaseReader["offering_user_id"];
            if (offeringUserIdValue != DBNull.Value)
            {
                offeringAccount = new Account { PamUserId = (int)offeringUserIdValue, DisplayName = databaseReader["offering_user_display_name"] as string ?? string.Empty };
            }
            return new Request((int)databaseReader["request_id"], requestedGame, renter, owner,
                (DateTime)databaseReader["start_date"], (DateTime)databaseReader["end_date"], requestStatus, offeringAccount);
        }

        public ImmutableList<Request> GetAll()
        {
            var allRetrievedRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allRetrievedRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return allRetrievedRequests.ToImmutableList();
        }

        public void Add(Request requestToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    AddRequestWithinTransaction(requestToInsert, connection, transaction);
                    transaction.Commit();
                }
            }
        }

        private static void AddRequestWithinTransaction(Request requestToInsert, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO Requests(game_id, renter_id, owner_id, start_date, end_date, status, offering_user_id) " +
                "VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date, @status, @offering_user_id); " +
                "SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@game_id", requestToInsert.Game?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@renter_id", requestToInsert.Renter?.PamUserId ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@owner_id", requestToInsert.Owner?.PamUserId ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@start_date", requestToInsert.StartDate);
            command.Parameters.AddWithValue("@end_date", requestToInsert.EndDate);
            command.Parameters.AddWithValue("@status", (int)requestToInsert.Status);
            command.Parameters.AddWithValue("@offering_user_id", requestToInsert.OfferingAccount?.PamUserId ?? (object)DBNull.Value);
            requestToInsert.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        private static readonly string DeleteRequestWithOutputQuery =
            "DELETE r OUTPUT deleted.request_id, deleted.game_id, deleted.renter_id, deleted.owner_id, " +
            "deleted.start_date, deleted.end_date, deleted.status, deleted.offering_user_id, " +
            "renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
            "g.name AS game_name, g.image AS game_image, " +
            "offeringUser.display_name AS offering_user_display_name " +
            "FROM Requests r " +
            "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
            "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
            "LEFT JOIN Games g ON g.game_id = r.game_id " +
            "LEFT JOIN Users offeringUser ON offeringUser.Id = r.offering_user_id " +
            "WHERE r.request_id = @id";

        public Request Delete(int requestIdToRemove)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    UnlinkNotificationsFromRequestWithinTransaction(requestIdToRemove, connection, transaction);
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = DeleteRequestWithOutputQuery;
                    command.Parameters.AddWithValue("@id", requestIdToRemove);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var deletedRequest = ReadRequestFromDeleteOutputReader(reader);
                            transaction.Commit();
                            return deletedRequest;
                        }
                    }

                    transaction.Rollback();
                }
            }
            throw new KeyNotFoundException();
        }

        private static Request ReadRequestFromDeleteOutputReader(SqlDataReader deleteOutputReader)
        {
            var deletedRequestedGame = new Game
            {
                Id = (int)deleteOutputReader["game_id"],
                Name = deleteOutputReader["game_name"] as string ?? string.Empty,
                Image = deleteOutputReader["game_image"] as byte[] ?? Array.Empty<byte>()
            };
            var deletedRenter = new Account { PamUserId = (int)deleteOutputReader["renter_id"], DisplayName = deleteOutputReader["renter_display_name"] as string ?? string.Empty };
            var deletedOwner = new Account { PamUserId = (int)deleteOutputReader["owner_id"], DisplayName = deleteOutputReader["owner_display_name"] as string ?? string.Empty };
            var deletedRequestStatus = (RequestStatus)(int)deleteOutputReader["status"];
            Account? deletedOfferingAccount = null;
            var deletedOfferingUserIdValue = deleteOutputReader["offering_user_id"];
            if (deletedOfferingUserIdValue != DBNull.Value)
            {
                deletedOfferingAccount = new Account { PamUserId = (int)deletedOfferingUserIdValue, DisplayName = deleteOutputReader["offering_user_display_name"] as string ?? string.Empty };
            }

            return new Request((int)deleteOutputReader["request_id"], deletedRequestedGame, deletedRenter, deletedOwner,
                (DateTime)deleteOutputReader["start_date"], (DateTime)deleteOutputReader["end_date"], deletedRequestStatus, deletedOfferingAccount);
        }

        public void Update(int requestIdToUpdate, Request requestDataToUpdate)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE Requests SET game_id = @game_id, renter_id = @renter_id, owner_id = @owner_id, " +
                        "start_date = @start_date, end_date = @end_date, status = @status, " +
                        "offering_user_id = @offering_user_id WHERE request_id = @id";
                    command.Parameters.AddWithValue("@id", requestIdToUpdate);
                    command.Parameters.AddWithValue("@game_id", requestDataToUpdate.Game?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@renter_id", requestDataToUpdate.Renter?.PamUserId ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@owner_id", requestDataToUpdate.Owner?.PamUserId ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@start_date", requestDataToUpdate.StartDate);
                    command.Parameters.AddWithValue("@end_date", requestDataToUpdate.EndDate);
                    command.Parameters.AddWithValue("@status", (int)requestDataToUpdate.Status);
                    command.Parameters.AddWithValue("@offering_user_id", requestDataToUpdate.OfferingAccount?.PamUserId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStatus(int requestIdToUpdateStatus, RequestStatus newRequestStatus, int? offeringUserId)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE Requests SET status = @status, offering_user_id = @offering_user_id WHERE request_id = @id";
                    command.Parameters.AddWithValue("@id", requestIdToUpdateStatus);
                    command.Parameters.AddWithValue("@status", (int)newRequestStatus);
                    command.Parameters.AddWithValue("@offering_user_id", offeringUserId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Request Get(int requestIdToFind)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.request_id = @id";
                    command.Parameters.AddWithValue("@id", requestIdToFind);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadFullRequestFromReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public ImmutableList<Request> GetRequestsByOwner(int ownerUserId)
        {
            var ownerRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.owner_id = @owner_id";
                    command.Parameters.AddWithValue("@owner_id", ownerUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ownerRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return ownerRequests.ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByRenter(int renterUserId)
        {
            var renterRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.renter_id = @renter_id";
                    command.Parameters.AddWithValue("@renter_id", renterUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            renterRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return renterRequests.ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByGame(int requestedGameId)
        {
            var gameRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.game_id = @game_id";
                    command.Parameters.AddWithValue("@game_id", requestedGameId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            gameRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return gameRequests.ToImmutableList();
        }

        public ImmutableList<Request> GetOverlappingRequests(
            int gameIdForOverlapCheck,
            int requestIdToExclude,
            DateTime bufferedStartDate,
            DateTime bufferedEndDate)
        {
            using var connection = new SqlConnection(boardRentConnectionString);
            connection.Open();
            return QueryOverlappingRequestsWithinConnection(
                gameIdForOverlapCheck,
                requestIdToExclude,
                bufferedStartDate,
                bufferedEndDate,
                connection,
                transaction: null).ToImmutableList();
        }

        public int ApproveAtomically(
            Request requestToApprove,
            ImmutableList<Request> conflictingRequests)
        {
            using var connection = new SqlConnection(boardRentConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                var newRentalIdentifier = InsertRentalFromApprovedRequest(requestToApprove, connection, transaction);

                foreach (var conflictingRequest in conflictingRequests)
                {
                    DeleteRequestWithinTransaction(conflictingRequest.Id, connection, transaction);
                }

                DeleteRequestWithinTransaction(requestToApprove.Id, connection, transaction);

                transaction.Commit();
                return newRentalIdentifier;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static int InsertRentalFromApprovedRequest(Request approvedRequest, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO Rentals(game_id, renter_id, owner_id, start_date, end_date) " +
                "VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date); SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@game_id", approvedRequest.Game?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@renter_id", approvedRequest.Renter?.PamUserId ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@owner_id", approvedRequest.Owner?.PamUserId ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@start_date", approvedRequest.StartDate);
            command.Parameters.AddWithValue("@end_date", approvedRequest.EndDate);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static List<Request> QueryOverlappingRequestsWithinConnection(
            int gameIdForOverlapCheck,
            int requestIdToExclude,
            DateTime bufferedStartDate,
            DateTime bufferedEndDate,
            SqlConnection connection,
            SqlTransaction? transaction)
        {
            var overlappingRequests = new List<Request>();
            using var command = connection.CreateCommand();
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandText =
                "SELECT r.request_id, r.game_id, r.renter_id, r.owner_id, r.start_date, r.end_date, " +
                "renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
                "g.name AS game_name, g.image AS game_image " +
                "FROM Requests r " +
                "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
                "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
                "LEFT JOIN Games g ON g.game_id = r.game_id " +
                "WHERE r.game_id = @game_id AND r.request_id != @exclude_id " +
                "AND r.start_date < @buffered_end AND r.end_date > @buffered_start";
            command.Parameters.AddWithValue("@game_id", gameIdForOverlapCheck);
            command.Parameters.AddWithValue("@exclude_id", requestIdToExclude);
            command.Parameters.AddWithValue("@buffered_end", bufferedEndDate);
            command.Parameters.AddWithValue("@buffered_start", bufferedStartDate);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var overlappingGame = new Game
                {
                    Id = (int)reader["game_id"],
                    Name = reader["game_name"] as string ?? string.Empty,
                    Image = reader["game_image"] as byte[] ?? Array.Empty<byte>()
                };
                var overlappingRenter = new Account { PamUserId = (int)reader["renter_id"], DisplayName = reader["renter_display_name"] as string ?? string.Empty };
                var overlappingOwner = new Account { PamUserId = (int)reader["owner_id"], DisplayName = reader["owner_display_name"] as string ?? string.Empty };
                overlappingRequests.Add(new Request(
                    (int)reader["request_id"],
                    overlappingGame,
                    overlappingRenter,
                    overlappingOwner,
                    (DateTime)reader["start_date"],
                    (DateTime)reader["end_date"]));
            }

            return overlappingRequests;
        }

        private static void UnlinkNotificationsFromRequestWithinTransaction(int linkedRequestId, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE Notifications SET related_request_id = NULL WHERE related_request_id = @id";
            command.Parameters.AddWithValue("@id", linkedRequestId);
            command.ExecuteNonQuery();
        }

        private static void DeleteRequestWithinTransaction(int requestIdToDelete, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            UnlinkNotificationsFromRequestWithinTransaction(requestIdToDelete, connection, transaction);
            command.CommandText = "DELETE FROM Requests WHERE request_id = @id";
            command.Parameters.AddWithValue("@id", requestIdToDelete);
            command.ExecuteNonQuery();
        }
    }
}
