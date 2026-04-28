namespace BoardRentAndProperty.Utilities
{
    public sealed class CurrentUserContext : ICurrentUserContext
    {
        private readonly ISessionContext sessionContext;

        public CurrentUserContext(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
        }

        public int CurrentUserId => this.sessionContext.PamUserId;
    }
}
