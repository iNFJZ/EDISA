using Shared.NotificationModels;

namespace Shared.Services
{
    public interface INotificationHelper
    {
        Task<NotificationResponse> SendNotificationAsync(NotificationRequest request);
        Task<NotificationResponse> SendNotificationToUserAsync(string userId, NotificationRequest request);
        Task<NotificationResponse> SendNotificationToAllAsync(NotificationRequest request);
        Task<NotificationResponse> SendNotificationToGroupAsync(List<string> userIds, NotificationRequest request);
    }
} 