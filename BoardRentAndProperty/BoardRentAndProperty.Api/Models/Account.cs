using System;
using System.Collections.Generic;

namespace BoardRentAndProperty.Api.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public bool IsSuspended { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string StreetName { get; set; } = string.Empty;

        public string StreetNumber { get; set; } = string.Empty;

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
