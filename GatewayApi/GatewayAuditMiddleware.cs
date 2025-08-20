using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Shared.AuditModels;
using Shared.Services;

namespace GatewayApi;

public class GatewayAuditMiddleware
{
    private readonly RequestDelegate _next;
    public GatewayAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditHelper auditHelper)
    {
        var sw = Stopwatch.StartNew();
        string? userId = context.User?.FindFirst("sub")?.Value
                         ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        string? userEmail = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                            ?? context.User?.FindFirst("email")?.Value;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response?.StatusCode ?? 0;
            var path = context.Request.Path.ToString();
            var method = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            var eventData = new GatewayAuditEvent
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = "API_REQUEST",
                ResourceId = path,
                Success = statusCode < 400,
                ErrorMessage = statusCode >= 400 ? $"HTTP {statusCode}" : null,
                IpAddress = ip,
                UserAgent = userAgent,
                Metadata = new Dictionary<string, object>
                {
                    { "Path", path },
                    { "Method", method },
                    { "StatusCode", statusCode },
                    { "DurationMs", sw.ElapsedMilliseconds }
                },
                RequestId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            _ = Task.Run(async () =>
            {
                try { await auditHelper.LogEventAsync(eventData); } catch { }
            });
        }
    }
}

