using System;
using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Models
{
    public class Rental : IEntity<int>
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public Account Renter { get; set; }
        public Account Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Rental()
        {
        }

        public Rental(int id, Game rentedGame, Account renterAccount, Account ownerAccount, DateTime startDate, DateTime endDate)
        {
            this.Id = id;
            Game = rentedGame;
            Renter = renterAccount;
            Owner = ownerAccount;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
