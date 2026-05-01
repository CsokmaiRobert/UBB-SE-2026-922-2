namespace BoardRentAndProperty.Utilities
{
    using System;
    public interface ICurrentUserContext
    {
        Guid CurrentUserId { get; }
    }
}
