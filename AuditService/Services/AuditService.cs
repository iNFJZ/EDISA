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

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

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

            _context.AuditLogs.AddRange(auditLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Batch audit events logged successfully: {Count} events", auditLogs.Count);

            return auditLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging batch audit events");
            throw;
        }
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
