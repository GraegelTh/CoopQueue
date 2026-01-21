using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using CoopQueue.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace CoopQueue.Client.Auth
{
    /// <summary>
    /// Custom implementation of the AuthenticationStateProvider for Blazor WebAssembly.
    /// Manages the user's authentication state by reading JWT tokens from LocalStorage 
    /// and synchronizing them with the HttpClient headers.
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        /// <summary>
        /// Triggered by Blazor to determine the current user's authentication state.
        /// Checks for a cached token, validates it, and sets the default HTTP Authorization header.
        /// </summary>
        /// <returns>The AuthenticationState containing the user's claims (or an anonymous state).</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _localStorage.GetItemAsync("authToken");

            var identity = new ClaimsIdentity();
            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Parse claims from the token payload to rebuild the user identity on the client
                    identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");

                    // Attach the token to every outgoing HTTP request automatically
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                catch
                {
                    // If parsing fails (e.g., token corrupted), clear the storage and force logout
                    await _localStorage.RemoveItemAsync("authToken");
                    identity = new ClaimsIdentity();
                }
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            // Notify Blazor components (like <AuthorizeView>) that the state has changed
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        /// <summary>
        /// Manually parses the claims from the JWT payload (Base64Url encoded).
        /// This approach avoids adding heavy external dependencies for JWT handling in the WASM client.
        /// </summary>
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return (keyValuePairs ?? new Dictionary<string, object>()).Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
        }

        /// <summary>
        /// Helper to restore the padding ('=') required by C#'s standard Base64 decoder, 
        /// which is stripped in standard JWTs.
        /// </summary>
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}