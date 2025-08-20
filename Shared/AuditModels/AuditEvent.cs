using System.Text.Json;

namespace Shared.AuditModels;

public class AuditEvent
{
    public string? UserId { get; set; }
    
    public string? UserEmail { get; set; }
    
    public string Action { get; set; } = string.Empty;
    
    public string ResourceType { get; set; } = string.Empty;
    
    public string? ResourceId { get; set; }
    
    public object? OldValues { get; set; }
    
    public object? NewValues { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    public bool Success { get; set; } = true;
    
    public string? ErrorMessage { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
    
    public string ServiceName { get; set; } = string.Empty;
    
    public string? RequestId { get; set; }
    
    public string? SessionId { get; set; }
    
    private DateTime _timestamp = DateTime.UtcNow;
    public DateTime Timestamp 
    { 
        get => _timestamp;
        set => _timestamp = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    
    public AuditEvent()
    {
        _timestamp = DateTime.UtcNow;
    }
    
    public string? GetOldValuesJson()
    {
        return OldValues != null ? JsonSerializer.Serialize(OldValues) : null;
    }
    
    public string? GetNewValuesJson()
    {
        return NewValues != null ? JsonSerializer.Serialize(NewValues) : null;
    }
    
    public string? GetMetadataJson()
    {
        return Metadata != null ? JsonSerializer.Serialize(Metadata) : null;
    }
}
