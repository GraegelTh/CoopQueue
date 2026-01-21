using CoopQueue.Api.Data;
using CoopQueue.Shared.DTOs;
using CoopQueue.Shared.Enums;
using CoopQueue.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Service responsible for administrative user management tasks.
    /// Includes security safeguards to protect the Root Admin (ID 1) and prevent self-lockout.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all registered users mapped to safe DTOs.
        /// </summary>
        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role.ToString(),
                    DateCreated = u.DateCreated
                })
                .ToListAsync();
        }

        /// <summary>
        /// Deletes a specific user account.
        /// Contains safeguards to prevent deleting the Root Admin or oneself.
        /// </summary>
        public async Task<ServiceResponse<bool>> DeleteUserAsync(int userId, int currentUserId)
        {
            // Safeguard: The initial Root Admin (ID 1) cannot be deleted to ensure system access.
            if (userId == 1)
                return new ServiceResponse<bool> { Success = false, Message = "The Owner account cannot be deleted." };

            var user = await _context.Users.FindAsync(userId);

            if (user == null) return new ServiceResponse<bool> { Success = false, Message = "User not found." };

            // Safeguard: Prevent admins from accidentally deleting their own account.
            if (user.Id == currentUserId)
                return new ServiceResponse<bool> { Success = false, Message = "You cannot delete your own account." };

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true, Success = true, Message = "User successfully deleted." };
        }

        /// <summary>
        /// Toggles a user's role between Admin and User.
        /// Prevents modification of the Root Admin and self-demotion.
        /// </summary>
        public async Task<ServiceResponse<string>> ToggleRoleAsync(int userId, int currentUserId)
        {
            if (userId == 1)
                return new ServiceResponse<string> { Success = false, Message = "The Owner role cannot be modified." };

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new ServiceResponse<string> { Success = false, Message = "User not found." };

            // Safeguard: Prevent admins from revoking their own admin rights (self-lockout).
            if (user.Id == currentUserId)
                return new ServiceResponse<string> { Success = false, Message = "You cannot revoke your own admin rights." };

            user.Role = user.Role == UserRole.Admin ? UserRole.User : UserRole.Admin;
            await _context.SaveChangesAsync();

            return new ServiceResponse<string> { Data = user.Role.ToString(), Success = true };
        }

        /// <summary>
        /// Administratively resets a user's password.
        /// </summary>
        public async Task<ServiceResponse<bool>> ResetPasswordAsync(int userId, string newPassword)
        {
            if (userId == 1)
                return new ServiceResponse<bool> { Success = false, Message = "The Owner's password cannot be reset via this method." };

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new ServiceResponse<bool> { Success = false, Message = "User not found." };

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true, Success = true, Message = "Password reset successfully." };
        }

        /// <summary>
        /// Helper method to generate a salt and hash for password security.
        /// </summary>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}