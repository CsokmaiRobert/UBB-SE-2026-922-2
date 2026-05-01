using System;

namespace BoardRentAndProperty.Models
{
    public class FailedLoginAttempt
    {
        public Guid AccountId { get; set; }
        public Account Account { get; set; } = default!;
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
