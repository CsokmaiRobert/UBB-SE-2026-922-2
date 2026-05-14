namespace BoardRentAndProperty.Api.Services
{
    internal static class CancelRequestErrorCodes
    {
        internal const int Unauthorized = -1;
        internal const int NotFound = -2;
    }

    public enum CreateRequestError
    {
        OwnerCannotRent,
        DatesUnavailable,
        GameDoesNotExist,
        InvalidDateRange,
    }

    public enum ApproveRequestError
    {
        Unauthorized,
        NotFound,
        TransactionFailed,
    }

    public enum DenyRequestError
    {
        Unauthorized,
        NotFound,
    }

    public enum OfferError
    {
        NotFound,
        NotOwner,
        RequestNotOpen,
        TransactionFailed,
    }

    public enum CancelRequestError
    {
        Unauthorized = CancelRequestErrorCodes.Unauthorized,
        NotFound = CancelRequestErrorCodes.NotFound,
    }
}
