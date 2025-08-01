namespace Shared.NotificationModels
{
    public class NotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Data { get; set; }
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? NotificationId { get; set; }
    }
} 