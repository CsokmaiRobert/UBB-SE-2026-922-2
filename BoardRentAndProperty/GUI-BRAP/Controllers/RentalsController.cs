using System;
using System.Threading.Tasks;
using GUI_BRAP.Authorization;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GUI_BRAP.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly IRentalProxyService rentalProxyService;

        public RentalsController(IRentalProxyService rentalProxyService)
        {
            this.rentalProxyService = rentalProxyService ?? throw new ArgumentNullException(nameof(rentalProxyService));
        }

        public async Task<IActionResult> My()
        {
            Guid renterId = User.GetAccountId();
            var rentals = await this.rentalProxyService.GetRentalsForRenterAsync(renterId);
            return View(rentals);
        }

        public async Task<IActionResult> Others()
        {
            Guid ownerId = User.GetAccountId();
            var rentals = await this.rentalProxyService.GetRentalsForOwnerAsync(ownerId);
            return View(rentals);
        }
    }
}
