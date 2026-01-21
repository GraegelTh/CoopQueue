using CoopQueue.Shared.Enums;

namespace CoopQueue.Api.Entities
{
    /// <summary>
    /// Represents a registered user in the system.
    /// Includes security credentials (hash/salt) and role-based access information.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The hashed version of the user's password.
        /// Raw passwords are never stored in the database.
        /// </summary>
        public byte[] PasswordHash { get; set; } = default!;

        /// <summary>
        /// The cryptographic salt used to hash the password.
        /// Ensures unique hashes even if two users share the same password.
        /// </summary>
        public byte[] PasswordSalt { get; set; } = default!;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The user's role (e.g., Admin, User) used for authorization checks.
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;
    }
}