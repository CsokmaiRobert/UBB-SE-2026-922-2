using System;

namespace GUI_BRAP.Services
{
    public sealed class ProxyServiceException : Exception
    {
        public int StatusCode { get; }

        public ProxyServiceException(string message, int statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }
    }
}
