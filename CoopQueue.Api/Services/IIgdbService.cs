using CoopQueue.Shared.DTOs;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Defines the contract for searching and retrieving game metadata from external sources (IGDB).
    /// </summary>
    public interface IIgdbService
    {
        /// <summary>
        /// Searches for games matching the specified query string.
        /// </summary>
        /// <param name="query">The game title to search for.</param>
        /// <returns>A list of search results mapped to DTOs.</returns>
        Task<List<GameSearchDto>> SearchGameAsync(string query);
    }
}