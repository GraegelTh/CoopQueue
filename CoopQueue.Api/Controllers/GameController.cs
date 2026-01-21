using CoopQueue.Shared.DTOs;
using CoopQueue.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CoopQueue.Shared.Enums;

namespace CoopQueue.Api.Controllers
{
    /// <summary>
    /// Manages the game queue, including adding, updating, voting, and picking games.
    /// handles user permissions (ownership/admin) for modification actions.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IIgdbService _igdbService;

        public GameController(IGameService gameService, IIgdbService igdbService)
        {
            _gameService = gameService;
            _igdbService = igdbService;
        }

        /// <summary>
        /// Retrieves all games currently in the queue, enriched with the current user's voting status.
        /// </summary>
        /// <returns>A list of games.</returns>
        [HttpGet]
        public async Task<ActionResult<List<GameResponseDto>>> GetAllGames()
        {
            var user = GetCurrentUser();
            var games = await _gameService.GetAllGamesAsync(user.Id);
            return Ok(games);
        }

        /// <summary>
        /// Searches for games using the external IGDB API.
        /// </summary>
        /// <param name="query">The name of the game to search for.</param>
        /// <returns>A list of search results from IGDB.</returns>
        [HttpGet("search/{query}")]
        public async Task<ActionResult<List<GameSearchDto>>> SearchGames(string query)
        {
            var searchResults = await _igdbService.SearchGameAsync(query);
            return Ok(searchResults);
        }

        /// <summary>
        /// Adds a new game to the queue. The current user is automatically assigned as the suggester.
        /// </summary>
        /// <param name="request">The game details.</param>
        /// <response code="200">Returns the updated list of games.</response>
        /// <response code="409">If the game already exists in the queue.</response>
        [HttpPost]
        public async Task<ActionResult<List<GameResponseDto>>> AddGame(CreateGameDto request)
        {
            var user = GetCurrentUser();
            
            var result = await _gameService.AddGameAsync(request, user.Name, user.Id);
            return Ok(result);
            
        }

        /// <summary>
        /// Updates the details of an existing game. Requires the user to be the owner or an Admin.
        /// </summary>
        /// <param name="id">The ID of the game to update.</param>
        /// <param name="request">The updated game data.</param>
        /// <response code="200">Returns the updated game list.</response>
        /// <response code="403">If the user is not authorized to edit this game.</response>
        /// <response code="404">If the game is not found.</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<List<GameResponseDto>>> UpdateGame(int id, CreateGameDto request)
        {
            var user = GetCurrentUser();
            
            var result = await _gameService.UpdateGameAsync(id, request, user.Id, user.Name, user.Role);

            if (result is null) return NotFound("Game not found.");
            return Ok(result);
           
        }

        /// <summary>
        /// Toggles a vote for a specific game by the current user.
        /// </summary>
        /// <param name="id">The ID of the game to upvote.</param>
        /// <response code="200">Returns the updated game list.</response>
        /// <response code="401">If the user identity cannot be verified.</response>
        /// <response code="404">If the game is not found.</response>
        /// <response code="409">If the voting logic fails (e.g., game state issues).</response>
        [HttpPut("{id}/upvote")]
        public async Task<ActionResult<List<GameResponseDto>>> UpvoteGame(int id)
        {
            var user = GetCurrentUser();
            if (user.Id == 0) return Unauthorized("User not recognized.");

            var result = await _gameService.UpvoteGameAsync(id, user.Id);
            if (result is null) return NotFound("Game not found.");
            return Ok(result);
            
        }

        /// <summary>
        /// Removes a game from the queue. Requires the user to be the owner or an Admin.
        /// </summary>
        /// <param name="id">The ID of the game to delete.</param>
        /// <response code="200">Returns the updated game list.</response>
        /// <response code="403">If the user is not authorized to delete this game.</response>
        /// <response code="404">If the game is not found.</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult<List<GameResponseDto>>> DeleteGame(int id)
        {
            var user = GetCurrentUser();            
            var result = await _gameService.DeleteGameAsync(id, user.Id, user.Name, user.Role);

            if (result is null) return NotFound("Game not found.");
            return Ok(result);
            
        }

        /// <summary>
        /// Selects the next game to play based on the specified selection mode.
        /// </summary>
        /// <param name="mode">The selection strategy: 'Democratic' or 'Weighted'. Defaults to Democratic.</param>
        /// <returns>The selected game.</returns>
        /// <response code="404">If there are no games with 'Suggestion' status available.</response>
        [HttpGet("pick")]
        public async Task<ActionResult<GameResponseDto>> PickNextGame([FromQuery] VotingMode mode = VotingMode.Democratic)
        {
            var winner = await _gameService.PickNextGameAsync(mode);
            if (winner == null) return NotFound("No games with status 'Suggestion' found in the queue.");
            return Ok(winner);
        }

        /// <summary>
        /// Helper method to extract user details (Id, Name, Role) from the current HttpContext claims.
        /// </summary>
        private (int Id, string Name, string Role) GetCurrentUser()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(idString, out int id);

            var name = User.Identity?.Name ?? "Unknown";
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";

            return (id, name, role);
        }
    }
}