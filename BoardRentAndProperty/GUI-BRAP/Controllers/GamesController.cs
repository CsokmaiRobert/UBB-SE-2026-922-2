using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.Services;
using Microsoft.AspNetCore.Mvc;

namespace GUI_BRAP.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGameProxyService gameProxyService;

        public GamesController(IGameProxyService gameProxyService)
        {
            this.gameProxyService = gameProxyService;
        }

        public async Task<IActionResult> Index()
        {
            var games = await this.gameProxyService.GetAllGamesAsync();
            return View(games);
        }

        public async Task<IActionResult> Details(int id)
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new GameDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameDTO body)
        {
            if (!ModelState.IsValid)
            {
                return View(body);
            }

            try
            {
                await this.gameProxyService.CreateGameAsync(body);
                return RedirectToAction(nameof(Index));
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(body);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GameDTO body)
        {
            if (id != body.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(body);
            }

            try
            {
                await this.gameProxyService.UpdateGameAsync(id, body);
                return RedirectToAction(nameof(Index));
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(body);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await this.gameProxyService.DeleteGameAsync(id);
            }
            catch (ProxyServiceException ex)
            {
                TempData["DeleteError"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
