using System.ComponentModel.DataAnnotations;

namespace CoopQueue.Shared.DTOs
{
    public class UserChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}