namespace BoardRent.Domain
{
    using System;

    public class UserRole
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }
    }
}