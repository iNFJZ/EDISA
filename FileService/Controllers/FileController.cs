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

        public FileController(
            IFileService fileService, 
            IFileValidationService validationService,
            ILogger<FileController> logger,
            IEmailMessageService emailMessageService,
            IStringLocalizer<FileController> localizer,
            INotificationService notificationService)
        {
            _fileService = fileService;
            _validationService = validationService;
            _logger = logger;
            _emailMessageService = emailMessageService;
            _localizer = localizer;
            _notificationService = notificationService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
        {
            var files = request.Files;
            if (files == null || files.Count == 0)
                return BadRequest(_localizer["NoFilesUploaded"]);

            var userEmailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
            var userNameClaim = User.FindFirst(JwtRegisteredClaimNames.Name);
            var userEmail = userEmailClaim?.Value ?? "";
            var userName = userNameClaim?.Value ?? "";

            var results = new List<object>();
            foreach (var file in files)
            {
                var (isValid, errorMessage) = _validationService.ValidateFile(file);
                if (!isValid)
                {
                    results.Add(new { fileName = file?.FileName ?? "", message = _localizer[errorMessage] });
                    continue;
                }

                try
                {
                    using var stream = file.OpenReadStream();
                    await _fileService.UploadFileAsync(file.FileName, stream, file.ContentType);

                    _logger.LogInformation("File uploaded successfully: {FileName}", file.FileName);
                    results.Add(new { fileName = file.FileName, message = _localizer["FileUploadedSuccessfully"] });

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
                        _logger.LogInformation("Email notification sent for file: {FileName}", file.FileName);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Failed to send email notification for file: {FileName}. File was uploaded successfully.", file.FileName);
                    }

                    // Send real-time notification
                    try
                    {
                        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userId))
                        {
                            await _notificationService.SendNotificationAsync(
                                userId,
                                "File Uploaded Successfully",
                                $"Your file '{file.FileName}' has been uploaded successfully.",
                                "success",
                                JsonSerializer.Serialize(new { fileName = file.FileName, eventType = "upload" })
                            );
                        }
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogWarning(notificationEx, "Failed to send real-time notification for file: {FileName}. File was uploaded successfully.", file.FileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
                    results.Add(new { fileName = file?.FileName, message = _localizer["ErrorUploadingFile"] });
                }
            }
            return Ok(results);
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var files = await _fileService.ListFilesAsync();
                if (!files.Any(f => f.FileName == fileName))
                {
                    return NotFound(new { message = string.Format(_localizer["FileNotFound"], fileName) });
                }
                var stream = await _fileService.DownloadFileAsync(fileName);

                _logger.LogInformation("File downloaded successfully: {FileName}", fileName);

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
                        Username = userName,
                        FileName = fileName,
                        EventType = "Download",
                        EventTime = DateTime.UtcNow,
                        Language = userLanguage
                    });
                    _logger.LogInformation("Email notification sent for download: {FileName}", fileName);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send email notification for download: {FileName}. File was downloaded successfully.", fileName);
                }

                // Send real-time notification
                try
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _notificationService.SendNotificationAsync(
                            userId,
                            "File Downloaded Successfully",
                            $"Your file '{fileName}' has been downloaded successfully.",
                            "info",
                            JsonSerializer.Serialize(new { fileName = fileName, eventType = "download" })
                        );
                    }
                }
                catch (Exception notificationEx)
                {
                    _logger.LogWarning(notificationEx, "Failed to send real-time notification for download: {FileName}. File was downloaded successfully.", fileName);
                }

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                return StatusCode(500, _localizer["ErrorDownloadingFile"]);
            }
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

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                var files = await _fileService.ListFilesAsync();
                if (!files.Any(f => f.FileName == fileName))
                {
                    return NotFound(new { message = string.Format(_localizer["FileNotFound"], fileName) });
                }
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
                    _logger.LogInformation("Email notification sent for delete: {FileName}", fileName);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send email notification for delete: {FileName}. File was deleted successfully.", fileName);
                }

                // Send real-time notification
                try
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _notificationService.SendNotificationAsync(
                            userId,
                            "File Deleted Successfully",
                            $"Your file '{fileName}' has been deleted successfully.",
                            "warning",
                            JsonSerializer.Serialize(new { fileName = fileName, eventType = "delete" })
                        );
                    }
                }
                catch (Exception notificationEx)
                {
                    _logger.LogWarning(notificationEx, "Failed to send real-time notification for delete: {FileName}. File was deleted successfully.", fileName);
                }

                return Ok(new { message = _localizer["FileDeletedSuccessfully"] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
                return StatusCode(500, _localizer["ErrorDeletingFile"]);
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
    }
}