namespace FileService.Models
{
    public class FileUploadEvent
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class FileDownloadEvent
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime DownloadTime { get; set; }
    }

    public class FileDeleteEvent
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime DeleteTime { get; set; }
    }

    public class FileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string UploadedAt { get; set; } = string.Empty;
    }
} 