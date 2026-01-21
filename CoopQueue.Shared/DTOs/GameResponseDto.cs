using CoopQueue.Shared.Enums;

namespace CoopQueue.Shared.DTOs
{
    public class GameResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public int Votes { get; set; }
        public GameStatus Status { get; set; }

        public long? SteamAppId { get; set; }
        public long? IgdbId { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public string AddedByUser { get; set; } = string.Empty;
        public bool IsVotedByCurrentUser { get; set; } = false;
    }
}
