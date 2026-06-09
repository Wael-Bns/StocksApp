using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StocksApp.Core.Exceptions;

namespace StocksApp.WebApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch(ClientException ex)
            {
                _logger.LogWarning(ex, "A client exception occurred during the execution of the request.");
                
                await HandleExceptionAsync(httpContext, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during the execution of the request.");

                await HandleExceptionAsync(httpContext,
                    StatusCodes.Status500InternalServerError,
                    "An internal server error occurred.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext httpContext, int statusCode, string message)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = statusCode;
            ExceptionMessage errorRepsonse = new(message);
            var errorJson = JsonSerializer.Serialize(errorRepsonse);
            await httpContext.Response.WriteAsync(errorJson);
        }
        private record ExceptionMessage(string message);
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
