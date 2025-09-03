using Grpc.Core;
using EmailService.Services;
using EmailService.DTOs;
using EmailService.Models;

namespace EmailService.Services;

public class EmailGrpcService : global::GrpcGreeter.EmailService.EmailServiceBase
{
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ILogger<EmailGrpcService> _logger;

    public EmailGrpcService(IEmailTemplateService emailTemplateService, ILogger<EmailGrpcService> logger)
    {
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public override async Task<global::GrpcGreeter.GetTemplateByIdResponse> GetTemplateById(global::GrpcGreeter.GetTemplateByIdRequest request, ServerCallContext context)
    {
        try
        {
            var template = await _emailTemplateService.GetTemplateByIdAsync(request.Id);
            if (template == null)
            {
                return new global::GrpcGreeter.GetTemplateByIdResponse
                {
                    Success = false,
                    Message = "Template not found"
                };
            }

                            return new global::GrpcGreeter.GetTemplateByIdResponse
                {
                    Success = true,
                    Message = "Template retrieved successfully",
                    Template = new global::GrpcGreeter.EmailTemplateInfo
                    {
                        Id = template.Id,
                        Name = template.Name,
                        Language = template.Language,
                        Subject = template.Subject,
                        Body = template.Body,
                        Description = template.Description ?? "",
                        IsActive = template.IsActive,
                        CreatedAt = template.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        UpdatedAt = template.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                        CreatedBy = "",
                        UpdatedBy = template.UpdatedBy ?? "",
                        DeletedAt = template.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                    }
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template by ID: {Id}", request.Id);
            return new global::GrpcGreeter.GetTemplateByIdResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GetTemplateByNameAndLanguageResponse> GetTemplateByNameAndLanguage(global::GrpcGreeter.GetTemplateByNameAndLanguageRequest request, ServerCallContext context)
    {
        try
        {
            var template = await _emailTemplateService.GetTemplateByNameAndLanguageAsync(request.Name, request.Language);
            if (template == null)
            {
                return new global::GrpcGreeter.GetTemplateByNameAndLanguageResponse
                {
                    Success = false,
                    Message = "Template not found"
                };
            }

            return new global::GrpcGreeter.GetTemplateByNameAndLanguageResponse
            {
                Success = true,
                Message = "Template retrieved successfully",
                Template = new global::GrpcGreeter.EmailTemplateInfo
                {
                    Id = template.Id,
                    Name = template.Name,
                    Language = template.Language,
                    Subject = template.Subject,
                    Body = template.Body,
                    Description = template.Description ?? "",
                    IsActive = template.IsActive,
                    CreatedAt = template.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    UpdatedAt = template.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                    CreatedBy = "",
                    UpdatedBy = template.UpdatedBy ?? "",
                    DeletedAt = template.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template by name and language: {Name}, {Language}", request.Name, request.Language);
            return new global::GrpcGreeter.GetTemplateByNameAndLanguageResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GetTemplatesByLanguageResponse> GetTemplatesByLanguage(global::GrpcGreeter.GetTemplatesByLanguageRequest request, ServerCallContext context)
    {
        try
        {
            var templates = await _emailTemplateService.GetTemplatesByLanguageAsync(request.Language);
            var templateInfos = templates.Select(template => new global::GrpcGreeter.EmailTemplateInfo
            {
                Id = template.Id,
                Name = template.Name,
                Language = template.Language,
                Subject = template.Subject,
                Body = template.Body,
                Description = template.Description ?? "",
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                UpdatedAt = template.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                CreatedBy = "",
                UpdatedBy = template.UpdatedBy ?? "",
                DeletedAt = template.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
            }).ToList();

            return new global::GrpcGreeter.GetTemplatesByLanguageResponse
            {
                Success = true,
                Message = "Templates retrieved successfully",
                Templates = { templateInfos }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates by language: {Language}", request.Language);
            return new global::GrpcGreeter.GetTemplatesByLanguageResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GetAllTemplatesResponse> GetAllTemplates(global::GrpcGreeter.GetAllTemplatesRequest request, ServerCallContext context)
    {
        try
        {
            var templates = await _emailTemplateService.GetAllTemplatesAsync();
            var templateInfos = templates.Select(template => new global::GrpcGreeter.EmailTemplateInfo
            {
                Id = template.Id,
                Name = template.Name,
                Language = template.Language,
                Subject = template.Subject,
                Body = template.Body,
                Description = template.Description ?? "",
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                UpdatedAt = template.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                CreatedBy = "",
                UpdatedBy = template.UpdatedBy ?? "",
                DeletedAt = template.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
            }).ToList();

            return new global::GrpcGreeter.GetAllTemplatesResponse
            {
                Success = true,
                Message = "Templates retrieved successfully",
                Templates = { templateInfos }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all templates");
            return new global::GrpcGreeter.GetAllTemplatesResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.CreateTemplateResponse> CreateTemplate(global::GrpcGreeter.CreateTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var template = new EmailTemplate
            {
                Name = request.Name,
                Language = request.Language,
                Subject = request.Subject,
                Body = request.Body,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var createdTemplate = await _emailTemplateService.CreateTemplateAsync(template);
            return new global::GrpcGreeter.CreateTemplateResponse
            {
                Success = true,
                Message = "Template created successfully",
                Template = new global::GrpcGreeter.EmailTemplateInfo
                {
                    Id = createdTemplate.Id,
                    Name = createdTemplate.Name,
                    Language = createdTemplate.Language,
                    Subject = createdTemplate.Subject,
                    Body = createdTemplate.Body,
                    Description = createdTemplate.Description ?? "",
                    IsActive = createdTemplate.IsActive,
                    CreatedAt = createdTemplate.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    UpdatedAt = createdTemplate.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                    CreatedBy = "",
                    UpdatedBy = createdTemplate.UpdatedBy ?? "",
                    DeletedAt = createdTemplate.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return new global::GrpcGreeter.CreateTemplateResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.UpdateTemplateResponse> UpdateTemplate(global::GrpcGreeter.UpdateTemplateRequest request, ServerCallContext context)
    {
        try
        {
            var existingTemplate = await _emailTemplateService.GetTemplateByIdAsync(request.Id);
            if (existingTemplate == null)
            {
                return new global::GrpcGreeter.UpdateTemplateResponse
                {
                    Success = false,
                    Message = "Template not found"
                };
            }

            if (!string.IsNullOrEmpty(request.Name))
                existingTemplate.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Language))
                existingTemplate.Language = request.Language;
            if (!string.IsNullOrEmpty(request.Subject))
                existingTemplate.Subject = request.Subject;
            if (!string.IsNullOrEmpty(request.Body))
                existingTemplate.Body = request.Body;
            if (!string.IsNullOrEmpty(request.Description))
                existingTemplate.Description = request.Description;
            existingTemplate.IsActive = request.IsActive;
            if (!string.IsNullOrEmpty(request.UpdatedBy))
                existingTemplate.UpdatedBy = request.UpdatedBy;

            var updatedTemplate = await _emailTemplateService.UpdateTemplateAsync(existingTemplate);
            return new global::GrpcGreeter.UpdateTemplateResponse
            {
                Success = true,
                Message = "Template updated successfully",
                Template = new global::GrpcGreeter.EmailTemplateInfo
                {
                    Id = updatedTemplate.Id,
                    Name = updatedTemplate.Name,
                    Language = updatedTemplate.Language,
                    Subject = updatedTemplate.Subject,
                    Body = updatedTemplate.Body,
                    Description = updatedTemplate.Description ?? "",
                    IsActive = updatedTemplate.IsActive,
                    CreatedAt = updatedTemplate.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    UpdatedAt = updatedTemplate.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? "",
                    CreatedBy = "",
                    UpdatedBy = updatedTemplate.UpdatedBy ?? "",
                    DeletedAt = updatedTemplate.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template: {Id}", request.Id);
            return new global::GrpcGreeter.UpdateTemplateResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.DeleteTemplateResponse> DeleteTemplate(global::GrpcGreeter.DeleteTemplateRequest request, ServerCallContext context)
    {
        try
        {
            await _emailTemplateService.DeleteTemplateAsync(request.Id);
            return new global::GrpcGreeter.DeleteTemplateResponse
            {
                Success = true,
                Message = "Template deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template: {Id}", request.Id);
            return new global::GrpcGreeter.DeleteTemplateResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.RestoreTemplateResponse> RestoreTemplate(global::GrpcGreeter.RestoreTemplateRequest request, ServerCallContext context)
    {
        try
        {
            await _emailTemplateService.RestoreTemplateAsync(request.Id);
            return new global::GrpcGreeter.RestoreTemplateResponse
            {
                Success = true,
                Message = "Template restored successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring template: {Id}", request.Id);
            return new global::GrpcGreeter.RestoreTemplateResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GenerateVerifyEmailContentResponse> GenerateVerifyEmailContent(global::GrpcGreeter.GenerateVerifyEmailContentRequest request, ServerCallContext context)
    {
        try
        {
            var content = await _emailTemplateService.GenerateVerifyEmailContentAsync(request.Username, request.VerifyLink, request.Lang);
            return new global::GrpcGreeter.GenerateVerifyEmailContentResponse
            {
                Success = true,
                Message = "Content generated successfully",
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating verify email content");
            return new global::GrpcGreeter.GenerateVerifyEmailContentResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GenerateResetPasswordContentResponse> GenerateResetPasswordContent(global::GrpcGreeter.GenerateResetPasswordContentRequest request, ServerCallContext context)
    {
        try
        {
            var content = await _emailTemplateService.GenerateResetPasswordContentAsync(
                request.Username, 
                request.Email, 
                request.UserId, 
                request.IpAddress, 
                request.ResetLink, 
                request.ExpiryMinutes, 
                request.Lang);
            
            return new global::GrpcGreeter.GenerateResetPasswordContentResponse
            {
                Success = true,
                Message = "Content generated successfully",
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating reset password content");
            return new global::GrpcGreeter.GenerateResetPasswordContentResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GenerateDeactivateAccountContentResponse> GenerateDeactivateAccountContent(global::GrpcGreeter.GenerateDeactivateAccountContentRequest request, ServerCallContext context)
    {
        try
        {
            var content = await _emailTemplateService.GenerateDeactivateAccountContentAsync(request.Username, request.Lang);
            return new global::GrpcGreeter.GenerateDeactivateAccountContentResponse
            {
                Success = true,
                Message = "Content generated successfully",
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating deactivate account content");
            return new global::GrpcGreeter.GenerateDeactivateAccountContentResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GenerateRegisterGoogleContentResponse> GenerateRegisterGoogleContent(global::GrpcGreeter.GenerateRegisterGoogleContentRequest request, ServerCallContext context)
    {
        try
        {
            var content = await _emailTemplateService.GenerateRegisterGoogleContentAsync(request.Username, request.ResetLink, request.Lang);
            return new global::GrpcGreeter.GenerateRegisterGoogleContentResponse
            {
                Success = true,
                Message = "Content generated successfully",
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating register Google content");
            return new global::GrpcGreeter.GenerateRegisterGoogleContentResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<global::GrpcGreeter.GenerateRestoreAccountContentResponse> GenerateRestoreAccountContent(global::GrpcGreeter.GenerateRestoreAccountContentRequest request, ServerCallContext context)
    {
        try
        {
            if (!DateTime.TryParse(request.RestoredAt, out var restoredAt))
            {
                return new global::GrpcGreeter.GenerateRestoreAccountContentResponse
                {
                    Success = false,
                    Message = "Invalid restored date format"
                };
            }

            var content = await _emailTemplateService.GenerateRestoreAccountContentAsync(request.Username, restoredAt, request.Reason, request.Lang);
            return new global::GrpcGreeter.GenerateRestoreAccountContentResponse
            {
                Success = true,
                Message = "Content generated successfully",
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating restore account content");
            return new global::GrpcGreeter.GenerateRestoreAccountContentResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override Task<global::GrpcGreeter.GetSubjectResponse> GetSubject(global::GrpcGreeter.GetSubjectRequest request, ServerCallContext context)
    {
        try
        {
            var subject = _emailTemplateService.GetSubject(request.Type, request.Lang);
            var response = new global::GrpcGreeter.GetSubjectResponse
            {
                Success = true,
                Message = "Subject retrieved successfully",
                Subject = subject
            };
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject for type: {Type}", request.Type);
            var response = new global::GrpcGreeter.GetSubjectResponse
            {
                Success = false,
                Message = "Internal server error"
            };
            return Task.FromResult(response);
        }
    }
}
