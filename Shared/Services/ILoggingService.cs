using Microsoft.Extensions.Logging;

namespace Shared.Services;

public interface ILoggingService
{   
    void Debug(string message, params object[] propertyValues);
    void Information(string message, params object[] propertyValues);
    void Warning(string message, params object[] propertyValues);
    void Error(string message, Exception exception, params object[] propertyValues);
    void Error(string message, params object[] propertyValues);
    void Fatal(string message, Exception exception, params object[] propertyValues);
    void Fatal(string message, params object[] propertyValues);

    void LogBusinessOperation(string operation, string userId, string details, bool success, TimeSpan? duration = null);

    void LogSecurityEvent(string eventType, string userId, string details, string ipAddress = null, string userAgent = null);

    void LogPerformance(string operation, TimeSpan duration, string userId = null, string details = null);

    void LogSystemHealth(string component, string status, string details = null, Exception exception = null);

    void LogUserActivity(string activity, string userId, string details = null, string ipAddress = null);

    void LogApiCall(string method, string endpoint, string userId, int statusCode, TimeSpan duration, string details = null);
}
