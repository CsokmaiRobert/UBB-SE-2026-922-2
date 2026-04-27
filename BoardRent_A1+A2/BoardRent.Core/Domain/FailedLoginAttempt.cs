using System;

namespace BoardRent.Domain
{
    public class FailedLoginAttempt
    {
        public Guid UserId { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
