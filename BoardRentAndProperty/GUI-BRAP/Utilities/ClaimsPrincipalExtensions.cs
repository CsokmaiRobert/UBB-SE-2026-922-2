using System;
using System.Security.Claims;

namespace GUI_BRAP.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        private const string AdministratorRoleName = "Administrator";

        public static Guid GetAccountId(this ClaimsPrincipal user)
        {
            string? rawId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawId, out Guid accountId) ? accountId : Guid.Empty;
        }

        public static string GetDisplayNameOrUsername(this ClaimsPrincipal user)
        {
            string? displayName = user?.FindFirstValue("DisplayName");
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return user?.Identity?.Name ?? string.Empty;
        }

        public static bool IsAdministrator(this ClaimsPrincipal user) =>
            user?.IsInRole(AdministratorRoleName) == true;
    }
}
