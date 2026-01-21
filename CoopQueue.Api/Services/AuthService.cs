using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoopQueue.Api.Data;
using CoopQueue.Api.Entities;
using CoopQueue.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoopQueue.Api.Services
{
    /// <summary>
    /// Service responsible for handling user authentication, registration, password management, and JWT token generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user, hashes their password, and assigns an initial role.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <param name="password">The raw password to hash.</param>
        /// <returns>The ID of the created user.</returns>
        /// <exception cref="Exception">Thrown if the username is already taken.</exception>
        public async Task<int> Register(User user, string password)
        {
            if (await UserExists(user.Username))
                throw new Exception("Username is already taken.");

            // Logic: If no users exist yet, assign Admin role to this first user.
            // Otherwise, assign the default User role.
            if (!await _context.Users.AnyAsync())
            {
                user.Role = UserRole.Admin;
            }
            else
            {
                user.Role = UserRole.User;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        /// <summary>
        /// Verifies credentials and generates a JWT token if successful.
        /// </summary>
        /// <returns>A JWT string or an error message.</returns>
        public async Task<string> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));

            if (user == null) return "User not found";

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) return "Wrong password";

            return CreateToken(user);
        }

        /// <summary>
        /// Changes the password for a specific user after verifying the old password.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // 1. Verify old password
            if (!VerifyPasswordHash(oldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return false;
            }

            // 2. Hash and store new password
            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Checks if a user with the given username already exists.
        /// </summary>
        public async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower());
        }

        // --- HELPER METHODS ---

        /// <summary>
        /// Generates a salt and a hash for the given password using HMACSHA512.
        /// </summary>
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash and salt.
        /// </summary>
        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a JWT (JSON Web Token) containing user claims (Id, Name, Role).
        /// </summary>
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                // Important: Convert Enum to string so it can be stored in the token payload
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}