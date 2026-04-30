using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Data;
using BoardRentAndProperty.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext dbContext;

        public RequestRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private IQueryable<Request> RequestsWithNavigations =>
            dbContext.Requests
                .Include(request => request.Game)
                .Include(request => request.Renter)
                .Include(request => request.Owner)
                .Include(request => request.OfferingUser);

        public ImmutableList<Request> GetAll() => RequestsWithNavigations.ToImmutableList();

        public void Add(Request request)
        {
            request.Game = ResolveGame(request.Game);
            request.Renter = ResolveAccount(request.Renter);
            request.Owner = ResolveAccount(request.Owner);
            request.OfferingUser = ResolveAccount(request.OfferingUser);
            dbContext.Requests.Add(request);
            dbContext.SaveChanges();
            var saved = RequestsWithNavigations.FirstOrDefault(savedRequest => savedRequest.Id == request.Id);
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
            var request = RequestsWithNavigations.FirstOrDefault(request => request.Id == id);
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
            var existing = RequestsWithNavigations.FirstOrDefault(request => request.Id == id);
            if (existing == null)
            {
                return;
            }
            if (updated.Game != null)
            {
                existing.Game = ResolveGame(updated.Game);
            }
            if (updated.Renter != null)
            {
                existing.Renter = ResolveAccount(updated.Renter);
            }
            if (updated.Owner != null)
            {
                existing.Owner = ResolveAccount(updated.Owner);
            }
            existing.OfferingUser = ResolveAccount(updated.OfferingUser);
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            existing.Status = updated.Status;
            dbContext.SaveChanges();
        }

        public void UpdateStatus(int requestId, RequestStatus status, Guid? offeringAccountId)
        {
            var existing = RequestsWithNavigations.FirstOrDefault(request => request.Id == requestId);
            if (existing == null)
            {
                return;
            }
            existing.Status = status;
            existing.OfferingUser = FindAccountById(offeringAccountId);
            dbContext.SaveChanges();
        }

        public Request Get(int id)
        {
            var request = RequestsWithNavigations.FirstOrDefault(request => request.Id == id);
            if (request == null)
            {
                throw new KeyNotFoundException();
            }
            return request;
        }

        public ImmutableList<Request> GetRequestsByOwner(Guid ownerAccountId) =>
            RequestsWithNavigations.Where(request => request.Owner.Id == ownerAccountId).ToImmutableList();

        public ImmutableList<Request> GetRequestsByRenter(Guid renterAccountId) =>
            RequestsWithNavigations.Where(request => request.Renter.Id == renterAccountId).ToImmutableList();

        public ImmutableList<Request> GetRequestsByGame(int gameId) =>
            RequestsWithNavigations.Where(request => request.Game.Id == gameId).ToImmutableList();

        public ImmutableList<Request> GetOverlappingRequests(int gameId, int excludeRequestId, DateTime bufferedStart, DateTime bufferedEnd)
        {
            return RequestsWithNavigations
                .Where(request => request.Game.Id == gameId
                    && request.Id != excludeRequestId
                    && request.StartDate < bufferedEnd
                    && request.EndDate > bufferedStart)
                .ToImmutableList();
        }

        public int ApproveAtomically(Request approvedRequest, ImmutableList<Request> overlappingRequests)
        {
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                foreach (var conflict in overlappingRequests)
                {
                    var conflictNotifications = dbContext.Notifications
                        .Where(notification => notification.RelatedRequest.Id == conflict.Id)
                        .ToList();
                    dbContext.Notifications.RemoveRange(conflictNotifications);
                }
                var approvedNotifications = dbContext.Notifications
                    .Where(notification => notification.RelatedRequest.Id == approvedRequest.Id)
                    .ToList();
                dbContext.Notifications.RemoveRange(approvedNotifications);

                var newRental = new Rental
                {
                    Game = approvedRequest.Game,
                    Renter = approvedRequest.Renter,
                    Owner = approvedRequest.Owner,
                    StartDate = approvedRequest.StartDate,
                    EndDate = approvedRequest.EndDate
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

        private Account ResolveAccount(Account? account)
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

        private Game ResolveGame(Game? game)
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

        private Account? FindAccountById(Guid? accountId)
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
