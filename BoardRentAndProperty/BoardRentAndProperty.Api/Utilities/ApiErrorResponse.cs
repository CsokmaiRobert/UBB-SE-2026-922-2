namespace BoardRentAndProperty.Api.Utilities
{
    public sealed class ApiErrorResponse
    {
        public string Code { get; init; } = "request_failed";

        public string Error { get; init; } = "Request failed.";

        public int Status { get; init; } = 400;
    }
}
