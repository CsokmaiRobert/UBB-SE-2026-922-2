using System;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    internal static class ApiUrlHelper
    {
        public static string ToAbsoluteUrl(Uri apiBaseAddress, string relativeOrAbsoluteUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(relativeOrAbsoluteUrl, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            return new Uri(apiBaseAddress, relativeOrAbsoluteUrl).ToString();
        }

        public static void RebaseAvatarUrl(Uri apiBaseAddress, AccountProfileDataTransferObject profile)
        {
            if (profile == null)
            {
                return;
            }

            profile.AvatarUrl = ToAbsoluteUrl(apiBaseAddress, profile.AvatarUrl);
        }
    }
}
