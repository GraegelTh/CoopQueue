using System.Security.Claims;
using CoopQueue.Api.Entities;
using CoopQueue.Api.Services;
using CoopQueue.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoopQueue.Api.Controllers
{
    /// <summary>
    /// Manages user authentication, including registration, login, and password management.
    /// Utilizes JWT (JSON Web Tokens) for secure stateless authentication.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user if a valid invite code is provided.
        /// </summary>
        /// <param name="request">The registration DTO containing username, password, and invite code.</param>
        /// <returns>The ID of the newly created user.</returns>
        /// <response code="200">Returns the User ID if registration is successful.</response>
        /// <response code="400">If the invite code is invalid or the request is malformed.</response>
        /// <response code="409">Username already exists.</response>
        [HttpPost("register")]
        public async Task<ActionResult<int>> Register(UserRegisterDto request)
        {
            var correctKey = _configuration["AppSettings:RegistrationKey"];
            if (request.InviteCode != correctKey)
            {
                return BadRequest("Invalid invite code! Access denied.");
            }

            var user = new User
            {
                Username = request.Username
            };

            var userId = await _authService.Register(user, request.Password);
            return Ok(userId);
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="request">Login credentials (username and password).</param>
        /// <returns>A session DTO containing the JWT token and user info.</returns>
        /// <response code="200">Returns the session details including the token.</response>
        /// <response code="400">If credentials are invalid.</response>
        [HttpPost("login")]
        public async Task<ActionResult<UserSessionDto>> Login(UserLoginDto request)
        {
            var result = await _authService.Login(request.Username, request.Password);
           
            if (result == "User not found" || result == "Wrong password")
            {
                return BadRequest("Invalid username or password.");
            }

            return Ok(new UserSessionDto
            {
                Username = request.Username,
                Token = result,
                Role = "See Token" // The actual role claims are embedded inside the encoded JWT
            });
        }

        /// <summary>
        /// Allows an authenticated user to change their password.
        /// </summary>
        /// <param name="request">DTO containing the old and new password.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">If the user ID cannot be parsed or the old password is incorrect.</response>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<string>> ChangePassword(UserChangePasswordDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Error retrieving user identity.");
            }

            var success = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

            if (!success)
            {
                return BadRequest("Old password is incorrect or user not found.");
            }

            return Ok("Password changed successfully.");
        }
    }
}