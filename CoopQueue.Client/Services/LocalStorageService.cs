using Microsoft.JSInterop;

namespace CoopQueue.Client.Services
{
    /// <summary>
    /// Implementation of the local storage service using Blazor's JavaScript Interop.
    /// Acts as a bridge to access the browser's native 'localStorage' API.
    /// </summary>
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Invokes the JavaScript function 'localStorage.setItem' asynchronously.
        /// </summary>
        public async Task SetItemAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        /// <summary>
        /// Invokes the JavaScript function 'localStorage.getItem' asynchronously.
        /// </summary>
        /// <returns>The stored value, or null if the key does not exist.</returns>
        public async Task<string> GetItemAsync(string key)
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }

        /// <summary>
        /// Invokes the JavaScript function 'localStorage.removeItem' asynchronously.
        /// </summary>
        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}