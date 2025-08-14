namespace Shared.EmailModels
{
    public class FileEventEmailNotification
    {
        public string To { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string? FileSize { get; set; }
        public string? IpAddress { get; set; }
        public string? Language { get; set; } = "en";
    }
}
