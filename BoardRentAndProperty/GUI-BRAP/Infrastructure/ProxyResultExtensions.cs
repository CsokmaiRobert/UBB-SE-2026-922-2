using System.Net;
using BoardRentAndProperty.ApiClient;

namespace GUI_BRAP.Infrastructure
{
    public static class ProxyResultExtensions
    {
        public static void ThrowIfFailed(this ServiceResult result)
        {
            if (!result.Success)
            {
                throw ToException(result);
            }
        }

        public static T ThrowIfFailed<T>(this ServiceResult<T> result)
        {
            if (!result.Success)
            {
                throw ToException(result);
            }

            return result.Data!;
        }

        public static ProxyServiceException ToException(ServiceResult result)
            => new(
                result.Error ?? "API request failed.",
                result.StatusCode ?? HttpStatusCode.InternalServerError,
                result.ErrorCode);
    }
}
