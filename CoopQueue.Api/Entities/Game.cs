using CoopQueue.Shared.Enums;

namespace CoopQueue.Api.Entities
{
    /// <summary>
    /// Represents a game entry in the database. 
    /// Stores both local app state (Votes, Status) and external metadata (IGDB ID, Steam ID).
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Unique identifier for the game entry.
        /// </summary>
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// URL to the cover image hosted by IGDB.
        /// </summary>
        public string? CoverUrl { get; set; }

        /// <summary>
        /// Optional Steam Application ID. Used to generate direct store links.
        /// </summary>
        public long? SteamAppId { get; set; }

        /// <summary>
        /// Current number of upvotes from the group.
        /// </summary>
        public int Votes { get; set; } = 0;

        /// <summary>
        /// The current workflow state of the game (e.g., Suggestion, Playing, Completed).
        /// </summary>
        public GameStatus Status { get; set; } = GameStatus.Suggestion;

        /// <summary>
        /// The unique ID from the external Internet Game Database (IGDB).
        /// </summary>
        public long? IgdbId { get; set; }

        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// The username of the person who suggested this game.
        /// </summary>
        public string AddedByUser { get; set; } = string.Empty;
    }
}