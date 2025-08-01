using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using FileService.Models;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace FileService.Services
{
    public class MinioFileService : IFileService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly ILogger<MinioFileService> _logger;

        public MinioFileService(IOptions<MinioOptions> options, ILogger<MinioFileService> logger)
        {
            var config = options.Value;
            _minioClient = new MinioClient()
                .WithEndpoint(config.Endpoint)
                .WithCredentials(config.AccessKey, config.SecretKey)
                .WithSSL(false)
                .Build();
            _bucketName = config.BucketName!;
            _logger = logger;
        }

        private async Task EnsureBucketExists()
        {
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }
        }
        
        public async Task UploadFileAsync(string objectName, Stream fileStream, string contentType, string description = "")
        {
            await EnsureBucketExists();
            
            var metadata = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(description))
            {
                metadata["description"] = description;
            }
            
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithHeaders(metadata);
            
            await _minioClient.PutObjectAsync(putObjectArgs);
        }

        public async Task<Stream> DownloadFileAsync(string objectName)
        {
            var ms = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream((stream) => stream.CopyTo(ms));
            await _minioClient.GetObjectAsync(getObjectArgs);
            ms.Position = 0;
            return ms;
        }

        public async Task DeleteFileAsync(string objectName)
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);
            
            await _minioClient.RemoveObjectAsync(removeObjectArgs);
        }

        public async Task<List<FileService.Models.FileInfo>> ListFilesAsync()
        {
            await EnsureBucketExists();
            
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithRecursive(true);
            
            var files = new List<FileService.Models.FileInfo>();
            var tcs = new TaskCompletionSource<bool>();
            
            var objects = _minioClient.ListObjectsAsync(listObjectsArgs);
            objects.Subscribe(
                item => {
                    var fileInfo = new FileService.Models.FileInfo {
                        FileName = item.Key,
                        FileUrl = $"/files/{item.Key}",
                        FileSize = (long)item.Size,
                        ContentType = string.Empty,
                        UploadedBy = string.Empty,
                        UploadedAt = item.LastModified.ToString(),
                        Description = string.Empty
                    };
                    
                    files.Add(fileInfo);
                },
                ex => {
                    _logger.LogError(ex, "Error listing objects");
                    tcs.SetException(ex);
                },
                () => {
                    tcs.SetResult(true);
                }
            );
            
            await tcs.Task;
            
            foreach (var fileInfo in files)
            {
                try
                {
                    var statObjectArgs = new StatObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(fileInfo.FileName);
                    var objectInfo = await _minioClient.StatObjectAsync(statObjectArgs);
                    fileInfo.ContentType = objectInfo.ContentType ?? string.Empty;
                    if (objectInfo.MetaData != null && objectInfo.MetaData.ContainsKey("description"))
                    {
                        fileInfo.Description = objectInfo.MetaData["description"];
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get file info for: {Key}", fileInfo.FileName);
                }
            }
            
            return files;
        }

        public async Task<FileService.Models.FileInfo> GetFileInfoAsync(string objectName)
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);
            var objectInfo = await _minioClient.StatObjectAsync(statObjectArgs);
            
            var description = string.Empty;
            if (objectInfo.MetaData != null && objectInfo.MetaData.ContainsKey("description"))
            {
                description = objectInfo.MetaData["description"];
            }
            
            return new FileService.Models.FileInfo {
                FileName = objectName,
                FileUrl = $"/files/{objectName}",
                FileSize = (long)objectInfo.Size,
                ContentType = objectInfo.ContentType ?? string.Empty,
                UploadedBy = string.Empty,
                UploadedAt = objectInfo.LastModified.ToString(),
                Description = description
            };
        }
    }
} 