namespace BoardRent.DataTransferObjects
{
    public class UpdateProfileDataTransferObject
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string Country { get; set; }
        public string City { get; set; }
        public string StreetName { get; set; }
        public string StreetNumber { get; set; }
    }
}
