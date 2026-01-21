namespace CoopQueue.Shared.DTOs
{
    public class GameSearchDto
    {
        public long IgdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }

        // Currently only used for display/sorting
        public DateTime? ReleaseDate { get; set; }

        // Optional for Steam games
        public long? SteamAppId { get; set; }
    }
}
