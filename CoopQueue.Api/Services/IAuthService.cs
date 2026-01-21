using CoopQueue.Api.Entities;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Defines the contract for user authentication, registration, and credential management.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the given password.
        /// </summary>
        /// <param name="user">The user entity containing basic info (e.g. Username).</param>
        /// <param name="password">The clear-text password to be hashed and stored.</param>
        /// <returns>The unique ID of the newly created user.</returns>
        Task<int> Register(User user, string password);

        /// <summary>
        /// Authenticates a user and generates a JWT token if credentials are valid.
        /// </summary>
        /// <param name="username">The username to look up.</param>
        /// <param name="password">The clear-text password to verify.</param>
        /// <returns>A JWT token string if successful, or an error message if failed.</returns>
        Task<string> Login(string username, string password);

        /// <summary>
        /// Checks if a specific username is already taken in the database.
        /// </summary>
        /// <returns>True if the user exists, otherwise False.</returns>
        Task<bool> UserExists(string username);

        /// <summary>
        /// Attempts to change a user's password, verifying the old password first.
        /// </summary>
        /// <returns>True if the change was successful, False if the old password was wrong.</returns>
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}