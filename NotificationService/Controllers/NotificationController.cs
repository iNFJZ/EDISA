using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services;
using System.Security.Claims;
using Shared.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILoggingService _loggingService;
        private readonly Shared.Services.IAuditHelper _auditHelper;

        public NotificationController(INotificationService notificationService, ILoggingService loggingService, Shared.Services.IAuditHelper auditHelper)
        {
            _notificationService = notificationService;
            _loggingService = loggingService;
            _auditHelper = auditHelper;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("GetUserNotifications failed: User ID not found in claims");
                    return Unauthorized();
                }

                _loggingService.Information("GetUserNotifications called for User: {UserId}, Page: {Page}, PageSize: {PageSize}", 
                    userId, page, pageSize);
                
                var language = User.FindFirst("language")?.Value ?? "en";
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, language);
                
                _loggingService.Information("GetUserNotifications completed successfully for User: {UserId}. Found {Count} notifications", 
                    userId, notifications.Count);
                
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("GetUserNotifications failed for User: {UserId}", ex, userId);
                return StatusCode(500, new { message = "An error occurred while fetching notifications" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("GetNotification failed: User ID not found in claims");
                    return Unauthorized();
                }

                _loggingService.Information("GetNotification called for ID: {Id}, User: {UserId}", id, userId);
                
                var language = User.FindFirst("language")?.Value ?? "en";
                var notification = await _notificationService.GetNotificationByIdAsync(id, language);
                if (notification == null)
                {
                    _loggingService.Warning("Notification not found for ID: {Id}, User: {UserId}", id, userId);
                    return NotFound();
                }

                _loggingService.Information("GetNotification completed successfully for ID: {Id}, User: {UserId}", id, userId);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("GetNotification failed for ID: {Id}, User: {UserId}", ex, id, userId);
                return StatusCode(500, new { message = "An error occurred while fetching notification" });
            }
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("GetUnreadCount failed: User ID not found in claims");
                    return Unauthorized();
                }

                _loggingService.Information("GetUnreadCount called for User: {UserId}", userId);
                
                var count = await _notificationService.GetUnreadCountAsync(userId);
                
                _loggingService.Information("GetUnreadCount completed successfully for User: {UserId}. Unread count: {Count}", userId, count);
                
                return Ok(count);
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("GetUnreadCount failed for User: {UserId}", ex, userId);
                return StatusCode(500, new { message = "An error occurred while fetching unread count" });
            }
        }

        [HttpPost("{id}/mark-as-read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("MarkAsRead failed: User ID not found in claims");
                    return Unauthorized();
                }

                _loggingService.Information("MarkAsRead called for Notification ID: {Id}, User: {UserId}", id, userId);
                
                var success = await _notificationService.MarkAsReadAsync(id, userId);
                if (!success)
                {
                    _loggingService.Warning("MarkAsRead failed: Notification not found for ID: {Id}, User: {UserId}", id, userId);
                    return NotFound();
                }

                _loggingService.Information("MarkAsRead completed successfully for Notification ID: {Id}, User: {UserId}", id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("MarkAsRead failed for Notification ID: {Id}, User: {UserId}", ex, id, userId);
                return StatusCode(500, new { message = "An error occurred while marking notification as read" });
            }
        }

        [HttpPost("mark-all-as-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("MarkAllAsRead failed: User ID not found in claims");
                    return Unauthorized();
                }

                _loggingService.Information("MarkAllAsRead called for User: {UserId}", userId);
                
                await _notificationService.MarkAllAsReadAsync(userId);
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new Shared.AuditModels.AuditEvent
                        {
                            UserId = userId,
                            UserEmail = User?.FindFirst(ClaimTypes.Email)?.Value,
                            Action = "MARK_ALL_NOTIFICATIONS_READ",
                            ResourceType = "Notification",
                            ResourceId = userId,
                            OldValues = null,
                            NewValues = new { Action = "Marked all notifications as read", UserId = userId },
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            Success = true,
                            ServiceName = "NotificationService",
                            RequestId = HttpContext.TraceIdentifier
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch (Exception auditEx)
                    {
                        _loggingService.Warning("Failed to log audit event for mark all notifications as read", auditEx);
                    }
                });

                _loggingService.Information("MarkAllAsRead completed successfully for User: {UserId}", userId);
                return Ok();
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("MarkAllAsRead failed for User: {UserId}", ex, userId);
                return StatusCode(500, new { message = "An error occurred while marking all notifications as read" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("DeleteNotification failed: User ID not found in claims");
                    return Unauthorized();
                }

                _loggingService.Information("DeleteNotification called for Notification ID: {Id}, User: {UserId}", id, userId);
                
                var success = await _notificationService.DeleteNotificationAsync(id, userId);
                if (!success)
                {
                    _loggingService.Warning("DeleteNotification failed: Notification not found for ID: {Id}, User: {UserId}", id, userId);
                    return NotFound();
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new Shared.AuditModels.AuditEvent
                        {
                            UserId = userId,
                            UserEmail = User?.FindFirst(ClaimTypes.Email)?.Value,
                            Action = "DELETE_NOTIFICATION",
                            ResourceType = "Notification",
                            ResourceId = id.ToString(),
                            OldValues = null,
                            NewValues = new { Action = "Deleted notification", NotificationId = id, UserId = userId },
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            Success = true,
                            ServiceName = "NotificationService",
                            RequestId = HttpContext.TraceIdentifier
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch (Exception auditEx)
                    {
                        _loggingService.Warning("Failed to log audit event for notification deletion", auditEx);
                    }
                });

                _loggingService.Information("DeleteNotification completed successfully for Notification ID: {Id}, User: {UserId}", id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("DeleteNotification failed for Notification ID: {Id}, User: {UserId}", ex, id, userId);
                return StatusCode(500, new { message = "An error occurred while deleting notification" });
            }
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAllNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _loggingService.Warning("DeleteAllNotifications failed: User ID not found in claims");
                    return Unauthorized();
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new Shared.AuditModels.AuditEvent
                        {
                            UserId = userId,
                            UserEmail = User?.FindFirst(ClaimTypes.Email)?.Value,
                            Action = "DELETE_ALL_NOTIFICATIONS",
                            ResourceType = "Notification",
                            ResourceId = userId,
                            OldValues = null,
                            NewValues = new { Action = "Deleted all notifications", UserId = userId },
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            Success = true,
                            ServiceName = "NotificationService",
                            RequestId = HttpContext.TraceIdentifier
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch (Exception auditEx)
                    {
                        _loggingService.Warning("Failed to log audit event for mark all notifications as read", auditEx);
                    }
                });

                _loggingService.Information("DeleteAllNotifications called for User: {UserId}", userId);
                
                await _notificationService.DeleteAllNotificationsAsync(userId);

                // Audit: DELETE_ALL_NOTIFICATIONS
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new Shared.AuditModels.AuditEvent
                        {
                            UserId = userId,
                            UserEmail = User?.FindFirst(ClaimTypes.Email)?.Value,
                            Action = "DELETE_ALL_NOTIFICATIONS",
                            ResourceType = "Notification",
                            ResourceId = userId,
                            OldValues = null,
                            NewValues = new { Action = "DeleteAll", UserId = userId },
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            Success = true,
                            ServiceName = "NotificationService",
                            RequestId = HttpContext.TraceIdentifier
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch { }
                });

                _loggingService.Information("DeleteAllNotifications completed successfully for User: {UserId}", userId);
                return Ok();
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _loggingService.Error("DeleteAllNotifications failed for User: {UserId}", ex, userId);
                return StatusCode(500, new { message = "An error occurred while deleting all notifications" });
            }
        }

        [HttpPost("send-to-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToUser([FromBody] CreateNotificationDto dto, [FromQuery] string userId, [FromQuery] string language = "en")
        {
            await _notificationService.SendNotificationToUserAsync(userId, dto, language);

            // Audit: SEND_TO_USER
            _ = Task.Run(async () =>
            {
                try
                {
                    var actorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    var actorEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                    var auditEvent = new Shared.AuditModels.AuditEvent
                    {
                        UserId = actorId,
                        UserEmail = actorEmail,
                        Action = "SEND_TO_USER",
                        ResourceType = "Notification",
                        ResourceId = userId,
                        OldValues = null,
                        NewValues = dto,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Success = true,
                        ServiceName = "NotificationService",
                        RequestId = HttpContext.TraceIdentifier
                    };
                    await _auditHelper.LogEventAsync(auditEvent);
                }
                catch { }
            });

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var language = User.FindFirst("language")?.Value ?? "en";
            var notification = await _notificationService.CreateNotificationAsync(dto, language);

            // Audit: CREATE_NOTIFICATION
            _ = Task.Run(async () =>
            {
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                    var auditEvent = new Shared.AuditModels.AuditEvent
                    {
                        UserId = userId,
                        UserEmail = userEmail,
                        Action = "CREATE_NOTIFICATION",
                        ResourceType = "Notification",
                        ResourceId = notification.Id.ToString(),
                        OldValues = null,
                        NewValues = notification,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Success = true,
                        ServiceName = "NotificationService",
                        RequestId = HttpContext.TraceIdentifier
                    };
                    await _auditHelper.LogEventAsync(auditEvent);
                }
                catch { }
            });

            return Ok(notification);
        }

        [HttpPost("internal")]
        [AllowAnonymous]
        public async Task<ActionResult<NotificationDto>> CreateNotificationInternal([FromBody] CreateNotificationDto dto, [FromQuery] string language = "en")
        {
            var notification = await _notificationService.CreateNotificationAsync(dto, language);

            // Audit: CREATE_NOTIFICATION_INTERNAL
            _ = Task.Run(async () =>
            {
                try
                {
                    var auditEvent = new Shared.AuditModels.AuditEvent
                    {
                        UserId = dto.UserId,
                        UserEmail = null,
                        Action = "CREATE_NOTIFICATION_INTERNAL",
                        ResourceType = "Notification",
                        ResourceId = notification.Id.ToString(),
                        OldValues = null,
                        NewValues = notification,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Success = true,
                        ServiceName = "NotificationService",
                        RequestId = HttpContext.TraceIdentifier
                    };
                    await _auditHelper.LogEventAsync(auditEvent);
                }
                catch { }
            });

            return Ok(notification);
        }

        [HttpPost("send-to-all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToAll([FromBody] CreateNotificationDto dto, [FromQuery] string language = "en")
        {
            await _notificationService.SendNotificationToAllAsync(dto, language);

            // Audit: SEND_TO_ALL
            _ = Task.Run(async () =>
            {
                try
                {
                    var actorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    var actorEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                    var auditEvent = new Shared.AuditModels.AuditEvent
                    {
                        UserId = actorId,
                        UserEmail = actorEmail,
                        Action = "SEND_TO_ALL",
                        ResourceType = "Notification",
                        ResourceId = "ALL",
                        OldValues = null,
                        NewValues = dto,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Success = true,
                        ServiceName = "NotificationService",
                        RequestId = HttpContext.TraceIdentifier
                    };
                    await _auditHelper.LogEventAsync(auditEvent);
                }
                catch { }
            });

            return Ok();
        }

        [HttpPost("send-to-group")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToGroup([FromBody] CreateNotificationDto dto, [FromQuery] List<string> userIds, [FromQuery] string language = "en")
        {
            await _notificationService.SendNotificationToGroupAsync(userIds, dto, language);

            // Audit: SEND_TO_GROUP
            _ = Task.Run(async () =>
            {
                try
                {
                    var actorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    var actorEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                    var auditEvent = new Shared.AuditModels.AuditEvent
                    {
                        UserId = actorId,
                        UserEmail = actorEmail,
                        Action = "SEND_TO_GROUP",
                        ResourceType = "Notification",
                        ResourceId = string.Join(",", userIds ?? new()),
                        OldValues = null,
                        NewValues = dto,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Success = true,
                        ServiceName = "NotificationService",
                        RequestId = HttpContext.TraceIdentifier
                    };
                    await _auditHelper.LogEventAsync(auditEvent);
                }
                catch { }
            });

            return Ok();
        }
    }
} 