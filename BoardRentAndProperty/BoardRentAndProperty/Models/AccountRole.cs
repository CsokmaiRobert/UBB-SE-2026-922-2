namespace BoardRentAndProperty.Models
{
    using System;

    public class AccountRole
    {
        public Guid AccountId { get; set; }

        public Account Account { get; set; } = default!;

        public Guid RoleId { get; set; }

        public Role Role { get; set; } = default!;
    }
}
