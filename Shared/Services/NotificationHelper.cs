using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Shared.NotificationModels;

namespace Shared.Services
{
    public class NotificationHelper : INotificationHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _notificationServiceUrl;

        public NotificationHelper(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _notificationServiceUrl = configuration["NotificationService:BaseUrl"] ?? "https://localhost:7001";
        }

        public async Task<NotificationResponse> SendNotificationAsync(NotificationRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_notificationServiceUrl}/api/notification/send-to-all", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new NotificationResponse { Success = true };
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return new NotificationResponse { Success = false, Message = errorContent };
            }
            catch (Exception ex)
            {
                return new NotificationResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<NotificationResponse> SendNotificationToUserAsync(string userId, NotificationRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_notificationServiceUrl}/api/notification/send-to-user?userId={userId}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new NotificationResponse { Success = true };
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return new NotificationResponse { Success = false, Message = errorContent };
            }
            catch (Exception ex)
            {
                return new NotificationResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<NotificationResponse> SendNotificationToAllAsync(NotificationRequest request)
        {
            return await SendNotificationAsync(request);
        }

        public async Task<NotificationResponse> SendNotificationToGroupAsync(List<string> userIds, NotificationRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var userIdsParam = string.Join(",", userIds);
                var response = await _httpClient.PostAsync($"{_notificationServiceUrl}/api/notification/send-to-group?userIds={userIdsParam}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return new NotificationResponse { Success = true };
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return new NotificationResponse { Success = false, Message = errorContent };
            }
            catch (Exception ex)
            {
                return new NotificationResponse { Success = false, Message = ex.Message };
            }
        }
    }
} 