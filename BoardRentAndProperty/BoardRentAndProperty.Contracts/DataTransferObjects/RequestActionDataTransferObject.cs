using System;

namespace BoardRentAndProperty.Contracts.DataTransferObjects
{
    public class RequestActionDataTransferObject
    {
        public Guid AccountId { get; set; }

        public string Reason { get; set; }
    }
}
