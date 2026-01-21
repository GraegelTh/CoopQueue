using CoopQueue.Client.Extensions;
using CoopQueue.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;

namespace CoopQueue.Client.Services
{
    /// <summary>
    /// Client-side service handling user authentication.
    /// Manages API calls for login/register and synchronizes the local authentication state.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage; // Custom wrapper for browser local storage
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        /// <summary>
        /// Attempts to log the user in.
        /// On success: Saves the JWT token and notifies the application state.
        /// </summary>
        /// <returns>Null if successful, otherwise the error message.</returns>
        public async Task<string?> Login(UserLoginDto loginDto)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/login", loginDto);

            if (!response.IsSuccessStatusCode)
            {
                // Return the raw error message from the backend (e.g. "User not found")
                return await response.GetErrorMessageAsync();
            }

            var result = await response.Content.ReadFromJsonAsync<UserSessionDto>();

            if (result is null || result.Token is null)
            {
                return "Login failed: Empty response from server.";
            }

            // 1. Persist token
            await _localStorage.SetItemAsync("authToken", result.Token);

            // 2. Trigger state re-evaluation so the UI updates (e.g. show "Logout" button)
            await _authStateProvider.GetAuthenticationStateAsync();

            return null;
        }

        /// <summary>
        /// Registers a new user. Throws an exception if validation fails.
        /// </summary>
        public async Task Register(UserRegisterDto registerDto)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/register", registerDto);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.GetErrorMessageAsync();
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Logs the user out by removing the token and refreshing the state.
        /// </summary>
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");

            // Notify the app that the user is now anonymous
            await _authStateProvider.GetAuthenticationStateAsync();
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        /// <returns>Null if successful, otherwise the error message.</returns>
        public async Task<string?> ChangePassword(UserChangePasswordDto request)
        {
            var result = await _http.PostAsJsonAsync("api/auth/change-password", request);

            if (result.IsSuccessStatusCode)
            {
                return null; 
            }

            return await result.GetErrorMessageAsync(); 
        }

        /// <summary>
        /// Validates whether the stored JWT token is still valid (not expired).
        /// Automatically triggers logout if the token has expired or is corrupted.
        /// </summary>
        /// <returns>True if the token is valid and active, otherwise false.</returns>
        public async Task<bool> IsTokenValid()
        {
            var token = await _localStorage.GetItemAsync("authToken");

            // No token stored = user is not authenticated
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    
                    await Logout();
                    return false;
                }

                return true;
            }
            catch
            {
                
                await Logout();
                return false;
            }
        }
    }
}