using System;
using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Models
{
    public class Request : IEntity<int>
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public Account Renter { get; set; }
        public Account Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Open;
        public Account? OfferingUser { get; set; }

        public Request()
        {
        }

        public Request(int id, Game requestedGame, Account renterAccount, Account ownerAccount, DateTime startDate, DateTime endDate,
                       RequestStatus status = RequestStatus.Open, Account? offeringUser = null)
        {
            this.Id = id;
            Game = requestedGame;
            Renter = renterAccount;
            Owner = ownerAccount;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            OfferingUser = offeringUser;
        }
    }
}
