namespace CoopQueue.Client.Services
{
    /// <summary>
    /// Interface for abstracting browser local storage access.
    /// Allows for dependency injection and easier unit testing (mocking) compared to direct IJSRuntime usage.
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        /// Persists a string value in the browser's local storage.
        /// </summary>
        /// <param name="key">The unique identifier for the item.</param>
        /// <param name="value">The value to store.</param>
        Task SetItemAsync(string key, string value);

        /// <summary>
        /// Retrieves a string value from local storage by its key.
        /// </summary>
        /// <returns>The stored string, or null/empty if not found.</returns>
        Task<string> GetItemAsync(string key);

        /// <summary>
        /// Removes a specific item from local storage.
        /// </summary>
        /// <param name="key">The identifier of the item to remove.</param>
        Task RemoveItemAsync(string key);
    }
}