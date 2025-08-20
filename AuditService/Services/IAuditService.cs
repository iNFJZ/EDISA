using AuditService.Models;
using Shared.AuditModels;

namespace AuditService.Services;

public interface IAuditService
{
    Task<AuditLog> LogEventAsync(AuditEvent auditEvent);
    
    Task<IEnumerable<AuditLog>> LogBatchEventsAsync(IEnumerable<AuditEvent> auditEvents);
    
    Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(AuditQueryDto query);
    
    Task<AuditLog?> GetAuditLogByIdAsync(long id);
    
    Task<int> CleanupOldLogsAsync(int retentionDays = 365);
}
