using Microsoft.AspNetCore.Http;

namespace FileService.DTOs
{
    public class UploadFileRequest
    {
        public List<IFormFile>? Files { get; set; }
        public string? Description { get; set; }
    }
} 