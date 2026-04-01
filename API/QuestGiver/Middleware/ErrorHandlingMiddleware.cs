using QuestGiver.Models.Send;
using System.Net;
using System.Text.Json;

namespace QuestGiver.Middleware
{
    /// <summary>
    /// Global error handling middleware that catches and processes all unhandled exceptions
    /// throughout the application, providing consistent error responses to clients.
    /// </summary>
    /// <remarks>
    /// This middleware:
    /// - Catches all exceptions at the application level
    /// - Maps exceptions to appropriate HTTP status codes
    /// - Returns standardized error responses as JSON
    /// - Logs errors for debugging and monitoring
    /// - Prevents sensitive error details from leaking to clients in production
    /// </remarks>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the ErrorHandlingMiddleware class.
        /// </summary>
        /// <param name="next">The next middleware in the request pipeline.</param>
        /// <param name="logger">Logger instance for recording error information.</param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware, wrapping the next middleware in a try-catch block
        /// to handle any exceptions that occur during request processing.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task that completes when the middleware processing is complete.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass the request to the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception with full context information
                _logger.LogError(ex, "An unhandled exception occurred: {ExceptionMessage}", ex.Message);

                // Handle the exception and generate an appropriate response
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Processes an exception and writes an appropriate error response to the HTTP context.
        /// Maps specific exception types to corresponding HTTP status codes and error details.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="exception">The exception that was caught.</param>
        /// <returns>A task that completes when the response has been written.</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Ensure the response hasn't already started being written
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }

            // Create the error response object with default values
            var response = new ErrorResponse();

            // Set response content type to JSON
            context.Response.ContentType = "application/json";

            // Map exception type to HTTP status code and populate error details
            switch (exception)
            {
                // 400 Bad Request - Null argument provided where value is required
                case ArgumentNullException argNullEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"A required value was null: {argNullEx.ParamName}";
                    response.ErrorType = "ArgumentNullException";
                    response.Details = "One or more required fields are missing or empty.";
                    break;

                // 400 Bad Request - Invalid input or argument validation failures
                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = argEx.Message;
                    response.ErrorType = "ArgumentException";
                    response.Details = "The request contains invalid arguments or failed validation checks.";
                    break;
    
                // 404 Not Found - Resource not found in database or collection
                case KeyNotFoundException keyEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = keyEx.Message;
                    response.ErrorType = "KeyNotFoundException";
                    response.Details = "The requested resource could not be found.";
                    break;

                // 401 Unauthorized - User lacks authentication or authorization
                case UnauthorizedAccessException unAuthEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = unAuthEx.Message;
                    response.ErrorType = "UnauthorizedAccessException";
                    response.Details = "You do not have permission to access this resource.";
                    break;

                // 409 Conflict - Operation violates business rules or data integrity
                case InvalidOperationException invOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = invOpEx.Message;
                    response.ErrorType = "InvalidOperationException";
                    response.Details = "The operation could not be completed due to a conflict.";
                    break;

                // 503 Service Unavailable - External API or service failure
                case HttpRequestException httpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    response.Message = "An external service is currently unavailable.";
                    response.ErrorType = "ServiceUnavailableException";
                    response.Details = "Please try again later. If the problem persists, contact support.";
                    break;

                // 500 Internal Server Error - OpenAI API errors (covers various OpenAI exceptions)
                case Exception ex when ex.GetType().Namespace?.StartsWith("OpenAI") == true:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Failed to generate content from AI service.";
                    response.ErrorType = "AIServiceException";
                    response.Details = "An error occurred while processing your request with our AI service. Please try again.";
                    break;

                // 500 Internal Server Error - Database operation errors
                case Microsoft.EntityFrameworkCore.DbUpdateException dbEx:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "A database error occurred while processing your request.";
                    response.ErrorType = "DatabaseException";
                    response.Details = "The operation could not be completed. Please try again or contact support.";
                    break;

                // 500 Internal Server Error - JSON serialization/deserialization failures
                case JsonException jsonEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid JSON format in request body.";
                    response.ErrorType = "JsonException";
                    response.Details = "The request body contains invalid JSON. Please check the format and try again.";
                    break;

                // 500 Internal Server Error - Catch-all for unexpected exceptions
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An unexpected error occurred. Please contact support.";
                    response.ErrorType = exception.GetType().Name;
                    response.Details = "An unhandled error has been logged and the support team has been notified.";
                    break;
            }

            // Set timestamp of when the error occurred
            response.Timestamp = DateTime.UtcNow;

            // Serialize the error response to JSON and write to response body
            return context.Response.WriteAsJsonAsync(response);
        }
    }

    /// <summary>
    /// Standardized error response model used for all API error responses.
    /// Provides consistent structure for error information returned to clients.
    /// </summary>
    /// <remarks>
    /// This model ensures that all error responses follow the same format, making it easier
    /// for clients to parse and handle error information.
    /// </remarks>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP status code of the error response.
        /// </summary>
        /// <example>400, 401, 404, 500</example>
        public int StatusCode { get; set; }

        /// <summary>
        /// Primary error message describing what went wrong.
        /// Should be user-friendly and concise.
        /// </summary>
        /// <example>"A user with the same email already exists."</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The full name of the exception type that was caught.
        /// Useful for debugging and logging purposes.
        /// </summary>
        /// <example>"ArgumentException", "KeyNotFoundException"</example>
        public string ErrorType { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the error, providing context for the client.
        /// More general than Message; suitable for displaying to end users.
        /// </summary>
        /// <example>"The requested resource could not be found."</example>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp indicating when the error occurred.
        /// Useful for error tracking and correlation in logs.
        /// </summary>
        /// <example>2026-04-01T10:30:45.123Z</example>
        public DateTime Timestamp { get; set; }
    }
}