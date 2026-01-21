using CoopQueue.Shared.DTOs;
using CoopQueue.Shared;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Defines the contract for user management operations.
    /// Typically used by Admins to manage other users (roles, deletion, password resets).
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a list of all registered users.
        /// </summary>
        /// <returns>A list of public user information.</returns>
        Task<List<UserResponseDto>> GetAllUsersAsync();

        /// <summary>
        /// Attempts to delete a user.
        /// Includes validation to prevent a user from deleting themselves.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="currentUserId">The ID of the admin performing the action (for self-check).</param>
        /// <returns>A response indicating success or failure message.</returns>
        Task<ServiceResponse<bool>> DeleteUserAsync(int userId, int currentUserId);

        /// <summary>
        /// Toggles a user's role between 'User' and 'Admin'.
        /// </summary>
        /// <param name="userId">The ID of the target user.</param>
        /// <param name="currentUserId">The ID of the admin performing the action (for self-check).</param>
        /// <returns>A response containing the new role string or an error message.</returns>
        Task<ServiceResponse<string>> ToggleRoleAsync(int userId, int currentUserId);

        /// <summary>
        /// Resets a user's password to a specific value (Admin override).
        /// </summary>
        /// <param name="userId">The ID of the target user.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>A response indicating success.</returns>
        Task<ServiceResponse<bool>> ResetPasswordAsync(int userId, string newPassword);
    }
}