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

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, profile.Id.ToString()),
                new(ClaimTypes.Name, profile.Username ?? string.Empty),
                new("DisplayName", profile.DisplayName ?? profile.Username ?? string.Empty),
            };

            if (profile.Role is not null && !string.IsNullOrWhiteSpace(profile.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, profile.Role.Name));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
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
    }
}
