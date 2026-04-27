namespace BoardRentAndProperty.DataTransferObjects
{
    using System;

    public class RegisterDataTransferObject
    {
        public string DisplayName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string StreetName { get; set; }

        public string StreetNumber { get; set; }
    }
}
