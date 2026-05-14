using System;

namespace BoardRentAndProperty.Contracts.DataTransferObjects
{
    public class CreateRentalDataTransferObject
    {
        public int GameId { get; set; }
        public Guid RenterAccountId { get; set; }
        public Guid OwnerAccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
