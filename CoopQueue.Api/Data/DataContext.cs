using CoopQueue.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoopQueue.Api.Data
{
    /// <summary>
    /// Represents the database session and acts as the bridge between the domain entities and the SQL Server database.
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        /// <summary>
        /// Table containing all suggested and active games.
        /// </summary>
        public DbSet<Game> Games { get; set; }

        /// <summary>
        /// Table containing registered users and their credentials.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Join table tracking which user voted for which game.
        /// </summary>
        public DbSet<GameVote> GameVotes { get; set; }
    }
}