using CoopQueue.Shared.DTOs;

namespace CoopQueue.Client.Services
{
    /// <summary>
    /// Defines the contract for client-side authentication logic.
    /// Handles communication with the Auth Controller and local state management.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Attempts to log the user in via the API.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>Returns <c>null</c> if successful, otherwise returns the error message string.</returns>
        Task<string?> Login(UserLoginDto loginDto);

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        Task Register(UserRegisterDto registerDto);

        /// <summary>
        /// Logs the user out by clearing the local storage token and refreshing the auth state.
        /// </summary>
        Task Logout();

        /// <summary>
        /// Updates the current user's password.
        /// </summary>
        /// <returns>Returns <c>null</c> if successful, otherwise returns the error message string.</returns>
        Task<string?> ChangePassword(UserChangePasswordDto request);

        /// <summary>
        /// Validates whether the stored JWT token is still valid (not expired).
        /// Automatically triggers logout if the token has expired or is corrupted.
        /// </summary>
        /// <returns>True if the token is valid and active, otherwise false.</returns>
        Task<bool> IsTokenValid();
    }
}