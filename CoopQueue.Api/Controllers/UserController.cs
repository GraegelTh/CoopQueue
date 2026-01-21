using CoopQueue.Api.Services;
using CoopQueue.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoopQueue.Api.Controllers
{
    /// <summary>
    /// Manages user accounts, providing administrative functions like deleting users, 
    /// toggling roles, and resetting passwords.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a list of all registered users.
        /// </summary>
        /// <returns>A list of user DTOs containing public user information.</returns>
        [HttpGet]
        public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
        {
            return Ok(await _userService.GetAllUsersAsync());
        }

        /// <summary>
        /// Deletes a specific user account. Restricted to Administrators.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <response code="200">If the user was successfully deleted.</response>
        /// <response code="400">If the deletion failed (e.g., trying to delete oneself).</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Extract the current admin's ID to prevent self-deletion logic in the service
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _userService.DeleteUserAsync(id, currentUserId);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        /// <summary>
        /// Toggles a user's role between 'User' and 'Admin'. Restricted to Administrators.
        /// </summary>
        /// <param name="id">The ID of the user whose role should be toggled.</param>
        /// <response code="200">Returns the new role of the user.</response>
        /// <response code="400">If the operation failed (e.g. cannot strip admin rights from the last admin).</response>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleRole(int id)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _userService.ToggleRoleAsync(id, currentUserId);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        /// <summary>
        /// Resets a user's password to a new value provided by an Admin.
        /// </summary>
        /// <param name="id">The ID of the target user.</param>
        /// <param name="newPassword">The new password string.</param>
        /// <response code="200">Password reset successful.</response>
        /// <response code="400">If the operation failed.</response>
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
        {
            var result = await _userService.ResetPasswordAsync(id, newPassword);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }
    }
}