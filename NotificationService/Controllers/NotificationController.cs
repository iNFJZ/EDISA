using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetUserNotifications(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var language = User.FindFirst("language")?.Value ?? "en";
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, language);
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            var language = User.FindFirst("language")?.Value ?? "en";
            var notification = await _notificationService.GetNotificationByIdAsync(id, language);
            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }

        [HttpPost("{id}/mark-as-read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _notificationService.MarkAsReadAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPost("mark-all-as-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _notificationService.DeleteNotificationAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAllNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _notificationService.DeleteAllNotificationsAsync(userId);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPost("send-to-user")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToUser([FromBody] CreateNotificationDto dto, [FromQuery] string userId, [FromQuery] string language = "en")
        {
            await _notificationService.SendNotificationToUserAsync(userId, dto, language);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var language = User.FindFirst("language")?.Value ?? "en";
            var notification = await _notificationService.CreateNotificationAsync(dto, language);
            return Ok(notification);
        }

        [HttpPost("internal")]
        [AllowAnonymous]
        public async Task<ActionResult<NotificationDto>> CreateNotificationInternal([FromBody] CreateNotificationDto dto, [FromQuery] string language = "en")
        {
            var notification = await _notificationService.CreateNotificationAsync(dto, language);
            return Ok(notification);
        }

        [HttpPost("send-to-all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToAll([FromBody] CreateNotificationDto dto, [FromQuery] string language = "en")
        {
            await _notificationService.SendNotificationToAllAsync(dto, language);
            return Ok();
        }

        [HttpPost("send-to-group")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendToGroup([FromBody] CreateNotificationDto dto, [FromQuery] List<string> userIds, [FromQuery] string language = "en")
        {
            await _notificationService.SendNotificationToGroupAsync(userIds, dto, language);
            return Ok();
        }
    }
} 