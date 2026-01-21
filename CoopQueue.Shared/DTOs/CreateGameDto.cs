using CoopQueue.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace CoopQueue.Shared.DTOs
{
    public class CreateGameDto
    {
        [Required(ErrorMessage = "Please enter a title.")]
        [StringLength(100, ErrorMessage = "The title is too long (max 100 characters).")]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Description { get; set; }

        [StringLength(2048)]
        public string? CoverUrl { get; set; }

        public GameStatus Status { get; set; } = GameStatus.Suggestion;

        public long? IgdbId { get; set; }
        public long? SteamAppId { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}