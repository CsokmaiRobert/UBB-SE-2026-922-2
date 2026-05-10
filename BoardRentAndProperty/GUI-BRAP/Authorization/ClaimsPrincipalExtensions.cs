using System;
using System.Security.Claims;

namespace GUI_BRAP.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetAccountId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string? raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out Guid accountId))
            {
                throw new InvalidOperationException("Account id claim is missing or invalid.");
            }

            return accountId;
        }

        public static bool TryGetAccountId(this ClaimsPrincipal user, out Guid accountId)
        {
            accountId = Guid.Empty;
            string? raw = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out accountId);
        }

        public static string GetDisplayNameOrUsername(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string? displayName = user.FindFirstValue("DisplayName");
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName!;
            }

            string? userName = user.FindFirstValue(ClaimTypes.Name);
            return string.IsNullOrWhiteSpace(userName) ? string.Empty : userName!;
        }

        public static string? GetRoleName(this ClaimsPrincipal user)
        {
            return user?.FindFirstValue(ClaimTypes.Role);
        }
    }
}
