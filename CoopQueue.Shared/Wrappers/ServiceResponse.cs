namespace CoopQueue.Shared
{
    /// <summary>
    /// A generic wrapper for service responses to standardize API output.
    /// Allows sending data along with success/error status and messages.
    /// </summary>
    /// <typeparam name="T">The type of data being returned (e.g., UserDto, List<GameDto>).</typeparam>
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}