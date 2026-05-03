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

            game.Owner = ResolveAccount(dbContext, game.Owner);
            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            var saved = GamesWithOwner(dbContext).FirstOrDefault(savedGame => savedGame.Id == game.Id);
            if (saved != null)
            {
                game.Owner = saved.Owner;
            }
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
    }
}
