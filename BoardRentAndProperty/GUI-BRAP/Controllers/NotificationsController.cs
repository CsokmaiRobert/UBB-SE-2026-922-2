using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Authorization;
using GUI_BRAP.Infrastructure;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GUI_BRAP.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationProxyService notificationProxyService;

        public NotificationsController(INotificationProxyService notificationProxyService)
        {
            this.notificationProxyService = notificationProxyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            Guid accountId = User.GetAccountId();
            int pageSize = 3;

            try
            {
                var notifications = await this.notificationProxyService.GetNotificationsForUserAsync(accountId);

                var paginatedNotifications = notifications.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewData["CurrentPage"] = page;
                ViewData["TotalCount"] = notifications.Count;
                ViewData["PageSize"] = pageSize;

                return View(paginatedNotifications);
            }
            catch (ProxyServiceException exception)
            {
                TempData["ErrorMessage"] = "Could not load notifications at this time.";
                return View(Array.Empty<NotificationDTO>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await this.notificationProxyService.DeleteNotificationAsync(id);
                TempData["SuccessMessage"] = "Notification deleted successfully.";
            }
            catch (ProxyServiceException exception)
            {
                TempData["ErrorMessage"] = "Failed to delete notification. " + exception.Message;
            }

            return RedirectToAction(nameof(Index));
        }

    }
}