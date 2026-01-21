using CoopQueue.Api.Data;
using CoopQueue.Api.Entities;
using CoopQueue.Shared.DTOs;
using CoopQueue.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Service containing the core business logic for game management.
    /// Handles CRUD operations, voting logic, and the algorithm for picking the next game.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly DataContext _context;

        public GameService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all games and enriches them with the voting status of the current user.
        /// </summary>
        /// <param name="userId">The ID of the current user (0 if anonymous).</param>
        public async Task<List<GameResponseDto>> GetAllGamesAsync(int userId = 0)
        {
            var games = await _context.Games.ToListAsync();

            // Optimization: Load user votes into a HashSet for O(1) lookup performance
            var userVotedGameIds = new HashSet<int>();
            if (userId > 0)
            {
                var ids = await _context.GameVotes
                    .Where(v => v.UserId == userId)
                    .Select(v => v.GameId)
                    .ToListAsync();

                userVotedGameIds = new HashSet<int>(ids);
            }

            return games.OrderByDescending(g => g.Votes)
                .Select(g => new GameResponseDto
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    CoverUrl = g.CoverUrl,
                    Votes = g.Votes,
                    Status = g.Status,
                    SteamAppId = g.SteamAppId,
                    IgdbId = g.IgdbId,
                    ReleaseDate = g.ReleaseDate,
                    AddedByUser = g.AddedByUser ?? "Unknown",

                    // Determine if the requesting user has already upvoted this game
                    IsVotedByCurrentUser = userVotedGameIds.Contains(g.Id)
                })
                .ToList();
        }

        /// <summary>
        /// Adds a new game to the backlog, preventing duplicates based on IGDB ID.
        /// </summary>
        public async Task<List<GameResponseDto>> AddGameAsync(CreateGameDto request, string username, int userId)
        {
            // Duplicate check to keep the queue clean
            if (request.IgdbId.HasValue && await _context.Games.AnyAsync(g => g.IgdbId == request.IgdbId))
            {
                throw new InvalidOperationException($"'{request.Title}' is already on the list.");
            }

            var newGame = new Game
            {
                Title = request.Title,
                Description = request.Description,
                CoverUrl = request.CoverUrl,
                Status = request.Status,
                SteamAppId = request.SteamAppId,
                IgdbId = request.IgdbId,
                ReleaseDate = request.ReleaseDate,
                AddedByUser = username
            };

            _context.Games.Add(newGame);
            await _context.SaveChangesAsync();

            return await GetAllGamesAsync(userId);
        }

        /// <summary>
        /// Updates an existing game. Includes a security check to ensure only Admins or Owners can edit.
        /// </summary>
        public async Task<List<GameResponseDto>?> UpdateGameAsync(int id, CreateGameDto request, int userId, string username, string userRole)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null) return null;

            EnsureUserHasPermission(game, username, userRole);

            game.Title = request.Title;
            game.Description = request.Description;
            game.CoverUrl = request.CoverUrl;
            game.Status = request.Status;

            // Only update optional fields if they are provided
            if (request.SteamAppId.HasValue) game.SteamAppId = request.SteamAppId;
            if (request.IgdbId.HasValue) game.IgdbId = request.IgdbId;
            if (request.ReleaseDate.HasValue) game.ReleaseDate = request.ReleaseDate;

            await _context.SaveChangesAsync();

            return await GetAllGamesAsync(userId);
        }

        /// <summary>
        /// Registers a user's vote for a game. Enforces a "one vote per user per game" rule.
        /// </summary>
        public async Task<List<GameResponseDto>?> UpvoteGameAsync(int gameId, int userId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game is null) return null;

            var alreadyVoted = await _context.GameVotes.AnyAsync(v => v.GameId == gameId && v.UserId == userId);

            if (alreadyVoted)
            {
                throw new InvalidOperationException("You have already voted for this game.");
            }

            _context.GameVotes.Add(new GameVote
            {
                GameId = gameId,
                UserId = userId
            });

            game.Votes++;
            await _context.SaveChangesAsync();

            return await GetAllGamesAsync(userId);
        }

        /// <summary>
        /// Deletes a game from the queue. Protected by permission checks.
        /// </summary>
        public async Task<List<GameResponseDto>?> DeleteGameAsync(int id, int userId, string username, string userRole)
        {
            var game = await _context.Games.FindAsync(id);
            if (game is null) return null;

            EnsureUserHasPermission(game, username, userRole);

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return await GetAllGamesAsync(userId);
        }

        /// <summary>
        /// Selects the next game based on the chosen algorithm (Democratic vs. Weighted Random).
        /// </summary>
        /// <param name="mode">"democratic" for majority vote, otherwise weighted random.</param>
        public async Task<GameResponseDto?> PickNextGameAsync(VotingMode mode)
        {
            var candidates = await _context.Games
                .Where(g => g.Status == GameStatus.Suggestion)
                .ToListAsync();

            if (!candidates.Any()) return null;

            Game selectedGame = null!;

            if (mode == VotingMode.Democratic)
            {
                // Strategy: Absolute Majority. 
                // If there is a tie, pick a random winner among the top voted games.
                int maxVotes = candidates.Max(g => g.Votes);
                var topDogs = candidates.Where(g => g.Votes == maxVotes).ToList();
                selectedGame = topDogs[Random.Shared.Next(topDogs.Count)];
            }
            else
            {
                // Strategy: Weighted Random (Lottery).
                // Each vote grants one "ticket" in the lottery pot.
                // Even games with few votes have a small mathematical chance of winning.
                var lotteryPot = new List<Game>();
                foreach (var game in candidates)
                {
                    int tickets = game.Votes + 1; // Base chance of 1 + votes
                    for (int i = 0; i < tickets; i++) lotteryPot.Add(game);
                }
                selectedGame = lotteryPot[Random.Shared.Next(lotteryPot.Count)];
            }

            selectedGame.Status = GameStatus.Playing;
            await _context.SaveChangesAsync();

            return new GameResponseDto
            {
                Id = selectedGame.Id,
                Title = selectedGame.Title,
                Description = selectedGame.Description,
                CoverUrl = selectedGame.CoverUrl,
                Votes = selectedGame.Votes,
                Status = selectedGame.Status,
                SteamAppId = selectedGame.SteamAppId,
                IgdbId = selectedGame.IgdbId,
                ReleaseDate = selectedGame.ReleaseDate,
                AddedByUser = selectedGame.AddedByUser ?? "Unknown",
                IsVotedByCurrentUser = false
            };
        }

        /// <summary>
        /// Helper method to enforce RBAC (Role-Based Access Control) for modification actions.
        /// </summary>
        private void EnsureUserHasPermission(Game game, string username, string userRole)
        {
            bool isAdmin = userRole == UserRole.Admin.ToString();
            bool isOwner = game.AddedByUser == username;

            if (!isAdmin && !isOwner)
            {
                throw new UnauthorizedAccessException("You are only allowed to edit or delete your own suggestions.");
            }
        }
    }
}