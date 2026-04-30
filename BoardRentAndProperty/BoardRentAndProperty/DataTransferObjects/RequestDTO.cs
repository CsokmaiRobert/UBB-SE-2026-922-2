using System;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;

namespace BoardRentAndProperty.DataTransferObjects
{
    public class RequestDTO : IDTO<Request>
    {
        private const string ShortDateDisplayFormat = "dd/MM";
        private const string LongDateDisplayFormat = "dd/MM/yyyy";
        private const string StartDateLabelPrefix = "Start: ";
        private const string EndDateLabelPrefix = "End: ";

        public int Id { get; set; }
        public GameDTO Game { get; set; }
        public Account Renter { get; set; }
        public Account Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Open;
        public Account? OfferingAccount { get; set; }

        public string StartDateDisplay => StartDate.ToString(ShortDateDisplayFormat);
        public string EndDateDisplay => EndDate.ToString(ShortDateDisplayFormat);
        public string StartDateDisplayLong => $"{StartDateLabelPrefix}{StartDate.ToString(LongDateDisplayFormat)}";
        public string EndDateDisplayLong => $"{EndDateLabelPrefix}{EndDate.ToString(LongDateDisplayFormat)}";
        public bool CanOffer => Status == RequestStatus.Open;

        public RequestDTO()
        {
        }
    }
}