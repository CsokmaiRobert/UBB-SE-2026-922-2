using System;

namespace BoardRentAndProperty.ApiClient
{
    public sealed class ApiClientOptions
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan? Timeout { get; set; }
    }
}
