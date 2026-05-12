using GUI_BRAP.Authorization;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GUI_BRAP.Controllers
{

    [Authorize(Roles = AppRoles.Administrator)]
    public class AdminController : Controller
    {
        private readonly IAdminProxyService _adminProxyService;

        public AdminController(IAdminProxyService adminProxyService)
        {
            _adminProxyService = adminProxyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accounts = await _adminProxyService.GetAllAccountsAsync();
            return View(accounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string id)
        {
            await _adminProxyService.SuspendAccountAsync(id);
            TempData["SuccessMessage"] = "Account successfully suspended.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsuspend(string id)
        {
            await _adminProxyService.UnsuspendAccountAsync(id);
            TempData["SuccessMessage"] = "Account successfully unsuspended.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Password cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            await _adminProxyService.ResetPasswordAsync(id, newPassword);
            TempData["SuccessMessage"] = "Password has been successfully reset.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            await _adminProxyService.UnlockAccountAsync(id);
            TempData["SuccessMessage"] = "Account has been successfully unlocked.";
            return RedirectToAction(nameof(Index));
        }
    }
}