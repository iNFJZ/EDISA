using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, string language = "en");
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, string language = "en");
        Task<NotificationDto?> GetNotificationByIdAsync(int id, string language = "en");
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> DeleteNotificationAsync(int id, string userId);
        Task<bool> DeleteAllNotificationsAsync(string userId);
        Task SendNotificationToUserAsync(string userId, CreateNotificationDto dto, string language = "en");
        Task SendNotificationToAllAsync(CreateNotificationDto dto, string language = "en");
        Task SendNotificationToGroupAsync(List<string> userIds, CreateNotificationDto dto, string language = "en");
    }
} 