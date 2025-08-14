using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shared.Services;
using System.Diagnostics;
using System.Text;

namespace Shared.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggingService loggingService)
        {
            _next = next;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            var userEmail = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var clientIp = GetClientIpAddress(context);

            // Log request
            _loggingService.Information("HTTP Request started - ID: {RequestId}, Method: {Method}, Path: {Path}, User: {UserId}, Email: {Email}, IP: {ClientIp}, UserAgent: {UserAgent}",
                requestId, context.Request.Method, context.Request.Path, userId, userEmail, clientIp, userAgent);

            try
            {
                // Add request ID to response headers
                context.Response.Headers.Add("X-Request-ID", requestId);

                // Process request
                await _next(context);

                stopwatch.Stop();

                // Log successful response
                _loggingService.Information("HTTP Request completed - ID: {RequestId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms, User: {UserId}",
                    requestId, context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, userId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log error response
                _loggingService.Error("HTTP Request failed - ID: {RequestId}, Method: {Method}, Path: {Path}, Duration: {Duration}ms, User: {UserId}, Error: {ErrorMessage}",
                    ex, requestId, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds, userId, ex.Message);

                // Re-throw to let other middleware handle it
                throw;
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp) && remoteIp != "::1")
            {
                return remoteIp;
            }

            return "127.0.0.1";
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
