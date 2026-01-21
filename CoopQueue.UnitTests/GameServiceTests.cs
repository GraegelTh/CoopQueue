using Xunit;
using Microsoft.EntityFrameworkCore;
using CoopQueue.Api.Data;
using CoopQueue.Api.Services;
using CoopQueue.Api.Entities;
using CoopQueue.Shared.DTOs;
using CoopQueue.Shared.Enums;

namespace CoopQueue.UnitTests.Services
{
    /// <summary>
    /// Contains unit tests for the <see cref="GameService"/> class.
    /// Utilizes an In-Memory database to validate core business logic, including duplicate checks,
    /// voting mechanisms, and security permissions, without external dependencies.
    /// </summary>
    public class GameServiceTests
    {
        /// <summary>
        /// Helper method to create a fresh In-Memory database context for each test.
        /// This ensures test isolation so that data from one test does not affect another.
        /// </summary>
        private DataContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name guarantees a clean DB per call
                .Options;

            var context = new DataContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Verifies that the service prevents adding a game if it already exists in the database
        /// (based on the IGDB ID).
        /// </summary>
        [Fact]
        public async Task AddGame_ShouldThrowException_WhenGameAlreadyExists()
        {
            
            using var context = GetInMemoryContext();
            var service = new GameService(context);

            var existingGame = new Game
            {
                Title = "Elden Ring",
                IgdbId = 100,
                AddedByUser = "Admin",
                CoverUrl = "http://url.com" 
            };

            context.Games.Add(existingGame);
            await context.SaveChangesAsync();

            
            var duplicateRequest = new CreateGameDto
            {
                Title = "Elden Ring",
                IgdbId = 100,
                CoverUrl = "http://url.com"
            };

            
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddGameAsync(duplicateRequest, "User2", 2));
        }

        /// <summary>
        /// Ensures that a user's vote is correctly registered and increments the game's vote count.
        /// </summary>
        [Fact]
        public async Task UpvoteGame_ShouldIncreaseVoteCount_WhenUserHasNotVotedYet()
        {
           
            using var context = GetInMemoryContext();
            var service = new GameService(context);

            var game = new Game
            {
                Title = "Indie Game",
                Votes = 0,
                CoverUrl = "http://test.com",
                AddedByUser = "System"
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            
            await service.UpvoteGameAsync(game.Id, 99);

            
            var updatedGame = await context.Games.FindAsync(game.Id);
            Assert.Equal(1, updatedGame?.Votes);
        }

        /// <summary>
        /// Validates the security check ensuring that users cannot delete games they did not create,
        /// unless they are Admins.
        /// </summary>
        [Fact]
        public async Task DeleteGame_ShouldThrowUnauthorized_WhenUserIsNotOwnerOrAdmin()
        {
            
            using var context = GetInMemoryContext();
            var service = new GameService(context);

            var game = new Game
            {
                Title = "Admins Game",
                AddedByUser = "Admin",
                CoverUrl = "http://test.com"
            };

            context.Games.Add(game);
            await context.SaveChangesAsync();

            
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.DeleteGameAsync(game.Id, 99, "Hacker", "User"));
        }

        /// <summary>
        /// Tests the "Democratic" election mode.
        /// Ensures that the game with the absolute highest number of votes is selected.
        /// </summary>
        [Fact]
        public async Task PickNextGame_ShouldSelectMostVotedGame_WhenModeIsDemocratic()
        {            
            using var context = GetInMemoryContext();
            var service = new GameService(context);
            
            var winner = new Game { Title = "Winner Game", Votes = 10, Status = GameStatus.Suggestion, AddedByUser = "System", CoverUrl = "url" };
            var loser = new Game { Title = "Loser Game", Votes = 1, Status = GameStatus.Suggestion, AddedByUser = "System", CoverUrl = "url" };

            context.Games.AddRange(winner, loser);
            await context.SaveChangesAsync();
          
            var result = await service.PickNextGameAsync(VotingMode.Democratic);
            
            Assert.NotNull(result);
            Assert.Equal("Winner Game", result.Title);
            Assert.Equal(GameStatus.Playing, result.Status); 
        }
    }
}