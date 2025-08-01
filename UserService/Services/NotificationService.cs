using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace UserService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NotificationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string type = "info", string? data = null)
        {
            try
            {
                var notificationData = new
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Icon = "ti ti-bell",
                    Data = data
                };

                var json = JsonSerializer.Serialize(notificationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var notificationServiceUrl = _configuration["NotificationService:BaseUrl"] ?? "http://notification-service";
                Console.WriteLine($"NotificationService: Sending to {notificationServiceUrl}/api/Notification/internal");
                Console.WriteLine($"NotificationService: Request data: {json}");
                var response = await _httpClient.PostAsync($"{notificationServiceUrl}/api/Notification/internal", content);

                if (!response.IsSuccessStatusCode)
                {
                    // Log error but don't throw exception to avoid breaking the main flow
                    Console.WriteLine($"Failed to send notification: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw exception to avoid breaking the main flow
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }
    }
} 