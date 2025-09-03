using AuditService.Data;
using AuditService.Models;
using AuditService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.AuditModels;

namespace AuditService.Services;

public class AuditService : IAuditService
{
    private readonly AuditDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AuditDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AuditLog> LogEventAsync(AuditEvent auditEvent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = auditEvent.UserId,
                UserEmail = auditEvent.UserEmail,
                Action = auditEvent.Action,
                ResourceType = auditEvent.ResourceType,
                ResourceId = auditEvent.ResourceId,
                OldValues = auditEvent.GetOldValuesJson(),
                NewValues = auditEvent.GetNewValuesJson(),
                IpAddress = auditEvent.IpAddress,
                UserAgent = auditEvent.UserAgent,
                Success = auditEvent.Success,
                ErrorMessage = auditEvent.ErrorMessage,
                Metadata = auditEvent.GetMetadataJson(),
                ServiceName = auditEvent.ServiceName,
                RequestId = auditEvent.RequestId,
                SessionId = auditEvent.SessionId,
                CreatedAt = auditEvent.Timestamp.Kind == DateTimeKind.Utc ? auditEvent.Timestamp : DateTime.SpecifyKind(auditEvent.Timestamp, DateTimeKind.Utc)
            };

            // Append to event store first, then update read model in the same transaction
            using var tx = await _context.Database.BeginTransactionAsync();

            var currentVersion = 0;
            if (!string.IsNullOrEmpty(auditEvent.ResourceId))
            {
                currentVersion = await _context.AuditEvents
                    .Where(e => e.AggregateType == auditEvent.ResourceType && e.AggregateId == auditEvent.ResourceId)
                    .OrderByDescending(e => e.Version)
                    .Select(e => e.Version)
                    .FirstOrDefaultAsync();
            }

            var storedEvent = new StoredEvent
            {
                AggregateType = auditEvent.ResourceType,
                AggregateId = auditEvent.ResourceId,
                EventType = auditEvent.Action,
                EventData = auditEvent.GetNewValuesJson() ?? auditEvent.GetOldValuesJson() ?? "{}",
                Metadata = auditEvent.GetMetadataJson(),
                Version = currentVersion + 1,
                CreatedAt = auditLog.CreatedAt,
                CorrelationId = auditEvent.RequestId,
                CausationId = auditEvent.SessionId,
                UserId = auditEvent.UserId,
                UserEmail = auditEvent.UserEmail
            };

            _context.AuditEvents.Add(storedEvent);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Audit event logged successfully: {Action} on {ResourceType} by {UserId}", 
                auditEvent.Action, auditEvent.ResourceType, auditEvent.UserId);

            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit event: {Action} on {ResourceType}", 
                auditEvent.Action, auditEvent.ResourceType);
            throw;
        }
    }

    public async Task<IEnumerable<AuditLog>> LogBatchEventsAsync(IEnumerable<AuditEvent> auditEvents)
    {
        try
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            var auditLogs = auditEvents.Select(ae => new AuditLog
            {
                UserId = ae.UserId,
                UserEmail = ae.UserEmail,
                Action = ae.Action,
                ResourceType = ae.ResourceType,
                ResourceId = ae.ResourceId,
                OldValues = ae.GetOldValuesJson(),
                NewValues = ae.GetNewValuesJson(),
                IpAddress = ae.IpAddress,
                UserAgent = ae.UserAgent,
                Success = ae.Success,
                ErrorMessage = ae.ErrorMessage,
                Metadata = ae.GetMetadataJson(),
                ServiceName = ae.ServiceName,
                RequestId = ae.RequestId,
                SessionId = ae.SessionId,
                CreatedAt = ae.Timestamp.Kind == DateTimeKind.Utc ? ae.Timestamp : DateTime.SpecifyKind(ae.Timestamp, DateTimeKind.Utc)
            }).ToList();

            var pairs = auditEvents
                .Where(ae => !string.IsNullOrEmpty(ae.ResourceId))
                .Select(ae => new { ae.ResourceType, ae.ResourceId })
                .Distinct()
                .ToList();

            var versionMap = new Dictionary<(string, string), int>();
            foreach (var p in pairs)
            {
                var max = await _context.AuditEvents
                    .Where(e => e.AggregateType == p.ResourceType && e.AggregateId == p.ResourceId)
                    .OrderByDescending(e => e.Version)
                    .Select(e => e.Version)
                    .FirstOrDefaultAsync();
                versionMap[(p.ResourceType, p.ResourceId!)] = max;
            }

            var storedEvents = new List<StoredEvent>();
            foreach (var ae in auditEvents)
            {
                var createdAt = auditLogs.First(al => al.RequestId == ae.RequestId && al.Action == ae.Action && al.ResourceId == ae.ResourceId).CreatedAt;
                int nextVersion = 1;
                if (!string.IsNullOrEmpty(ae.ResourceId))
                {
                    var key = (ae.ResourceType, ae.ResourceId!);
                    if (!versionMap.ContainsKey(key)) versionMap[key] = 0;
                    versionMap[key] = versionMap[key] + 1;
                    nextVersion = versionMap[key];
                }

                storedEvents.Add(new StoredEvent
                {
                    AggregateType = ae.ResourceType,
                    AggregateId = ae.ResourceId,
                    EventType = ae.Action,
                    EventData = ae.GetNewValuesJson() ?? ae.GetOldValuesJson() ?? "{}",
                    Metadata = ae.GetMetadataJson(),
                    Version = nextVersion,
                    CreatedAt = createdAt,
                    CorrelationId = ae.RequestId,
                    CausationId = ae.SessionId,
                    UserId = ae.UserId,
                    UserEmail = ae.UserEmail
                });
            }

            _context.AuditEvents.AddRange(storedEvents);
            _context.AuditLogs.AddRange(auditLogs);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Batch audit events logged successfully: {Count} events", auditLogs.Count);

            return auditLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging batch audit events");
            throw;
        }
    }

    public async Task<StoredEvent> AppendEventAsync(string aggregateType, string? aggregateId, string eventType, object eventData, Dictionary<string, object>? metadata = null, string? correlationId = null, string? causationId = null, string? userId = null, string? userEmail = null)
    {
        var currentVersion = 0;
        if (!string.IsNullOrEmpty(aggregateId))
        {
            currentVersion = await _context.AuditEvents
                .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
                .OrderByDescending(e => e.Version)
                .Select(e => e.Version)
                .FirstOrDefaultAsync();
        }

        var stored = new StoredEvent
        {
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = eventType,
            EventData = System.Text.Json.JsonSerializer.Serialize(eventData),
            Metadata = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
            Version = currentVersion + 1,
            CreatedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            CausationId = causationId,
            UserId = userId,
            UserEmail = userEmail
        };

        _context.AuditEvents.Add(stored);
        await _context.SaveChangesAsync();
        return stored;
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string aggregateType, string aggregateId, int fromVersion = 0)
    {
        return await _context.AuditEvents
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId && e.Version >= fromVersion)
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsAsync(AuditQueryDto query)
    {
        try
        {
            var queryable = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(query.UserId))
                queryable = queryable.Where(a => a.UserId == query.UserId);

            if (!string.IsNullOrEmpty(query.Action))
                queryable = queryable.Where(a => a.Action == query.Action);

            if (!string.IsNullOrEmpty(query.ResourceType))
                queryable = queryable.Where(a => a.ResourceType == query.ResourceType);

            if (!string.IsNullOrEmpty(query.ServiceName))
                queryable = queryable.Where(a => a.ServiceName == query.ServiceName);

            if (query.FromDate.HasValue)
                queryable = queryable.Where(a => a.CreatedAt >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                queryable = queryable.Where(a => a.CreatedAt <= query.ToDate.Value);

            var totalCount = await queryable.CountAsync();

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                queryable = query.SortBy switch
                {
                    "UserId" => query.SortOrder == "desc" ? queryable.OrderByDescending(a => a.UserId) : queryable.OrderBy(a => a.UserId),
                    "Action" => query.SortOrder == "desc" ? queryable.OrderByDescending(a => a.Action) : queryable.OrderBy(a => a.Action),
                    "ResourceType" => query.SortOrder == "desc" ? queryable.OrderByDescending(a => a.ResourceType) : queryable.OrderBy(a => a.ResourceType),
                    "ServiceName" => query.SortOrder == "desc" ? queryable.OrderByDescending(a => a.ServiceName) : queryable.OrderBy(a => a.ServiceName),
                    _ => query.SortOrder == "desc" ? queryable.OrderByDescending(a => a.CreatedAt) : queryable.OrderBy(a => a.CreatedAt)
                };
            }
            else
            {
                queryable = query.SortOrder == "desc" ? queryable.OrderByDescending(a => a.CreatedAt) : queryable.OrderBy(a => a.CreatedAt);
            }

            var logs = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            throw;
        }
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(long id)
    {
        try
        {
            return await _context.AuditLogs.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log by ID: {Id}", id);
            throw;
        }
    }
        
    public async Task<int> CleanupOldLogsAsync(int retentionDays = 365)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var oldLogs = await _context.AuditLogs
                .Where(a => a.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.AuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} old audit logs older than {RetentionDays} days", 
                    oldLogs.Count, retentionDays);

                return oldLogs.Count;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old audit logs");
            throw;
        }
    }
}
