using Shared.AuditModels;

namespace Shared.Services;

public interface IAuditHelper
{
    Task LogEventAsync(AuditEvent auditEvent);
    
    Task LogBatchEventsAsync(IEnumerable<AuditEvent> auditEvents);
    
    Task LogEventAsync<T>(T auditEvent) where T : AuditEvent;
}
