using System.Security.Claims;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Models.Auth;
using GUI_BRAP.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GUI_BRAP.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthProxyService authProxyService;

        public AuthController(IAuthProxyService authProxyService)
        {
            this.authProxyService = authProxyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            AccountProfileDataTransferObject profile;
            try
            {
                profile = await this.authProxyService.LoginAsync(new LoginDataTransferObject
                {
                    UsernameOrEmail = viewModel.UsernameOrEmail,
                    Password = viewModel.Password,
                    RememberMe = viewModel.RememberMe,
                });
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(viewModel);
            }

            ClaimsIdentity identity = BuildIdentity(profile);
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                IsPersistent = viewModel.RememberMe,
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
            {
                return Redirect(viewModel.ReturnUrl);
            }

            return RedirectToAction("Index", "Games");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private static ClaimsIdentity BuildIdentity(AccountProfileDataTransferObject profile)
        {
            ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, profile.Username ?? string.Empty));
            identity.AddClaim(new Claim("DisplayName", profile.DisplayName ?? string.Empty));

            string? roleName = profile.Role?.Name;
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }

            return identity;
        }
    }
}
