using System;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Authorization;
using GUI_BRAP.Infrastructure;
using GUI_BRAP.Models;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GUI_BRAP
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly IRequestProxyService requestProxyService;
        private readonly IGameProxyService gameProxyService;

        public RequestsController(IRequestProxyService requestProxyService, IGameProxyService gameProxyService)
        {
            this.requestProxyService = requestProxyService ?? throw new ArgumentNullException(nameof(requestProxyService));
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> My()
        {
            Guid renterAccountId = User.GetAccountId();

            try
            {
                var requests = await this.requestProxyService.GetRequestsForRenterAsync(renterAccountId);
                var sortedRequests = requests.OrderByDescending(request => request.StartDate).ToList();

                return View(new MyRequestsViewModel
                {
                    Requests = sortedRequests,
                });
            }
            catch (ProxyServiceException ex)
            {
                return View(new MyRequestsViewModel
                {
                    ErrorMessage = ex.Message,
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Guid renterAccountId = User.GetAccountId();

            try
            {
                var availableGames = await this.gameProxyService.GetAvailableGamesForRenterAsync(renterAccountId);

                return View(new CreateRequestViewModel
                {
                    AvailableGames = availableGames,
                });
            }
            catch (ProxyServiceException ex)
            {
                return View(new CreateRequestViewModel
                {
                    ErrorMessage = ex.Message,
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel form)
        {
            Guid renterAccountId = User.GetAccountId();

            var availableGames = await LoadAvailableGamesOrEmptyAsync(renterAccountId);

            if (!ModelState.IsValid)
            {
                return View(new CreateRequestViewModel
                {
                    GameId = form.GameId,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    AvailableGames = availableGames,
                });
            }

            GameDTO? selectedGame = availableGames.FirstOrDefault(game => game.Id == form.GameId);
            if (selectedGame is null)
            {
                ModelState.AddModelError(nameof(form.GameId), "The selected game is not available.");
                return View(new CreateRequestViewModel
                {
                    GameId = form.GameId,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    AvailableGames = availableGames,
                });
            }

            var body = new CreateRequestDataTransferObject
            {
                GameId = selectedGame.Id,
                RenterAccountId = renterAccountId,
                OwnerAccountId = selectedGame.Owner?.Id ?? Guid.Empty,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
            };

            try
            {
                await this.requestProxyService.CreateRequestAsync(body);
                return RedirectToAction(nameof(My));
            }
            catch (ProxyServiceException ex)
            {
                return View(new CreateRequestViewModel
                {
                    GameId = form.GameId,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    AvailableGames = availableGames,
                    ErrorMessage = ex.Message,
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int requestId)
        {
            Guid renterAccountId = User.GetAccountId();

            var body = new RequestActionDataTransferObject
            {
                AccountId = renterAccountId,
            };

            try
            {
                await this.requestProxyService.CancelRequestAsync(requestId, body);
            }
            catch (ProxyServiceException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(My));
        }

        private async Task<System.Collections.Generic.IReadOnlyList<GameDTO>> LoadAvailableGamesOrEmptyAsync(Guid renterAccountId)
        {
            try
            {
                return await this.gameProxyService.GetAvailableGamesForRenterAsync(renterAccountId);
            }
            catch (ProxyServiceException)
            {
                return new System.Collections.Generic.List<GameDTO>();
            }
        }
    }
}
