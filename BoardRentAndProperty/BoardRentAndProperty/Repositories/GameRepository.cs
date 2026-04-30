using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Data;
using BoardRentAndProperty.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext dbContext;

        public GameRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private IQueryable<Game> GamesWithOwner => dbContext.Games.Include(game => game.Owner);

        public ImmutableList<Game> GetAll()
        {
            return GamesWithOwner.ToImmutableList();
        }

        public void Add(Game game)
        {
            game.Owner = ResolveAccount(game.Owner);
            dbContext.Games.Add(game);
            dbContext.SaveChanges();
            var saved = GamesWithOwner.FirstOrDefault(savedGame => savedGame.Id == game.Id);
            if (saved != null)
            {
                game.Owner = saved.Owner;
            }
        }

        public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId)
        {
            return GamesWithOwner.Where(game => game.Owner.Id == ownerAccountId).ToImmutableList();
        }

        public void Update(int id, Game updated)
        {
            var existing = GamesWithOwner.FirstOrDefault(game => game.Id == id);
            if (existing == null)
            {
                return;
            }
            if (updated.Owner != null)
            {
                existing.Owner = ResolveAccount(updated.Owner);
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
            var game = GamesWithOwner.FirstOrDefault(game => game.Id == id);
            if (game == null)
            {
                throw new KeyNotFoundException();
            }
            return game;
        }

        public Game Delete(int id)
        {
            var game = GamesWithOwner.FirstOrDefault(game => game.Id == id);
            if (game == null)
            {
                throw new KeyNotFoundException();
            }
            dbContext.Games.Remove(game);
            dbContext.SaveChanges();
            return game;
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
    }
}
