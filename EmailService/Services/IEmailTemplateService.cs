using EmailService.Models;
using EmailService.DTOs;

namespace EmailService.Services;

public interface IEmailTemplateService
{
    Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync();
    Task<EmailTemplate?> GetTemplateByIdAsync(int id);
    Task<EmailTemplate?> GetTemplateByNameAndLanguageAsync(string name, string language);
    Task<IEnumerable<EmailTemplate>> GetTemplatesByLanguageAsync(string language);
    Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template);
    Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template);
    Task DeleteTemplateAsync(int id);
    Task RestoreTemplateAsync(int id);
    
    string LoadTemplate(string templateName, string lang = null);
    Task<string> LoadTemplateAsync(string templateName, string lang = null);
    string ReplacePlaceholders(string template, Dictionary<string, string> placeholders);
    
    Task<string> GenerateVerifyEmailContentAsync(string username, string verifyLink, string lang = null);
    Task<string> GenerateResetPasswordContentAsync(string username, string email, string userId, string ipAddress, string resetLink, int expiryMinutes, string lang = null);
    Task<string> GenerateDeactivateAccountContentAsync(string username, string lang = null);
    Task<string> GenerateRegisterGoogleContentAsync(string username, string resetLink = "", string lang = null);
    Task<string> GenerateRestoreAccountContentAsync(string username, DateTime restoredAt, string reason, string lang = null);
    
    string GenerateVerifyEmailContent(string username, string verifyLink, string lang = null);
    string GenerateResetPasswordContent(string username, string email, string userId, string ipAddress, string resetLink, int expiryMinutes, string lang = null);
    string GenerateDeactivateAccountContent(string username, string lang = null);
    string GenerateRegisterGoogleContent(string username, string resetLink = "", string lang = null);
    string GenerateRestoreAccountContent(string username, DateTime restoredAt, string reason, string lang = null);
    
    string GetSubject(string type, string lang = null);
} 