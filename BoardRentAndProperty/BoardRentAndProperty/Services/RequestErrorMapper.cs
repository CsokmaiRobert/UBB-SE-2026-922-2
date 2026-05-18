using System;
using System.Net;
using BoardRentAndProperty.ApiClient;

namespace BoardRentAndProperty.Services
{
    public static class RequestErrorMapper
    {
        public static CreateRequestError MapCreate(ServiceResult failure)
        {
            return NormalizeCode(failure) switch
            {
                "owner_cannot_rent" => CreateRequestError.OwnerCannotRent,
                "dates_unavailable" => CreateRequestError.DatesUnavailable,
                "game_not_found" => CreateRequestError.GameDoesNotExist,
                "invalid_date_range" => CreateRequestError.InvalidDateRange,
                _ => ParseEnum(failure, CreateRequestError.InvalidDateRange),
            };
        }

        public static ApproveRequestError MapApprove(ServiceResult failure)
        {
            return failure.StatusCode switch
            {
                HttpStatusCode.NotFound => ApproveRequestError.NotFound,
                HttpStatusCode.Forbidden => ApproveRequestError.Unauthorized,
                _ => NormalizeCode(failure) == "request_transaction_failed"
                    ? ApproveRequestError.TransactionFailed
                    : ApproveRequestError.TransactionFailed,
            };
        }

        public static DenyRequestError MapDeny(ServiceResult failure)
        {
            return failure.StatusCode switch
            {
                HttpStatusCode.Forbidden => DenyRequestError.Unauthorized,
                HttpStatusCode.NotFound => DenyRequestError.NotFound,
                _ => DenyRequestError.NotFound,
            };
        }

        public static CancelRequestError MapCancel(ServiceResult failure)
        {
            return failure.StatusCode switch
            {
                HttpStatusCode.Forbidden => CancelRequestError.Unauthorized,
                HttpStatusCode.NotFound => CancelRequestError.NotFound,
                _ => CancelRequestError.NotFound,
            };
        }

        public static OfferError MapOffer(ServiceResult failure)
        {
            return NormalizeCode(failure) switch
            {
                "request_not_found" => OfferError.NotFound,
                "request_forbidden" => OfferError.NotOwner,
                "request_not_open" => OfferError.RequestNotOpen,
                "request_transaction_failed" => OfferError.TransactionFailed,
                _ => failure.StatusCode switch
                {
                    HttpStatusCode.NotFound => OfferError.NotFound,
                    HttpStatusCode.Forbidden => OfferError.NotOwner,
                    _ => OfferError.TransactionFailed,
                },
            };
        }

        private static TEnum ParseEnum<TEnum>(ServiceResult failure, TEnum fallback)
            where TEnum : struct, Enum
        {
            string? value = failure.ErrorCode ?? failure.Error;
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsedValue) ? parsedValue : fallback;
        }

        private static string NormalizeCode(ServiceResult failure) =>
            (failure.ErrorCode ?? string.Empty).Trim().ToLowerInvariant();
    }
}
