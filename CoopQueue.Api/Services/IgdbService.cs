using CoopQueue.Shared.DTOs;
using IGDB;
using IGDB.Models;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Service responsible for communicating with the external Internet Game Database (IGDB) API.
    /// Handles searching, filtering, and mapping complex external data structures to internal DTOs.
    /// </summary>
    public class IgdbService : IIgdbService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IGDBClient _igdb;

        public IgdbService(IConfiguration config)
        {
            _clientId = config["Igdb:ClientId"] ?? string.Empty;
            _clientSecret = config["Igdb:ClientSecret"] ?? string.Empty;

            if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
                throw new Exception("IGDB Credentials missing in appsettings.json");

            _igdb = new IGDBClient(_clientId, _clientSecret);
        }

        /// <summary>
        /// Searches for games on IGDB based on a text query.
        /// Filters out irrelevant entries (DLCs, packs) and enriches results with Steam IDs and Cover URLs.
        /// </summary>
        /// <param name="searchName">The title to search for.</param>
        /// <returns>A list of simplified game DTOs.</returns>
        public async Task<List<GameSearchDto>> SearchGameAsync(string searchName)
        {
            // IGDB Query Syntax (Apicalypse):
            // Filter by Game Categories: 
            // 0 = Main Game, 2 = Expansion, 4 = Standalone Expansion, 8 = Remake, 9 = Remaster.
            // We request specific fields to minimize payload size.
            var query = $"search \"{searchName}\"; fields name, summary, first_release_date, cover.url, external_games.uid, external_games.url, external_games.external_game_source.id; where game_type = (0,2,4,8,9); limit 20;";

            var igdbGames = await _igdb.QueryAsync<Game>(IGDBClient.Endpoints.Games, query);
            var resultDtos = new List<GameSearchDto>();

            foreach (var g in igdbGames)
            {
                long? steamId = null;

                // Logic to extract Steam AppID from the list of external games
                if (g.ExternalGames != null && g.ExternalGames.Values != null)
                {
                    var steamEntry = g.ExternalGames.Values.FirstOrDefault(e =>
                        // Check 1: URL contains "steampowered"
                        (!string.IsNullOrEmpty(e.Url) && e.Url.Contains("steampowered", StringComparison.OrdinalIgnoreCase))
                        ||
                        // Check 2: External Source ID 1 is officially mapped to Steam by IGDB
                        (e.ExternalGameSource?.Id == 1)
                    );

                    if (steamEntry != null && long.TryParse(steamEntry.Uid, out long parsedId))
                    {
                        steamId = parsedId;
                    }
                }

                DateTime? releaseDate = g.FirstReleaseDate?.DateTime;

                // Image Processing:
                // IGDB returns 't_thumb' (90x90) by default. We replace it with 't_cover_big' (264x374) for better UI quality.
                // Also ensures the protocol is set to https.
                string? coverUrl = null;
                if (g.Cover != null && g.Cover.Value != null)
                {
                    coverUrl = g.Cover.Value.Url;
                    if (!string.IsNullOrEmpty(coverUrl))
                    {
                        if (coverUrl.StartsWith("//")) coverUrl = "https:" + coverUrl;
                        coverUrl = coverUrl.Replace("t_thumb", "t_cover_big");
                    }
                }

                resultDtos.Add(new GameSearchDto
                {
                    IgdbId = g.Id ?? 0,
                    Title = g.Name ?? "Unknown",
                    Description = g.Summary,
                    CoverUrl = coverUrl,
                    ReleaseDate = releaseDate,
                    SteamAppId = steamId
                });
            }

            return resultDtos;
        }
    }
}