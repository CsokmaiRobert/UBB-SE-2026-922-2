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
        private readonly AppDbContext dbContext;

        public RentalRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private IQueryable<Rental> RentalsWithNavigations =>
            dbContext.Rentals
                .Include(rental => rental.Game)
                .Include(rental => rental.Renter)
                .Include(rental => rental.Owner);

        public ImmutableList<Rental> GetAll() => RentalsWithNavigations.ToImmutableList();

        public void Add(Rental rental)
        {
            rental.Game = ResolveGame(rental.Game);
            rental.Renter = ResolveAccount(rental.Renter);
            rental.Owner = ResolveAccount(rental.Owner);
            dbContext.Rentals.Add(rental);
            dbContext.SaveChanges();
            var saved = RentalsWithNavigations.FirstOrDefault(savedRental => savedRental.Id == rental.Id);
            if (saved != null)
            {
                rental.Game = saved.Game;
                rental.Renter = saved.Renter;
                rental.Owner = saved.Owner;
            }
        }

        public void AddConfirmed(Rental rental) => Add(rental);

        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId) =>
            RentalsWithNavigations.Where(rental => rental.Owner.Id == ownerAccountId).ToImmutableList();

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId) =>
            RentalsWithNavigations.Where(rental => rental.Renter.Id == renterAccountId).ToImmutableList();

        public ImmutableList<Rental> GetRentalsByGame(int gameId) =>
            RentalsWithNavigations.Where(rental => rental.Game.Id == gameId).ToImmutableList();

        public Rental Delete(int id)
        {
            var rental = RentalsWithNavigations.FirstOrDefault(rental => rental.Id == id);
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
            var existing = RentalsWithNavigations.FirstOrDefault(rental => rental.Id == id);
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
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            dbContext.SaveChanges();
        }

        public Rental Get(int id)
        {
            var rental = RentalsWithNavigations.FirstOrDefault(rental => rental.Id == id);
            if (rental == null)
            {
                throw new KeyNotFoundException();
            }
            return rental;
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
    }
}
