using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly ActivitySource _activitySource;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
            _activitySource = new ActivitySource("EDISA.LoggingService");
        }

        public void Debug(string message, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Debug");
            _logger.LogDebug(message, propertyValues);
        }

        public void Information(string message, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Information");
            _logger.LogInformation(message, propertyValues);
        }

        public void Warning(string message, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Warning");
            _logger.LogWarning(message, propertyValues);
        }

        public void Error(string message, Exception exception, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Error");
            _logger.LogError(exception, message, propertyValues);
        }

        public void Error(string message, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Error");
            _logger.LogError(message, propertyValues);
        }

        public void Fatal(string message, Exception exception, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Fatal");
            _logger.LogCritical(exception, message, propertyValues);
        }

        public void Fatal(string message, params object[] propertyValues)
        {
            using var activity = _activitySource.StartActivity("Fatal");
            _logger.LogCritical(message, propertyValues);
        }

        public void LogBusinessOperation(string operation, string userId, string details, bool success, TimeSpan? duration = null)
        {
            var message = "Business operation: {Operation}, User: {UserId}, Success: {Success}, Details: {Details}";
            var properties = new object[] { operation, userId, success, details };

            if (success)
            {
                if (duration.HasValue)
                {
                    message += ", Duration: {Duration}ms";
                    properties = new object[] { operation, userId, success, details, duration.Value.TotalMilliseconds };
                }
                Information(message, properties);
            }
            else
            {
                if (duration.HasValue)
                {
                    message += ", Duration: {Duration}ms";
                    properties = new object[] { operation, userId, success, details, duration.Value.TotalMilliseconds };
                }
                Warning(message, properties);
            }
        }

        public void LogSecurityEvent(string eventType, string userId, string details, string ipAddress = null, string userAgent = null)
        {
            var message = "Security event: {EventType}, User: {UserId}, Details: {Details}";
            var properties = new object[] { eventType, userId, details };

            if (!string.IsNullOrEmpty(ipAddress))
            {
                message += ", IP: {IPAddress}";
                properties = properties.Concat(new object[] { ipAddress }).ToArray();
            }

            if (!string.IsNullOrEmpty(userAgent))
            {
                message += ", UserAgent: {UserAgent}";
                properties = properties.Concat(new object[] { userAgent }).ToArray();
            }

            Warning(message, properties);
        }

        public void LogPerformance(string operation, TimeSpan duration, string userId = null, string details = null)
        {
            var message = "Performance: {Operation}, Duration: {Duration}ms";
            var properties = new object[] { operation, duration.TotalMilliseconds };

            if (!string.IsNullOrEmpty(userId))
            {
                message += ", User: {UserId}";
                properties = properties.Concat(new object[] { userId }).ToArray();
            }

            if (!string.IsNullOrEmpty(details))
            {
                message += ", Details: {Details}";
                properties = properties.Concat(new object[] { details }).ToArray();
            }

            if (duration.TotalMilliseconds > 1000)
            {
                Warning(message, properties);
            }
            else
            {
                Information(message, properties);
            }
        }

        public void LogSystemHealth(string component, string status, string details = null, Exception exception = null)
        {
            var message = "System health: {Component}, Status: {Status}";
            var properties = new object[] { component, status };

            if (!string.IsNullOrEmpty(details))
            {
                message += ", Details: {Details}";
                properties = properties.Concat(new object[] { details }).ToArray();
            }

            switch (status.ToLower())
            {
                case "healthy":
                case "ok":
                    Information(message, properties);
                    break;
                case "warning":
                case "degraded":
                    Warning(message, properties);
                    break;
                case "error":
                case "critical":
                case "down":
                    if (exception != null)
                    {
                        Error(message, exception, properties);
                    }
                    else
                    {
                        Error(message, properties);
                    }
                    break;
                default:
                    Information(message, properties);
                    break;
            }
        }

        public void LogUserActivity(string activity, string userId, string details = null, string ipAddress = null)
        {
            var message = "User activity: {Activity}, User: {UserId}";
            var properties = new object[] { activity, userId };

            if (!string.IsNullOrEmpty(details))
            {
                message += ", Details: {Details}";
                properties = properties.Concat(new object[] { details }).ToArray();
            }

            if (!string.IsNullOrEmpty(ipAddress))
            {
                message += ", IP: {IPAddress}";
                properties = properties.Concat(new object[] { ipAddress }).ToArray();
            }

            Information(message, properties);
        }

        public void LogApiCall(string method, string endpoint, string userId, int statusCode, TimeSpan duration, string details = null)
        {
            var message = "API call: {Method} {Endpoint}, User: {UserId}, Status: {StatusCode}, Duration: {Duration}ms";
            var properties = new object[] { method, endpoint, userId, statusCode, duration.TotalMilliseconds };

            if (!string.IsNullOrEmpty(details))
            {
                message += ", Details: {Details}";
                properties = properties.Concat(new object[] { details }).ToArray();
            }

            if (statusCode >= 400)
            {
                Warning(message, properties);
            }
            else if (duration.TotalMilliseconds > 1000)
            {
                Warning(message, properties);
            }
            else
            {
                Information(message, properties);
            }
        }
    }
}
