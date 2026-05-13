using System;
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
    public class RequestsController : Controller
    {
        private readonly IRequestProxyService requestProxyService;

        public RequestsController(IRequestProxyService requestProxyService)
        {
            this.requestProxyService = requestProxyService ?? throw new ArgumentNullException(nameof(requestProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Others()
        {
            Guid ownerAccountId = User.GetAccountId();
            var openRequests = await this.requestProxyService.GetOpenRequestsForOwnerAsync(ownerAccountId);
            return View(openRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Offer(int id)
        {
            Guid ownerAccountId = User.GetAccountId();

            try
            {
                await this.requestProxyService.OfferGameAsync(id, new RequestActionDataTransferObject
                {
                    AccountId = ownerAccountId,
                });
                TempData["SuccessMessage"] = "The request was approved and the rental was created.";
            }
            catch (ProxyServiceException proxyException)
            {
                TempData["ErrorMessage"] = proxyException.Message;
            }

            return RedirectToAction(nameof(Others));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id, string? reason)
        {
            Guid ownerAccountId = User.GetAccountId();

            try
            {
                await this.requestProxyService.DenyRequestAsync(id, new RequestActionDataTransferObject
                {
                    AccountId = ownerAccountId,
                    Reason = reason ?? string.Empty,
                });
                TempData["SuccessMessage"] = "The request was declined.";
            }
            catch (ProxyServiceException proxyException)
            {
                TempData["ErrorMessage"] = proxyException.Message;
            }

            return RedirectToAction(nameof(Others));
        }
    }
}
