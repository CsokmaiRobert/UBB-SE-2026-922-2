using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Infrastructure;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GUI_BRAP.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly IGameProxyService gameProxyService;

        public GamesController(IGameProxyService gameProxyService)
        {
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                IReadOnlyList<GameDTO> games = await this.gameProxyService.GetAllAsync();
                return View(games);
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Array.Empty<GameDTO>());
            }
        }
    }
}
