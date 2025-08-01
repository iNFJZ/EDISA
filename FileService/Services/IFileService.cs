namespace FileService.Services
{
    public interface IFileService
    {
        Task UploadFileAsync(string objectName, Stream fileStream, string contentType, string description = "");
        Task<Stream> DownloadFileAsync(string objectName);
        Task DeleteFileAsync(string objectName);
        Task<List<FileService.Models.FileInfo>> ListFilesAsync();
        Task<FileService.Models.FileInfo> GetFileInfoAsync(string objectName);
    }
} 