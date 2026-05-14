using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;
using GUI_BRAP.Models;
using GUI_BRAP.ProxyServices;
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
            this.authProxyService = authProxyService ?? throw new ArgumentNullException(nameof(authProxyService));
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AccountProfileDataTransferObject profile;
            try
            {
                profile = await this.authProxyService.LoginAsync(new LoginDataTransferObject
                {
                    UsernameOrEmail = model.UsernameOrEmail,
                    Password = model.Password,
                    RememberMe = model.RememberMe,
                });
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            ClaimsIdentity identity = BuildIdentity(profile);
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Games");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await this.authProxyService.RegisterAsync(new RegisterDataTransferObject
                {
                    DisplayName = model.DisplayName,
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Country = model.Country ?? string.Empty,
                    City = model.City ?? string.Empty,
                    StreetName = model.StreetName ?? string.Empty,
                    StreetNumber = model.StreetNumber ?? string.Empty,
                });
            }
            catch (ProxyServiceException ex)
            {
                this.AddFieldErrors(ex.Message);
                return View(model);
            }

            AccountProfileDataTransferObject profile;
            try
            {
                profile = await this.authProxyService.LoginAsync(new LoginDataTransferObject
                {
                    UsernameOrEmail = model.Username,
                    Password = model.Password,
                });
            }
            catch (ProxyServiceException)
            {
                return RedirectToAction(nameof(Login));
            }

            ClaimsIdentity identity = BuildIdentity(profile);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });

            return RedirectToAction("Index", "Games");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            string message;
            try
            {
                message = await this.authProxyService.ForgotPasswordAsync();
            }
            catch (ProxyServiceException)
            {
                message = "Please contact the administrator at admin@boardrent.com.";
            }

            ViewData["Message"] = message;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await this.authProxyService.LogoutAsync();
            }
            catch (ProxyServiceException)
            {
            }

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

        private void AddFieldErrors(string errorMessage)
        {
            const int maximumSplitParts = 2;
            string[] errors = errorMessage.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (string error in errors)
            {
                string[] parts = error.Split('|', maximumSplitParts);
                if (parts.Length == maximumSplitParts)
                {
                    ModelState.AddModelError(parts[0].Trim(), parts[1].Trim());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Trim());
                }
            }
        }
    }
}
