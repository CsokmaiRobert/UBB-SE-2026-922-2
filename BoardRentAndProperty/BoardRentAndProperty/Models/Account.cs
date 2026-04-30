namespace BoardRentAndProperty.Models
{
    using System;
    using System.Collections.Generic;
    using BoardRentAndProperty.Mappers;

    public class Account : IEntity
    {
        public Guid Id { get; set; }

        int IEntity.Id
        {
            get => PamUserId ?? 0;
            set => PamUserId = value;
        }

        public string DisplayName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string PhoneNumber { get; set; }

        public string AvatarUrl { get; set; }

        public bool IsSuspended { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string StreetName { get; set; }

        public string StreetNumber { get; set; }

        public int? PamUserId { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}