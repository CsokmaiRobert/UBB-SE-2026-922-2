namespace BoardRentAndProperty.Utilities
{
    using System;
    public sealed class CurrentUserContext : ICurrentUserContext
    {
        private readonly ISessionContext sessionContext;
        public CurrentUserContext(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
        }
        public Guid CurrentUserId => this.sessionContext.AccountId;
    }
}
