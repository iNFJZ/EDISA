using AuditService.Models;
using AuditService.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.AuditModels;

namespace AuditService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost("events")]
    public async Task<ActionResult<object>> LogAuditEvent([FromBody] AuditEvent auditEvent)
    {
        try
        {
            var auditLog = await _auditService.LogEventAsync(auditEvent);

            return Ok(new
            {
                Success = true,
                Message = "Audit event logged successfully",
                Data = auditLog
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit event via HTTP API");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }
        
    [HttpPost("batch-events")]
    public async Task<ActionResult<object>> LogBatchAuditEvents([FromBody] List<AuditEvent> auditEvents)
    {
        try
        {
            var auditLogs = await _auditService.LogBatchEventsAsync(auditEvents);

            return Ok(new
            {
                Success = true,
                Message = "Batch audit events logged successfully",
                Data = auditLogs,
                Count = auditLogs.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging batch audit events via HTTP API");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs")]
    public async Task<ActionResult<object>> GetAuditLogs([FromQuery] AuditQueryDto query)
    {
        try
        {
            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            return Ok(new
            {
                Success = true,
                Message = "Audit logs retrieved successfully",
                Data = logs,
                Pagination = new
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs/{id}")]
    public async Task<ActionResult<object>> GetAuditLogById(long id)
    {
        try
        {
            var auditLog = await _auditService.GetAuditLogByIdAsync(id);

            if (auditLog == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Audit log not found"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Audit log retrieved successfully",
                Data = auditLog
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log by ID: {Id}", id);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs/user/{userId}")]
    public async Task<ActionResult<object>> GetAuditLogsByUserId(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new AuditQueryDto
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            return Ok(new
            {
                Success = true,
                Message = "User audit logs retrieved successfully",
                Data = logs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for user: {UserId}", userId);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs/action/{action}")]
    public async Task<ActionResult<object>> GetAuditLogsByAction(string action, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new AuditQueryDto
            {
                Action = action,
                Page = page,
                PageSize = pageSize
            };

            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            return Ok(new
            {
                Success = true,
                Message = "Action audit logs retrieved successfully",
                Data = logs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for action: {Action}", action);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs/resource/{resourceType}")]
    public async Task<ActionResult<object>> GetAuditLogsByResourceType(string resourceType, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new AuditQueryDto
            {
                ResourceType = resourceType,
                Page = page,
                PageSize = pageSize
            };

            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            return Ok(new
            {
                Success = true,
                Message = "Resource type audit logs retrieved successfully",
                Data = logs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for resource type: {ResourceType}", resourceType);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs/service/{serviceName}")]
    public async Task<ActionResult<object>> GetAuditLogsByService(string serviceName, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new AuditQueryDto
            {
                ServiceName = serviceName,
                Page = page,
                PageSize = pageSize
            };

            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            return Ok(new
            {
                Success = true,
                Message = "Service audit logs retrieved successfully",
                Data = logs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for service: {ServiceName}", serviceName);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("logs/date-range")]
    public async Task<ActionResult<object>> GetAuditLogsByDateRange(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new AuditQueryDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var (logs, totalCount) = await _auditService.GetAuditLogsAsync(query);

            return Ok(new
            {
                Success = true,
                Message = "Date range audit logs retrieved successfully",
                Data = logs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for date range: {FromDate} to {ToDate}", fromDate, toDate);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpPost("cleanup")]
    public async Task<ActionResult<object>> CleanupOldLogs([FromQuery] int retentionDays = 365)
    {
        try
        {
            var deletedCount = await _auditService.CleanupOldLogsAsync(retentionDays);

            return Ok(new
            {
                Success = true,
                Message = $"Cleaned up {deletedCount} old audit logs",
                DeletedCount = deletedCount,
                RetentionDays = retentionDays
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old audit logs");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Internal server error",
                Error = ex.Message
            });
        }
    }

    [HttpGet("health")]
    public ActionResult<object> Health()
    {
        return Ok(new
        {
            Status = "healthy",
            Service = "AuditService",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}
