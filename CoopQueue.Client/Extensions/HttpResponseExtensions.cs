using System.Net.Http.Json;
using CoopQueue.Shared;

namespace CoopQueue.Client.Extensions
{
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Extensions method to safely extract user-friendly error messages from API responses.
        /// Handles typed JSON responses (ServiceResponse) as well as raw HTML/Text fallbacks.
        /// </summary>
        public static async Task<string> GetErrorMessageAsync(this HttpResponseMessage response)
        {
            // 1. Try to parse our standardized JSON error wrapper
            try
            {
                // We use <object> or <string> generic because we only care about the 'Message' property
                var errorObj = await response.Content.ReadFromJsonAsync<ServiceResponse<object>>();

                if (!string.IsNullOrWhiteSpace(errorObj?.Message))
                {
                    return errorObj.Message;
                }
            }
            catch
            {
                // JSON parsing failed (e.g. Server returned 502 Bad Gateway HTML)
                // We swallow this exception and try the fallback.
            }

            // 2. Fallback: Read raw string content
            try
            {
                var rawText = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(rawText))
                {
                    // Truncate huge error dumps (like SQL stack traces in HTML) to keep UI clean
                    return rawText.Length > 200
                        ? $"Server error: {rawText.Substring(0, 197)}..."
                        : rawText;
                }
            }
            catch { /* Ignore read errors */ }

            // 3. Default fallback if response is empty
            return $"An unexpected error occurred (Status: {response.StatusCode}).";
        }
    }
}