using System.Net;

namespace BoardRentAndProperty.ApiClient
{
    public class ServiceResult
    {
        public bool Success { get; init; }

        public string? Error { get; init; }

        public HttpStatusCode? StatusCode { get; init; }

        public string? ErrorCode { get; init; }

        public static ServiceResult Ok() => new() { Success = true };

        public static ServiceResult Fail(string error, HttpStatusCode? statusCode = null, string? errorCode = null)
            => new() { Success = false, Error = error, StatusCode = statusCode, ErrorCode = errorCode };
    }

    public sealed class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };

        public static new ServiceResult<T> Fail(string error, HttpStatusCode? statusCode = null, string? errorCode = null)
            => new() { Success = false, Error = error, StatusCode = statusCode, ErrorCode = errorCode };

        public static ServiceResult<T> Fail(ServiceResult failure)
            => new()
            {
                Success = false,
                Error = failure.Error,
                StatusCode = failure.StatusCode,
                ErrorCode = failure.ErrorCode,
            };
    }
}
