using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using CoopQueue.Shared; // Dein Namespace für ServiceResponse

namespace CoopQueue.Api.Middleware
{
    /// <summary>
    /// Global middleware to intercept exceptions and convert them into standardized JSON responses.
    /// Handles security by hiding sensitive error details in production environments for server errors.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env; //to check if we are in production or development env

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ArgumentException or ArgumentNullException or FormatException => (int)HttpStatusCode.BadRequest,
                AuthenticationException => (int)HttpStatusCode.Unauthorized,
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
                KeyNotFoundException or FileNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;
            
            string message;
            
            if (statusCode == (int)HttpStatusCode.InternalServerError && !_env.IsDevelopment())
            {
                
                message = "An unexpected internal server error occurred. Please try again later.";
            }
            else
            {
                
                message = exception.Message;
            }

            var response = new ServiceResponse<string>
            {
                Success = false,
                Message = message
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}