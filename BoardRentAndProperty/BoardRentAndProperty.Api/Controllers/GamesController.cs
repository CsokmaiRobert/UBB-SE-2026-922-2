using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Api.Utilities;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService gameService;

        public GamesController(IGameService gameService)
        {
            this.gameService = gameService;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<GameDTO>> GetAll()
        {
            return Ok(this.gameService.GetAllGames());
        }

        [HttpGet("{gameId:int}")]
        public ActionResult<GameDTO> GetById(int gameId)
        {
            try
            {
                return Ok(this.gameService.GetGameByIdentifier(gameId));
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Game not found.", "game_not_found");
            }
        }

        [HttpGet("owner/{ownerAccountId:guid}")]
        public ActionResult<IReadOnlyList<GameDTO>> GetByOwner(Guid ownerAccountId)
        {
            return Ok(this.gameService.GetGamesForOwner(ownerAccountId));
        }

        [HttpGet("owner/{ownerAccountId:guid}/active")]
        public ActionResult<IReadOnlyList<GameDTO>> GetActiveByOwner(Guid ownerAccountId)
        {
            return Ok(this.gameService.GetActiveGamesForOwner(ownerAccountId));
        }

        [HttpGet("renter/{renterAccountId:guid}/available")]
        public ActionResult<IReadOnlyList<GameDTO>> GetAvailableForRenter(Guid renterAccountId)
        {
            return Ok(this.gameService.GetAvailableGamesForRenter(renterAccountId));
        }

        [HttpPost]
        public IActionResult Create([FromBody] GameDTO body)
        {
            try
            {
                this.gameService.AddGame(body);
                return Ok();
            }
            catch (ArgumentException exception)
            {
                return this.ApiValidation(exception.Message, "game_validation_failed");
            }
        }

        [HttpPut("{gameId:int}")]
        public IActionResult Update(int gameId, [FromBody] GameDTO body)
        {
            try
            {
                this.gameService.UpdateGameByIdentifier(gameId, body);
                return NoContent();
            }
            catch (ArgumentException exception)
            {
                return this.ApiValidation(exception.Message, "game_validation_failed");
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Game not found.", "game_not_found");
            }
        }

        [HttpDelete("{gameId:int}")]
        public ActionResult<GameDTO> Delete(int gameId)
        {
            try
            {
                return Ok(this.gameService.DeleteGameByIdentifier(gameId));
            }
            catch (InvalidOperationException exception)
            {
                return this.ApiConflict(exception.Message, "game_delete_conflict");
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Game not found.", "game_not_found");
            }
        }
    }
}
