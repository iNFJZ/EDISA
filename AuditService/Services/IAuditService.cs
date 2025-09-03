using AuditService.Models;
using System.Collections.Generic;
using Shared.AuditModels;

namespace AuditService.Services;

public interface IAuditService
{
    Task<AuditLog> LogEventAsync(AuditEvent auditEvent);
    
    Task<IEnumerable<AuditLog>> LogBatchEventsAsync(IEnumerable<AuditEvent> auditEvents);
    
    Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(AuditQueryDto query);
    
    Task<AuditLog?> GetAuditLogByIdAsync(long id);
    
    Task<int> CleanupOldLogsAsync(int retentionDays = 365);

    Task<StoredEvent> AppendEventAsync(string aggregateType, string? aggregateId, string eventType, object eventData, Dictionary<string, object>? metadata = null, string? correlationId = null, string? causationId = null, string? userId = null, string? userEmail = null);
    Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string aggregateType, string aggregateId, int fromVersion = 0);
}
