using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty
{
    public sealed class CurrentUserContext : ICurrentUserContext
    {
        public int CurrentUserId { get; }

        public CurrentUserContext(int currentUserId)
        {
            this.CurrentUserId = currentUserId;
        }
    }
}
