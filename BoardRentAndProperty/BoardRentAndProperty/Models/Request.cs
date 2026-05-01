using System;
using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Models
{
    public class Request : IEntity
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public Account Renter { get; set; }
        public Account Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Open;
        public Account? OfferingAccount { get; set; }

        public Request()
        {
        }

        public Request(int id, Game requestedGame, Account renterUser, Account ownerUser, DateTime startDate, DateTime endDate,
                       RequestStatus status = RequestStatus.Open, Account? offeringAccount = null)
        {
            this.Id = id;
            Game = requestedGame;
            Renter = renterUser;
            Owner = ownerUser;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            OfferingAccount = offeringAccount;
        }
    }
}