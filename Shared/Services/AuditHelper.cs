using Shared.AuditModels;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Shared.Services;

public class AuditHelper : IAuditHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditHelper> _logger;
    private readonly string _auditServiceUrl;

    public AuditHelper(HttpClient httpClient, ILogger<AuditHelper> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _auditServiceUrl = configuration["AuditService:Url"] ?? "http://audit-service:80";
    }

    public async Task LogEventAsync(AuditEvent auditEvent)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            
            // Tạo một object mới để serialize, xử lý Metadata đặc biệt
            var auditEventData = new
            {
                auditEvent.UserId,
                auditEvent.UserEmail,
                auditEvent.Action,
                auditEvent.ResourceType,
                auditEvent.ResourceId,
                auditEvent.OldValues,
                auditEvent.NewValues,
                auditEvent.IpAddress,
                auditEvent.UserAgent,
                auditEvent.Success,
                auditEvent.ErrorMessage,
                Metadata = auditEvent.Metadata != null ? auditEvent.Metadata : null,
                auditEvent.ServiceName,
                auditEvent.RequestId,
                auditEvent.SessionId,
                Timestamp = auditEvent.Timestamp
            };
            
            var json = JsonSerializer.Serialize(auditEventData, jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_auditServiceUrl}/api/audit/events", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to log audit event. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogDebug("Audit event logged successfully: {Action} on {ResourceType}", 
                    auditEvent.Action, auditEvent.ResourceType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit event: {Action} on {ResourceType}", 
                auditEvent.Action, auditEvent.ResourceType);
        }
    }

    public async Task LogBatchEventsAsync(IEnumerable<AuditEvent> auditEvents)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            
            // Tạo một list mới để serialize, xử lý Metadata đặc biệt
            var auditEventsData = auditEvents.Select(ae => new
            {
                ae.UserId,
                ae.UserEmail,
                ae.Action,
                ae.ResourceType,
                ae.ResourceId,
                ae.OldValues,
                ae.NewValues,
                ae.IpAddress,
                ae.UserAgent,
                ae.Success,
                ae.ErrorMessage,
                Metadata = ae.Metadata != null ? ae.Metadata : null,
                ae.ServiceName,
                ae.RequestId,
                ae.SessionId,
                Timestamp = ae.Timestamp
            }).ToList();
            
            var json = JsonSerializer.Serialize(auditEventsData, jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_auditServiceUrl}/api/audit/batch-events", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to log batch audit events. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
            }
            else
            {
                _logger.LogDebug("Batch audit events logged successfully: {Count} events", auditEvents.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging batch audit events");
        }
    }

    public async Task LogEventAsync<T>(T auditEvent) where T : AuditEvent
    {
        await LogEventAsync((AuditEvent)auditEvent);
    }
}
