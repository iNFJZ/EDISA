using System.Text;
using System.Text.Json;
using EmailService.Repositories;
using EmailService.Models;

namespace EmailService.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailTemplateRepository _templateRepository;
        private readonly IConfiguration _config;
        private readonly Dictionary<string, Dictionary<string, object>> _langFiles;

        public EmailTemplateService(IEmailTemplateRepository templateRepository, IConfiguration config)
        {
            _templateRepository = templateRepository;
            _config = config;
            _langFiles = LoadLangFiles();
        }

        private Dictionary<string, Dictionary<string, object>> LoadLangFiles()
        {
            var langFiles = new Dictionary<string, Dictionary<string, object>>();
            var sharedPath = _config["Frontend:Path"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Shared", "LanguageFiles");
            
            var languages = new[] { "en", "vi", "ja" };
            
            foreach (var lang in languages)
            {
                var langFile = Path.Combine(sharedPath, $"{lang}.json");
                if (File.Exists(langFile))
                {
                    try
                    {
                        var json = File.ReadAllText(langFile, Encoding.UTF8);
                        var langData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                        langFiles[lang] = langData;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            
            return langFiles;
        }

        public async Task<string> LoadTemplateAsync(string templateName, string lang = null)
        {
            lang = string.IsNullOrEmpty(lang) ? "en" : lang;
            
            var template = await _templateRepository.GetByNameAndLanguageAsync(templateName, lang);
            if (template == null)
            {
                if (lang != "en")
                {
                    template = await _templateRepository.GetByNameAndLanguageAsync(templateName, "en");
                }
                
                if (template == null)
                {
                    throw new InvalidOperationException($"Template {templateName} not found for language {lang}");
                }
            }
            
            return template.Body;
        }

        public string LoadTemplate(string templateName, string lang = null)
        {
            return LoadTemplateAsync(templateName, lang).GetAwaiter().GetResult();
        }

        public string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            var result = template;
            
            foreach (var placeholder in placeholders)
            {
                result = result.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }

            return result;
        }

        public async Task<string> GenerateVerifyEmailContentAsync(string username, string verifyLink, string lang = null)
        {
            var template = await LoadTemplateAsync("verify-email", lang);
            var placeholders = new Dictionary<string, string>
            {
                { "Username", username },
                { "VerifyLink", verifyLink }
            };
            return ReplacePlaceholders(template, placeholders);
        }

        public string GenerateVerifyEmailContent(string username, string verifyLink, string lang = null)
        {
            return GenerateVerifyEmailContentAsync(username, verifyLink, lang).GetAwaiter().GetResult();
        }

        public async Task<string> GenerateResetPasswordContentAsync(string username, string email, string userId, string ipAddress, string resetLink, int expiryMinutes, string lang = null)
        {
            var template = await LoadTemplateAsync("reset-password", lang);
            var placeholders = new Dictionary<string, string>
            {
                { "Username", username },
                { "Username", username },
                { "Email", email },
                { "UserId", userId },
                { "IpAddress", ipAddress },
                { "ResetLink", resetLink },
                { "ExpiryMinutes", expiryMinutes.ToString() }
            };
            return ReplacePlaceholders(template, placeholders);
        }

        public string GenerateResetPasswordContent(string username, string email, string userId, string ipAddress, string resetLink, int expiryMinutes, string lang = null)
        {
            return GenerateResetPasswordContentAsync(username, email, userId, ipAddress, resetLink, expiryMinutes, lang).GetAwaiter().GetResult();
        }

        public async Task<string> GenerateDeactivateAccountContentAsync(string username, string lang = null)
        {
            var template = await LoadTemplateAsync("deactivate-account", lang);
            var placeholders = new Dictionary<string, string>
            {
                { "Username", username }
            };
            return ReplacePlaceholders(template, placeholders);
        }

        public string GenerateDeactivateAccountContent(string username, string lang = null)
        {
            return GenerateDeactivateAccountContentAsync(username, lang).GetAwaiter().GetResult();
        }

        public async Task<string> GenerateRegisterGoogleContentAsync(string username, string resetLink = "", string lang = null)
        {
            var template = await LoadTemplateAsync("register-google", lang);
            var placeholders = new Dictionary<string, string>
            {
                { "Username", username },
                { "ResetLink", resetLink }
            };
            return ReplacePlaceholders(template, placeholders);
        }

        public string GenerateRegisterGoogleContent(string username, string resetLink = "", string lang = null)
        {
            return GenerateRegisterGoogleContentAsync(username, resetLink, lang).GetAwaiter().GetResult();
        }

        public async Task<string> GenerateRestoreAccountContentAsync(string username, DateTime restoredAt, string reason, string lang = null)
        {
            var template = await LoadTemplateAsync("restore-account", lang);
            string localizedReason = reason;
            if (reason == "Account restored by administrator")
            {
                switch ((lang ?? "en").ToLower())
                {
                    case "vi":
                        localizedReason = "Tài khoản được quản trị viên khôi phục";
                        break;
                    case "ja":
                        localizedReason = "管理者によってアカウントが復元されました";
                        break;
                    default:
                        localizedReason = "Account restored by administrator";
                        break;
                }
            }
            var placeholders = new Dictionary<string, string>
            {
                { "Username", username },
                { "RestoredAt", restoredAt.ToString("dd/MM/yyyy HH:mm:ss UTC") },
                { "Reason", localizedReason },
                { "LoginUrl", _config["Frontend:BaseUrl"] + "/auth/login.html" }
            };
            return ReplacePlaceholders(template, placeholders);
        }

        public string GenerateRestoreAccountContent(string username, DateTime restoredAt, string reason, string lang = null)
        {
            return GenerateRestoreAccountContentAsync(username, restoredAt, reason, lang).GetAwaiter().GetResult();
        }

        public string GetSubject(string type, string lang = null)
        {
            lang = string.IsNullOrEmpty(lang) ? "en" : lang;
            if (_langFiles.TryGetValue(lang, out var langData))
            {
                if (langData.TryGetValue("emailSubjects", out var emailSubjectsObj) && emailSubjectsObj is JsonElement emailSubjects)
                {
                    if (emailSubjects.TryGetProperty(type, out var subject))
                    {
                        return subject.GetString() ?? type;
                    }
                }
            }
            if (lang != "en" && _langFiles.TryGetValue("en", out var enLangData))
            {
                if (enLangData.TryGetValue("emailSubjects", out var enEmailSubjectsObj) && enEmailSubjectsObj is JsonElement enEmailSubjects)
                {
                    if (enEmailSubjects.TryGetProperty(type, out var subject))
                    {
                        return subject.GetString() ?? type;
                    }
                }
            }
            return type;
        }

        // CRUD Operations
        public async Task<IEnumerable<EmailTemplate>> GetAllTemplatesAsync()
        {
            return await _templateRepository.GetAllAsync();
        }

        public async Task<EmailTemplate?> GetTemplateByIdAsync(int id)
        {
            return await _templateRepository.GetByIdAsync(id);
        }

        public async Task<EmailTemplate?> GetTemplateByNameAndLanguageAsync(string name, string language)
        {
            return await _templateRepository.GetByNameAndLanguageAsync(name, language);
        }

        public async Task<IEnumerable<EmailTemplate>> GetTemplatesByLanguageAsync(string language)
        {
            return await _templateRepository.GetByLanguageAsync(language);
        }

        public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template)
        {
            template.CreatedAt = DateTime.UtcNow;
            return await _templateRepository.CreateAsync(template);
        }

        public async Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            return await _templateRepository.UpdateAsync(template);
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var result = await _templateRepository.DeleteAsync(id);
            if (!result)
            {
                throw new InvalidOperationException($"Template with ID {id} not found or could not be deleted");
            }
        }

        public async Task RestoreTemplateAsync(int id)
        {
            var result = await _templateRepository.RestoreAsync(id);
            if (!result)
            {
                throw new InvalidOperationException($"Template with ID {id} not found or could not be restored");
            }
        }
    }
} 