using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Api.Data;
using BoardRentAndProperty.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Api.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public GameRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Game> GamesWithOwner(AppDbContext dbContext) =>
            dbContext.Games.Include(game => game.Owner);

        public ImmutableList<Game> GetAll()
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return GamesWithOwner(dbContext).ToImmutableList();
        }

        public void Add(Game game)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            if (game.Owner != null)
            {
                var owner = ResolveAccount(dbContext, game.Owner);
                // ensure FK uses existing PamUserId
                game.Owner = owner;
                game.OwnerId = owner.PamUserId;
            }

            dbContext.Games.Add(game);
            dbContext.SaveChanges();
        }

        public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return GamesWithOwner(dbContext)
                .Where(game => game.Owner != null && game.Owner.Id == ownerAccountId)
                .ToImmutableList();
        }

        public void Update(int id, Game updated)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = GamesWithOwner(dbContext).FirstOrDefault(game => game.Id == id);
            if (existing == null)
            {
                throw new KeyNotFoundException();
            }

            if (updated.Owner != null)
            {
                existing.Owner = ResolveAccount(dbContext, updated.Owner);
            }

            existing.Name = updated.Name;
            existing.Price = updated.Price;
            existing.MinimumPlayerNumber = updated.MinimumPlayerNumber;
            existing.MaximumPlayerNumber = updated.MaximumPlayerNumber;
            existing.Description = updated.Description;
            existing.Image = updated.Image;
            existing.IsActive = updated.IsActive;

            dbContext.SaveChanges();
        }

        public Game Get(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var game = GamesWithOwner(dbContext).FirstOrDefault(repositoryGame => repositoryGame.Id == id);
            if (game == null)
            {
                throw new KeyNotFoundException();
            }

            return game;
        }

        public Game Delete(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var game = GamesWithOwner(dbContext).FirstOrDefault(repositoryGame => repositoryGame.Id == id);
            if (game == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Games.Remove(game);
            dbContext.SaveChanges();
            return game;
        }

        private Account ResolveAccount(AppDbContext dbContext, Account account)
        {
            if (account == null) return null!;

            // Prefer PamUserId (non-nullable int). Treat 0 as not provided.
            if (account.PamUserId != 0)
            {
                var tracked = dbContext.Accounts.Local.FirstOrDefault(a => a.PamUserId == account.PamUserId)
                             ?? dbContext.Accounts.SingleOrDefault(a => a.PamUserId == account.PamUserId);
                if (tracked != null) return tracked;
                throw new InvalidOperationException($"Account with PamUserId {account.PamUserId} was not found.");
            }

            // Fallback to GUID Id
            if (account.Id != Guid.Empty)
            {
                var tracked = dbContext.Accounts.Local.FirstOrDefault(a => a.Id == account.Id)
                             ?? dbContext.Accounts.SingleOrDefault(a => a.Id == account.Id);
                if (tracked != null) return tracked;
                throw new InvalidOperationException($"Account with Id {account.Id} was not found.");
            }

            throw new InvalidOperationException("Owner must include a valid PamUserId or Id.");
        }
    }
}
