using AuditService.Protos;
using AuditService.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.AuditModels;
using System.Text.Json;

namespace AuditService.Services;

public class AuditGrpcServiceImpl : AuditGrpcService.AuditGrpcServiceBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditGrpcServiceImpl> _logger;

    public AuditGrpcServiceImpl(IAuditService auditService, ILogger<AuditGrpcServiceImpl> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    public override async Task<AuditEventResponse> LogEvent(AuditEventRequest request, ServerCallContext context)
    {
        try
        {
            var auditEvent = new AuditEvent
            {
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                Action = request.Action,
                ResourceType = request.ResourceType,
                ResourceId = request.ResourceId,
                OldValues = !string.IsNullOrEmpty(request.OldValues) ? JsonSerializer.Deserialize<object>(request.OldValues) : null,
                NewValues = !string.IsNullOrEmpty(request.NewValues) ? JsonSerializer.Deserialize<object>(request.NewValues) : null,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Success = request.Success,
                ErrorMessage = request.ErrorMessage,
                Metadata = !string.IsNullOrEmpty(request.Metadata) ? JsonSerializer.Deserialize<Dictionary<string, object>>(request.Metadata) : null,
                ServiceName = request.ServiceName,
                RequestId = request.RequestId,
                SessionId = request.SessionId,
                Timestamp = !string.IsNullOrEmpty(request.Timestamp) && DateTime.TryParse(request.Timestamp, out var ts) ? ts : DateTime.UtcNow
            };

            var auditLog = await _auditService.LogEventAsync(auditEvent);

            return new AuditEventResponse
            {
                Success = true,
                Message = "Audit event logged successfully",
                AuditLogId = auditLog.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit event via gRPC");
            return new AuditEventResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public override async Task<AuditBatchResponse> LogBatchEvents(AuditBatchRequest request, ServerCallContext context)
    {
        try
        {
            var auditEvents = request.Events.Select(e => new AuditEvent
            {
                UserId = e.UserId,
                UserEmail = e.UserEmail,
                Action = e.Action,
                ResourceType = e.ResourceType,
                ResourceId = e.ResourceId,
                OldValues = !string.IsNullOrEmpty(e.OldValues) ? JsonSerializer.Deserialize<object>(e.OldValues) : null,
                NewValues = !string.IsNullOrEmpty(e.NewValues) ? JsonSerializer.Deserialize<object>(e.NewValues) : null,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                Success = e.Success,
                ErrorMessage = e.ErrorMessage,
                Metadata = !string.IsNullOrEmpty(e.Metadata) ? JsonSerializer.Deserialize<Dictionary<string, object>>(e.Metadata) : null,
                ServiceName = e.ServiceName,
                RequestId = e.RequestId,
                SessionId = e.SessionId,
                Timestamp = !string.IsNullOrEmpty(e.Timestamp) && DateTime.TryParse(e.Timestamp, out var ts) ? ts : DateTime.UtcNow
            }).ToList();

            var auditLogs = await _auditService.LogBatchEventsAsync(auditEvents);

            return new AuditBatchResponse
            {
                Success = true,
                Message = "Batch audit events logged successfully",
                ProcessedCount = auditLogs.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging batch audit events via gRPC");
            return new AuditBatchResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public override async Task<AuditLogsResponse> GetAuditLogs(AuditQueryRequest request, ServerCallContext context)
    {
        try
        {
            var query = new Models.AuditQueryDto
            {
                UserId = request.UserId,
                Action = request.Action,
                ResourceType = request.ResourceType,
                ServiceName = request.ServiceName,
                FromDate = !string.IsNullOrEmpty(request.FromDate) && DateTime.TryParse(request.FromDate, out var fromDate) ? fromDate : null,
                ToDate = !string.IsNullOrEmpty(request.ToDate) && DateTime.TryParse(request.ToDate, out var toDate) ? toDate : null,
                Page = request.Page,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                SortOrder = request.SortOrder
            };

            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            var auditLogsData = logs.Select(l => new AuditLogData
            {
                Id = l.Id,
                UserId = l.UserId ?? "",
                UserEmail = l.UserEmail ?? "",
                Action = l.Action,
                ResourceType = l.ResourceType,
                ResourceId = l.ResourceId ?? "",
                OldValues = l.OldValues ?? "",
                NewValues = l.NewValues ?? "",
                IpAddress = l.IpAddress ?? "",
                UserAgent = l.UserAgent ?? "",
                Success = l.Success,
                ErrorMessage = l.ErrorMessage ?? "",
                Metadata = l.Metadata ?? "",
                CreatedAt = l.CreatedAt.ToString("O"),
                ServiceName = l.ServiceName,
                RequestId = l.RequestId ?? "",
                SessionId = l.SessionId ?? ""
            }).ToList();

            return new AuditLogsResponse
            {
                Success = true,
                Message = "Audit logs retrieved successfully",
                Logs = { auditLogsData },
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs via gRPC");
            return new AuditLogsResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public override async Task<AuditLogResponse> GetAuditLogById(AuditLogByIdRequest request, ServerCallContext context)
    {
        try
        {
            var auditLog = await _auditService.GetAuditLogByIdAsync(request.Id);

            if (auditLog == null)
            {
                return new AuditLogResponse
                {
                    Success = false,
                    Message = "Audit log not found"
                };
            }

            var auditLogData = new AuditLogData
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId ?? "",
                UserEmail = auditLog.UserEmail ?? "",
                Action = auditLog.Action,
                ResourceType = auditLog.ResourceType,
                ResourceId = auditLog.ResourceId ?? "",
                OldValues = auditLog.OldValues ?? "",
                NewValues = auditLog.NewValues ?? "",
                IpAddress = auditLog.IpAddress ?? "",
                UserAgent = auditLog.UserAgent ?? "",
                Success = auditLog.Success,
                ErrorMessage = auditLog.ErrorMessage ?? "",
                Metadata = auditLog.Metadata ?? "",
                CreatedAt = auditLog.CreatedAt.ToString("O"),
                ServiceName = auditLog.ServiceName,
                RequestId = auditLog.RequestId ?? "",
                SessionId = auditLog.SessionId ?? ""
            };

            return new AuditLogResponse
            {
                Success = true,
                Message = "Audit log retrieved successfully",
                AuditLog = auditLogData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log by ID via gRPC: {Id}", request.Id);
            return new AuditLogResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public override async Task<CleanupResponse> CleanupOldLogs(CleanupRequest request, ServerCallContext context)
    {
        try
        {
            var deletedCount = await _auditService.CleanupOldLogsAsync(request.RetentionDays);

            return new CleanupResponse
            {
                Success = true,
                Message = $"Cleaned up {deletedCount} old audit logs",
                DeletedCount = deletedCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old audit logs via gRPC");
            return new CleanupResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
}
