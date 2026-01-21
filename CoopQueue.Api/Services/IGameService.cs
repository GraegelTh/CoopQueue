using CoopQueue.Shared.DTOs;
using CoopQueue.Shared.Enums;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Defines the contract for managing the game backlog, voting, and selection logic.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Retrieves all games from the database.
        /// </summary>
        /// <param name="userId">Optional User ID to determine if the current user has already voted on specific games.</param>
        /// <returns>A list of games sorted by votes.</returns>
        Task<List<GameResponseDto>> GetAllGamesAsync(int userId = 0);

        /// <summary>
        /// Adds a new game to the queue.
        /// </summary>
        /// <param name="request">The game details.</param>
        /// <param name="username">The name of the user adding the game.</param>
        /// <param name="userId">The ID of the user (for returning the updated list context).</param>
        /// <returns>The updated list of games.</returns>
        Task<List<GameResponseDto>> AddGameAsync(CreateGameDto request, string username, int userId);

        /// <summary>
        /// Updates an existing game's details.
        /// </summary>
        /// <param name="id">The ID of the game to update.</param>
        /// <param name="request">The new game data.</param>
        /// <param name="userId">The ID of the requesting user.</param>
        /// <param name="username">The name of the requesting user (for ownership check).</param>
        /// <param name="userRole">The role of the requesting user (for admin override).</param>
        /// <returns>The updated list of games, or null if the game was not found.</returns>
        Task<List<GameResponseDto>?> UpdateGameAsync(int id, CreateGameDto request, int userId, string username, string userRole);

        /// <summary>
        /// Registers a vote for a specific game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="userId">The ID of the user voting.</param>
        /// <returns>The updated list of games, or null if the game was not found.</returns>
        Task<List<GameResponseDto>?> UpvoteGameAsync(int gameId, int userId);

        /// <summary>
        /// Deletes a game from the queue.
        /// </summary>
        /// <param name="id">The ID of the game to delete.</param>
        /// <param name="userId">The ID of the requesting user.</param>
        /// <param name="username">The name of the requesting user (for ownership check).</param>
        /// <param name="userRole">The role of the requesting user (for admin override).</param>
        /// <returns>The updated list of games, or null if the game was not found.</returns>
        Task<List<GameResponseDto>?> DeleteGameAsync(int id, int userId, string username, string userRole);

        /// <summary>
        /// Selects a game to play based on the provided strategy.
        /// </summary>
        /// <param name="mode">The selection mode (e.g. "democratic", "random").</param>
        /// <returns>The winning game, or null if no candidates are available.</returns>
        Task<GameResponseDto?> PickNextGameAsync(VotingMode mode);
    }
}