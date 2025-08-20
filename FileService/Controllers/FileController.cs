using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using FileService.Services;
using FileService.Models;
using System.Security.Claims;
using System.Collections.Generic;
using FileService.DTOs;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using System.Text.Json;
using System.IO;
using System.Linq;
using Shared.Services;
using Shared.AuditModels;

namespace FileService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IFileValidationService _validationService;
        private readonly ILogger<FileController> _logger;
        private readonly IEmailMessageService _emailMessageService;
        private readonly IStringLocalizer<FileController> _localizer;
        private readonly INotificationService _notificationService;
        private readonly IAuditHelper _auditHelper;

        public FileController(
            IFileService fileService, 
            IFileValidationService validationService,
            ILogger<FileController> logger,
            IEmailMessageService emailMessageService,
            IStringLocalizer<FileController> localizer,
            INotificationService notificationService,
            IAuditHelper auditHelper)
        {
            _fileService = fileService;
            _validationService = validationService;
            _logger = logger;
            _emailMessageService = emailMessageService;
            _localizer = localizer;
            _notificationService = notificationService;
            _auditHelper = auditHelper;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
        {
            try
            {
                var userEmailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
                var userNameClaim = User.FindFirst(JwtRegisteredClaimNames.Name);
                var userEmail = userEmailClaim?.Value ?? "";
                var userName = userNameClaim?.Value ?? "";
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

                _logger.LogInformation("File upload started by User: {UserId}, Email: {Email}, Files count: {FilesCount}", 
                    userId, userEmail, request.Files?.Count ?? 0);

                var files = request.Files;
                if (files == null || files.Count == 0)
                {
                    _logger.LogWarning("File upload failed: No files provided by User: {UserId}", userId);
                    // Audit failure
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var auditEvent = new FileAuditEvent
                            {
                                UserId = userId,
                                UserEmail = userEmail,
                                Action = "UPLOAD_FILE",
                                ResourceId = null,
                                Success = false,
                                ErrorMessage = "No files provided",
                                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                                UserAgent = Request.Headers["User-Agent"].ToString()
                            };
                            await _auditHelper.LogEventAsync(auditEvent);
                        }
                        catch { }
                    });
                    return BadRequest(_localizer["NoFilesUploaded"]);
                }

                var results = new List<object>();
                var successCount = 0;
                var failureCount = 0;

                foreach (var file in files)
                {
                    try
                    {
                        _logger.LogInformation("Processing file upload: {FileName}, Size: {FileSize}, ContentType: {ContentType} for User: {UserId}", 
                            file.FileName, file.Length, file.ContentType, userId);

                        var (isValid, errorMessage) = _validationService.ValidateFile(file);
                        if (!isValid)
                        {
                            _logger.LogWarning("File validation failed: {FileName}, Error: {ErrorMessage}, User: {UserId}", 
                                file.FileName, errorMessage, userId);
                            // Audit validation failure
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    var auditEvent = new FileAuditEvent
                                    {
                                        UserId = userId,
                                        UserEmail = userEmail,
                                        Action = "UPLOAD_FILE",
                                        ResourceId = file.FileName,
                                        Success = false,
                                        ErrorMessage = errorMessage ?? "Validation failed",
                                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                                        UserAgent = Request.Headers["User-Agent"].ToString(),
                                        Metadata = new Dictionary<string, object>
                                        {
                                            { "FileName", file.FileName },
                                            { "FileSize", file.Length },
                                            { "ContentType", file.ContentType }
                                        }
                                    };
                                    await _auditHelper.LogEventAsync(auditEvent);
                                }
                                catch { }
                            });
                            results.Add(new { fileName = file?.FileName ?? "", message = _localizer[errorMessage] });
                            failureCount++;
                            continue;
                        }

                        using var stream = file.OpenReadStream();
                        var description = request.Description ?? "";
                        await _fileService.UploadFileAsync(file.FileName, stream, file.ContentType, description);

                        _logger.LogInformation("File uploaded successfully: {FileName}, Size: {FileSize}, User: {UserId}", 
                            file.FileName, file.Length, userId);
                        results.Add(new { fileName = file.FileName, message = _localizer["FileUploadedSuccessfully"] });
                        successCount++;

                        var auditEvent = new FileAuditEvent
                        {
                            UserId = userId,
                            UserEmail = userEmail,
                            Action = "UPLOAD_FILE",
                            ResourceId = file.FileName,
                            Success = true,
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            Metadata = new Dictionary<string, object>
                            {
                                { "FileName", file.FileName },
                                { "FileSize", file.Length },
                                { "ContentType", file.ContentType },
                                { "Description", description },
                                { "UploadTime", DateTime.UtcNow }
                            }
                        };

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _auditHelper.LogEventAsync(auditEvent);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to log audit event for file upload: {FileName}", file.FileName);
                            }
                        });

                        try
                        {
                            var userLanguageClaim = User.FindFirst("language");
                            var userLanguage = userLanguageClaim?.Value ?? "en";
                            
                            await _emailMessageService.PublishFileEventNotificationAsync(new FileEventEmailNotification
                            {
                                To = userEmail,
                                Username = userName,
                                FileName = file.FileName,
                                EventType = "Upload",
                                EventTime = DateTime.UtcNow,
                                Language = userLanguage
                            });
                            _logger.LogInformation("Email notification sent for file upload: {FileName}, User: {UserId}", file.FileName, userId);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Failed to send email notification for file upload: {FileName}, User: {UserId}. File was uploaded successfully.", 
                                file.FileName, userId);
                        }
    
                        try
                        {
                            if (!string.IsNullOrEmpty(userId))
                            {
                                var userLanguageClaim = User.FindFirst("language");
                                var userLanguage = userLanguageClaim?.Value ?? "en";
                                
                                var title = userLanguage switch
                                {
                                    "vi" => "Tải tệp lên thành công",
                                    "ja" => "ファイルのアップロードが完了しました",
                                    _ => "File Uploaded Successfully"
                                };
                                
                                var message = userLanguage switch
                                {
                                    "vi" => $"Tệp '{file.FileName}' của bạn đã được tải lên thành công.",
                                    "ja" => $"ファイル '{file.FileName}' のアップロードが完了しました。",
                                    _ => $"Your file '{file.FileName}' has been uploaded successfully."
                                };
                                
                                await _notificationService.SendNotificationAsync(
                                    userId,
                                    title,
                                    message,
                                    "success",
                                    JsonSerializer.Serialize(new { fileName = file.FileName, eventType = "upload" })
                                );
                                _logger.LogInformation("Real-time notification sent for file upload: {FileName}, User: {UserId}", file.FileName, userId);
                            }
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogWarning(notificationEx, "Failed to send real-time notification for file upload: {FileName}, User: {UserId}. File was uploaded successfully.", 
                                file.FileName, userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading file: {FileName}, User: {UserId}", file?.FileName, userId);
                        results.Add(new { fileName = file?.FileName, message = _localizer["ErrorUploadingFile"] });
                        failureCount++;
                    }
                }

                _logger.LogInformation("File upload batch completed. Success: {SuccessCount}, Failures: {FailureCount}, User: {UserId}", 
                    successCount, failureCount, userId);

                return Ok(results);
            }
            catch (Exception ex)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
                _logger.LogError(ex, "Unexpected error during file upload batch, User: {UserId}", userId);
                return StatusCode(500, new { message = "An unexpected error occurred during file upload" });
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var files = await _fileService.ListFilesAsync();
                
                var decodedFileName = Uri.UnescapeDataString(fileName);
                
                var matchingFile = files.FirstOrDefault(f => 
                    string.Equals(f.FileName, decodedFileName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
                    
                if (matchingFile == null)
                {
                    return NotFound(new { message = string.Format(_localizer["FileNotFound"], fileName) });
                }
                
                var stream = await _fileService.DownloadFileAsync(matchingFile.FileName);

                try
                {
                    var userEmailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
                    var userNameClaim = User.FindFirst(JwtRegisteredClaimNames.Name);
                    var userEmail = userEmailClaim?.Value ?? "";
                    var userName = userNameClaim?.Value ?? "";
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    var userLanguageClaim = User.FindFirst("language");
                    var userLanguage = userLanguageClaim?.Value ?? "en";
                    
                    await _emailMessageService.PublishFileEventNotificationAsync(new FileEventEmailNotification
                    {
                        To = userEmail,
                        Username = userName,
                        FileName = fileName,
                        EventType = "Download",
                        EventTime = DateTime.UtcNow,
                        Language = userLanguage
                    });
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send email notification for download: {FileName}. File was downloaded successfully.", fileName);
                }

                try
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var userLanguageClaim = User.FindFirst("language");
                        var userLanguage = userLanguageClaim?.Value ?? "en";
                        
                        var title = userLanguage switch
                        {
                            "vi" => "Tải xuống tệp thành công",
                            "ja" => "ファイルのダウンロードが完了しました",
                            _ => "File Downloaded Successfully"
                        };
                        
                        var message = userLanguage switch
                        {
                            "vi" => $"Tệp '{fileName}' của bạn đã được tải xuống thành công.",
                            "ja" => $"ファイル '{fileName}' のダウンロードが完了しました。",
                            _ => $"Your file '{fileName}' has been downloaded successfully."
                        };
                        
                        await _notificationService.SendNotificationAsync(
                            userId,
                            title,
                            message,
                            "info",
                            JsonSerializer.Serialize(new { fileName = fileName, eventType = "download" })
                        );
                    }
                }
                catch (Exception notificationEx)
                {
                    _logger.LogWarning(notificationEx, "Failed to send real-time notification for download: {FileName}. File was downloaded successfully.", fileName);
                }

                try
                {
                    var userIdForAudit = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    var userEmailForAudit = (User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email"))?.Value ?? string.Empty;

                    var auditEvent = new FileAuditEvent
                    {
                        UserId = userIdForAudit,
                        UserEmail = userEmailForAudit,
                        Action = "DOWNLOAD_FILE",
                        ResourceId = fileName,
                        Success = true,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Metadata = new Dictionary<string, object>
                        {
                            { "FileName", fileName },
                            { "DownloadTime", DateTime.UtcNow }
                        }
                    };

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _auditHelper.LogEventAsync(auditEvent);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to log audit event for file download: {FileName}", fileName);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error when preparing audit event for download: {FileName}", fileName);
                }

                var contentType = GetContentType(fileName);
                return File(stream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                return StatusCode(500, _localizer["ErrorDownloadingFile"]);
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            };
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            try
            {
                var files = await _fileService.ListFilesAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(500, "Error listing files");
            }
        }

        [HttpGet("get-file-info/{fileName}")]
        public async Task<IActionResult> GetFileInfo(string fileName)
        {
            try
            {
                var fileInfo = await _fileService.GetFileInfoAsync(fileName);
                return Ok(fileInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info: {FileName}", fileName);
                return StatusCode(500, _localizer["ErrorGettingFileInfo"]);
            }
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                await _fileService.DeleteFileAsync(fileName);

                try
                {
                    var userEmailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
                    var userNameClaim = User.FindFirst(JwtRegisteredClaimNames.Name);
                    var userEmail = userEmailClaim?.Value ?? "";
                    var userName = userNameClaim?.Value ?? "";

                    var userLanguageClaim = User.FindFirst("language");
                    var userLanguage = userLanguageClaim?.Value ?? "en";
                    
                    await _emailMessageService.PublishFileEventNotificationAsync(new FileEventEmailNotification
                    {
                        To = userEmail,
                        Username = userName ?? "",
                        FileName = fileName,
                        EventType = "Delete",
                        EventTime = DateTime.UtcNow,
                        Language = userLanguage
                    });
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send email notification for delete: {FileName}. File was deleted successfully.", fileName);
                }

                try
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var userLanguageClaim = User.FindFirst("language");
                        var userLanguage = userLanguageClaim?.Value ?? "en";
                        
                        var title = userLanguage switch
                        {
                            "vi" => "Xóa tệp thành công",
                            "ja" => "ファイルの削除が完了しました",
                            _ => "File Deleted Successfully"
                        };
                        
                        var message = userLanguage switch
                        {
                            "vi" => $"Tệp '{fileName}' của bạn đã được xóa thành công.",
                            "ja" => $"ファイル '{fileName}' の削除が完了しました。",
                            _ => $"Your file '{fileName}' has been deleted successfully."
                        };
                        
                        await _notificationService.SendNotificationAsync(
                            userId,
                            title,
                            message,
                            "warning",
                            JsonSerializer.Serialize(new { fileName = fileName, eventType = "delete" })
                        );
                    }
                }
                catch (Exception notificationEx)
                {
                    _logger.LogWarning(notificationEx, "Failed to send real-time notification for delete: {FileName}. File was deleted successfully.", fileName);
                }

                try
                {
                    var userIdForAudit = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    var userEmailForAudit = (User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email"))?.Value ?? string.Empty;

                    var auditEvent = new FileAuditEvent
                    {
                        UserId = userIdForAudit,
                        UserEmail = userEmailForAudit,
                        Action = "DELETE_FILE",
                        ResourceId = fileName,
                        Success = true,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        Metadata = new Dictionary<string, object>
                        {
                            { "FileName", fileName },
                            { "DeletedAt", DateTime.UtcNow }
                        }
                    };

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _auditHelper.LogEventAsync(auditEvent);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to log audit event for file delete: {FileName}", fileName);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error when preparing audit event for delete: {FileName}", fileName);
                }

                return Ok(new { message = _localizer["FileDeletedSuccessfully"] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
                return StatusCode(500, _localizer["ErrorDeletingFile"]);
            }
        }
    }
}