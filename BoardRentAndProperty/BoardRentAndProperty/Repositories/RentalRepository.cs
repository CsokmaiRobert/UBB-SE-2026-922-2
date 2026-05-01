using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Data;
using BoardRentAndProperty.Models;
using Microsoft.EntityFrameworkCore;

namespace BoardRentAndProperty.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly IDbContextFactory<AppDbContext> dbContextFactory;

        public RentalRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        private static IQueryable<Rental> RentalsWithNavigations(AppDbContext dbContext) =>
            dbContext.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Renter)
                .Include(rental => rental.Owner);

        public ImmutableList<Rental> GetAll()
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext).ToImmutableList();
        }

        public void Add(Rental rental)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            rental.Game = ResolveGame(dbContext, rental.Game);
            rental.Renter = ResolveAccount(dbContext, rental.Renter);
            rental.Owner = ResolveAccount(dbContext, rental.Owner);
            dbContext.Rentals.Add(rental);
            dbContext.SaveChanges();

            var saved = RentalsWithNavigations(dbContext).FirstOrDefault(savedRental => savedRental.Id == rental.Id);
            if (saved != null)
            {
                rental.Game = saved.Game;
                rental.Renter = saved.Renter;
                rental.Owner = saved.Owner;
            }
        }

        public void AddConfirmed(Rental rental) => Add(rental);

        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext).Where(rental => rental.Owner.Id == ownerAccountId).ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext).Where(rental => rental.Renter.Id == renterAccountId).ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            return RentalsWithNavigations(dbContext).Where(rental => rental.Game.Id == gameId).ToImmutableList();
        }

        public Rental Delete(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var rental = RentalsWithNavigations(dbContext).FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            dbContext.Rentals.Remove(rental);
            dbContext.SaveChanges();
            return rental;
        }

        public void Update(int id, Rental updated)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var existing = RentalsWithNavigations(dbContext).FirstOrDefault(rental => rental.Id == id);
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

            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            dbContext.SaveChanges();
        }

        public Rental Get(int id)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();
            var rental = RentalsWithNavigations(dbContext).FirstOrDefault(repositoryRental => repositoryRental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }

            return rental;
        }

        private static Account ResolveAccount(AppDbContext dbContext, Account? account)
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

        private static Game ResolveGame(AppDbContext dbContext, Game? game)
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
    }
}
