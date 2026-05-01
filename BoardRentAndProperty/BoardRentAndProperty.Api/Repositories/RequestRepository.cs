using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Api.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public RequestRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Request> RequestsWithNavigations(AppDbContext dbContext) =>
            dbContext.Requests
                .Include(request => request.Game)
                .Include(request => request.Renter)
                .Include(request => request.Owner)
                .Include(request => request.OfferingUser);

        public ImmutableList<Request> GetAll()
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext).ToImmutableList();
        }

        public void Add(Request request)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            request.Game = ResolveGame(dbContext, request.Game);
            request.Renter = ResolveAccount(dbContext, request.Renter);
            request.Owner = ResolveAccount(dbContext, request.Owner);
            request.OfferingUser = ResolveAccount(dbContext, request.OfferingUser);
            dbContext.Requests.Add(request);
            dbContext.SaveChanges();

            var saved = RequestsWithNavigations(dbContext).FirstOrDefault(savedRequest => savedRequest.Id == request.Id);
            if (saved != null)
            {
                request.Game = saved.Game;
                request.Renter = saved.Renter;
                request.Owner = saved.Owner;
                request.OfferingUser = saved.OfferingUser;
            }
        }

        public Request Delete(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var request = RequestsWithNavigations(dbContext).FirstOrDefault(repositoryRequest => repositoryRequest.Id == id);
            if (request == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Requests.Remove(request);
            dbContext.SaveChanges();
            return request;
        }

        public void Update(int id, Request updated)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = RequestsWithNavigations(dbContext).FirstOrDefault(request => request.Id == id);
            if (existing == null)
            {
                return;
            }

            if (updated.Game != null)
            {
                existing.Game = ResolveGame(dbContext, updated.Game);
            }

            if (updated.Renter != null)
            {
                existing.Renter = ResolveAccount(dbContext, updated.Renter);
            }

            if (updated.Owner != null)
            {
                existing.Owner = ResolveAccount(dbContext, updated.Owner);
            }

            existing.OfferingUser = ResolveAccount(dbContext, updated.OfferingUser);
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            existing.Status = updated.Status;
            dbContext.SaveChanges();
        }

        public void UpdateStatus(int requestId, RequestStatus status, Guid? offeringAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = RequestsWithNavigations(dbContext).FirstOrDefault(request => request.Id == requestId);
            if (existing == null)
            {
                return;
            }

            existing.Status = status;
            existing.OfferingUser = FindAccountById(dbContext, offeringAccountId);
            dbContext.SaveChanges();
        }

        public Request Get(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var request = RequestsWithNavigations(dbContext).FirstOrDefault(repositoryRequest => repositoryRequest.Id == id);
            if (request == null)
            {
                throw new KeyNotFoundException();
            }

            return request;
        }

        public ImmutableList<Request> GetRequestsByOwner(Guid ownerAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Owner != null && request.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByRenter(Guid renterAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Renter != null && request.Renter.Id == renterAccountId)
                .ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByGame(int gameId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Game != null && request.Game.Id == gameId)
                .ToImmutableList();
        }

        public ImmutableList<Request> GetOverlappingRequests(int gameId, int excludeRequestId, DateTime bufferedStart, DateTime bufferedEnd)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RequestsWithNavigations(dbContext)
                .Where(request => request.Game != null && request.Game.Id == gameId
                    && request.Id != excludeRequestId
                    && request.StartDate < bufferedEnd
                    && request.EndDate > bufferedStart)
                .ToImmutableList();
        }

        public int ApproveAtomically(Request approvedRequest, ImmutableList<Request> overlappingRequests)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                foreach (var conflict in overlappingRequests)
                {
                    var conflictNotifications = dbContext.Notifications
                        .Where(notification => notification.RelatedRequest != null && notification.RelatedRequest.Id == conflict.Id)
                        .ToList();
                    dbContext.Notifications.RemoveRange(conflictNotifications);
                }

                var approvedNotifications = dbContext.Notifications
                    .Where(notification => notification.RelatedRequest != null && notification.RelatedRequest.Id == approvedRequest.Id)
                    .ToList();
                dbContext.Notifications.RemoveRange(approvedNotifications);

                var newRental = new Rental
                {
                    Game = ResolveGame(dbContext, approvedRequest.Game),
                    Renter = ResolveAccount(dbContext, approvedRequest.Renter),
                    Owner = ResolveAccount(dbContext, approvedRequest.Owner),
                    StartDate = approvedRequest.StartDate,
                    EndDate = approvedRequest.EndDate,
                };
                dbContext.Rentals.Add(newRental);
                dbContext.SaveChanges();

                foreach (var conflict in overlappingRequests)
                {
                    var conflictEntity = dbContext.Requests.FirstOrDefault(request => request.Id == conflict.Id);
                    if (conflictEntity != null)
                    {
                        dbContext.Requests.Remove(conflictEntity);
                    }
                }

                var approvedEntity = dbContext.Requests.FirstOrDefault(request => request.Id == approvedRequest.Id);
                if (approvedEntity != null)
                {
                    dbContext.Requests.Remove(approvedEntity);
                }

                dbContext.SaveChanges();
                transaction.Commit();
                return newRental.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static Account? ResolveAccount(AppDbContext dbContext, Account? account)
        {
            if (account == null)
            {
                return null;
            }

            var cached = dbContext.Accounts.Local.FirstOrDefault(cachedAccount => cachedAccount.Id == account.Id);
            if (cached != null)
            {
                return cached;
            }

            if (dbContext.Entry(account).State == EntityState.Detached)
            {
                dbContext.Attach(account);
            }

            return account;
        }

        private static Game? ResolveGame(AppDbContext dbContext, Game? game)
        {
            if (game == null)
            {
                return null;
            }

            var cached = dbContext.Games.Local.FirstOrDefault(cachedGame => cachedGame.Id == game.Id);
            if (cached != null)
            {
                return cached;
            }

            if (dbContext.Entry(game).State == EntityState.Detached)
            {
                dbContext.Attach(game);
            }

            return game;
        }

        private static Account? FindAccountById(AppDbContext dbContext, Guid? accountId)
        {
            if (!accountId.HasValue)
            {
                return null;
            }

            var cached = dbContext.Accounts.Local.FirstOrDefault(cachedAccount => cachedAccount.Id == accountId.Value);
            return cached ?? dbContext.Accounts.Find(accountId.Value);
        }
    }
}
