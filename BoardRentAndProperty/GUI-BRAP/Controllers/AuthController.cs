using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Models;
using GUI_BRAP.ProxyServices;
using GUI_BRAP.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GUI_BRAP.Services;

namespace GUI_BRAP.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthProxyService authProxyService;

        public AuthController(IAuthProxyService authProxyService)
        {
            this.authProxyService = authProxyService ?? throw new ArgumentNullException(nameof(authProxyService));
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
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
