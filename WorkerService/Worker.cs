using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Net.Mail;
using System.Threading;
using System.Globalization;
using Shared.EmailModels;
using EmailService.Services;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IEmailTemplateService _emailTemplateService;
        private IConnection _connection;
        private IModel _channel;
        private string _smtpUser;
        private string _smtpPass;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _smtpEnableSsl;
        private readonly int _resetTokenExpiryMinutes;

        public Worker(ILogger<Worker> logger, IConfiguration config, IEmailTemplateService emailTemplateService)
        {
            _logger = logger;
            _config = config;
            _emailTemplateService = emailTemplateService;
            _smtpUser = _config["Smtp:User"];
            _smtpPass = _config["Smtp:Password"];
            _smtpHost = _config["Smtp:Host"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
            _smtpEnableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "true");
            _resetTokenExpiryMinutes = int.Parse(_config["EmailPolicy:ResetTokenExpiryMinutes"] ?? "15");
            Task.Run(() => InitRabbitMQ()).GetAwaiter().GetResult();
        }

        private async Task InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:HostName"],
                Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                UserName = _config["RabbitMQ:UserName"],
                Password = _config["RabbitMQ:Password"],
                VirtualHost = _config["RabbitMQ:VirtualHost"] ?? "/"
            };
            int retry = 0;
            const int maxRetry = 10;
            const int delaySeconds = 5;
            while (true)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: "email.notifications", durable: true, exclusive: false, autoDelete: false);
                    break;
                }
                catch (Exception ex)
                {
                    retry++;
                    if (retry >= maxRetry)
                    {
                        throw;
                    }
                    await Task.Delay(delaySeconds * 1000);
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    using var doc = JsonDocument.Parse(message);
                    var root = doc.RootElement;
                    
                    string? eventType = null;
                    if (ea.BasicProperties.Headers != null && 
                        ea.BasicProperties.Headers.ContainsKey("event_type"))
                    {
                        eventType = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["event_type"]);
                    }
                    
                    switch (eventType)
                    {
                        case "RestoreAccountEmailEvent":
                            var restoreEvent = JsonSerializer.Deserialize<RestoreAccountEmailEvent>(message);
                            if (restoreEvent != null)
                            {
                                SendRestoreAccountMail(restoreEvent);
                            }
                            break;
                        default:
                            if (root.TryGetProperty("EventType", out _))
                            {
                                var fileEvent = JsonSerializer.Deserialize<Shared.EmailModels.FileEventEmailNotification>(message);
                                if (fileEvent != null && !string.IsNullOrEmpty(fileEvent.To))
                                {
                                    SendFileEventMail(fileEvent);
                                }
                            }
                            else if (root.TryGetProperty("ResetToken", out _))
                            {
                                var resetEvent = JsonSerializer.Deserialize<ResetPasswordEmailEvent>(message);
                                if (resetEvent != null && !string.IsNullOrEmpty(resetEvent.To))
                                {
                                    SendResetPasswordMail(resetEvent);
                                }
                            }
                            else if (root.TryGetProperty("ChangeAt", out _))
                            {
                                var changeEvent = JsonSerializer.Deserialize<ChangePasswordEmailEvent>(message);
                                if (changeEvent != null && !string.IsNullOrEmpty(changeEvent.To))
                                {
                                    SendChangePasswordMail(changeEvent);
                                }
                            }
                            else if (root.TryGetProperty("DeactivatedAt", out _))
                            {
                                var deactivateEvent = JsonSerializer.Deserialize<DeactivateAccountEmailEvent>(message);
                                if (deactivateEvent != null && !string.IsNullOrEmpty(deactivateEvent.To))
                                {
                                    SendDeactivateAccountMail(deactivateEvent);
                                }
                            }
                            else if (root.TryGetProperty("RegisterAt", out _) && root.TryGetProperty("Username", out _) && !root.TryGetProperty("VerifyLink", out _))
                            {
                                var googleEvent = JsonSerializer.Deserialize<RegisterGoogleNotificationEmailEvent>(message);
                                if (googleEvent != null && !string.IsNullOrEmpty(googleEvent.To))
                                {
                                    SendRegisterGoogleMail(googleEvent);
                                }
                            }
                            else
                            {
                                var registerEvent = JsonSerializer.Deserialize<RegisterNotificationEmailEvent>(message);
                                if (registerEvent != null && !string.IsNullOrEmpty(registerEvent.To))
                                {
                                    SendRegisterMail(registerEvent);
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: "email.notifications", autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        }

        private async void SendRegisterMail(RegisterNotificationEmailEvent emailEvent)
        {
            try
            {
                var registerAt = emailEvent.RegisterAt == DateTime.MinValue ? DateTime.UtcNow : emailEvent.RegisterAt;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(registerAt, GetVietnamTimeZone());
                
                var language = !string.IsNullOrEmpty(emailEvent.Language) ? emailEvent.Language : "vi";
                
                string emailBody;
                string subject;
                
                if (!string.IsNullOrEmpty(emailEvent.VerifyLink))
                {
                    emailBody = await _emailTemplateService.LoadTemplateAsync("verify-email", language);
                    var placeholders = new Dictionary<string, string>
                    {
                        { "Username", emailEvent.Username },
                        { "VerifyLink", emailEvent.VerifyLink }
                    };
                    emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                    subject = _emailTemplateService.GetSubject("register", language);
                }
                else
                {
                    emailBody = await _emailTemplateService.LoadTemplateAsync("register", language);
                    var placeholders = new Dictionary<string, string>
                    {
                        { "Username", emailEvent.Username }
                    };
                    emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                    subject = _emailTemplateService.GetSubject("register", language);
                }
                
                var mail = new MailMessage();
                mail.To.Add(emailEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");
                mail.IsBodyHtml = true;
                mail.Body = emailBody;
                
                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"Register email sent to {emailEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send register email to {emailEvent.To}");
            }
        }

        private async void SendFileEventMail(Shared.EmailModels.FileEventEmailNotification emailEvent)
        {
            try
            {
                var eventTime = emailEvent.EventTime == DateTime.MinValue ? DateTime.UtcNow : emailEvent.EventTime;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(eventTime, GetVietnamTimeZone());
                
                var eventType = emailEvent.EventType?.ToLowerInvariant();
                string templateName;
                
                templateName = eventType switch
                {
                    "upload" => "file-upload",
                    "download" => "file-download", 
                    "delete" => "file-delete",
                    _ => "file-event"
                };
                
                var language = !string.IsNullOrEmpty(emailEvent.Language) ? emailEvent.Language : "vi";
                
                var emailBody = await _emailTemplateService.LoadTemplateAsync(templateName, language);
                var placeholders = new Dictionary<string, string>
                {
                    { "Username", emailEvent.Username },
                    { "FileName", emailEvent.FileName },
                    { "FileSize", emailEvent.FileSize ?? "Unknown" },
                    { "IpAddress", emailEvent.IpAddress ?? "Unknown" },
                    { eventType switch
                        {
                            "upload" => "UploadTime",
                            "download" => "DownloadTime", 
                            "delete" => "DeleteTime",
                            _ => "UploadTime"
                        }, vnTime.ToString("dd/MM/yyyy HH:mm:ss") }
                };
                
                emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                
                var subject = _emailTemplateService.GetSubject(eventType switch
                {
                    "upload" => "fileUpload",
                    "download" => "fileDownload", 
                    "delete" => "fileDelete",
                    _ => "fileUpload"
                }, language);
                
                var mail = new MailMessage();
                mail.To.Add(emailEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");
                mail.IsBodyHtml = true;
                mail.Body = emailBody;
                
                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"File event email sent to {emailEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send file event email to {emailEvent.To}");
            }
        }

        private async void SendResetPasswordMail(ResetPasswordEmailEvent emailEvent)
        {
            try
            {
                var requestedAt = emailEvent.RequestedAt == DateTime.MinValue ? DateTime.UtcNow : emailEvent.RequestedAt;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(requestedAt, GetVietnamTimeZone());
                
                string emailBody;
                string subject;
                
                var language = !string.IsNullOrEmpty(emailEvent.Language) ? emailEvent.Language : "vi";
                
                if (!string.IsNullOrEmpty(emailEvent.ResetLink))
                {
                    emailBody = await _emailTemplateService.LoadTemplateAsync("reset-password", language);
                    var placeholders = new Dictionary<string, string>
                    {
                        { "Username", emailEvent.Username },
                        { "Email", emailEvent.To },
                        { "UserId", emailEvent.UserId ?? string.Empty },
                        { "IpAddress", emailEvent.IpAddress ?? "Unknown" },
                        { "ResetLink", emailEvent.ResetLink },
                        { "ExpiryMinutes", _resetTokenExpiryMinutes.ToString() }
                    };
                    emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                    subject = _emailTemplateService.GetSubject("resetPassword", language);
                }
                else
                {
                    emailBody = await _emailTemplateService.LoadTemplateAsync("reset-password", language);
                    var placeholders = new Dictionary<string, string>
                    {
                        { "Username", emailEvent.Username },
                        { "Email", emailEvent.To },
                        { "UserId", emailEvent.UserId ?? string.Empty },
                        { "IpAddress", emailEvent.IpAddress ?? "Unknown" },
                        { "ResetToken", emailEvent.ResetToken },
                        { "ExpiryMinutes", _resetTokenExpiryMinutes.ToString() }
                    };
                    emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                    subject = _emailTemplateService.GetSubject("resetPassword", language);
                }
                
                var mail = new MailMessage();
                mail.To.Add(emailEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");
                mail.IsBodyHtml = true;
                mail.Body = emailBody;
                
                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"Reset password email sent to {emailEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send reset password email to {emailEvent.To}");
            }
        }

        private async void SendChangePasswordMail(ChangePasswordEmailEvent emailEvent)
        {
            try
            {
                var changeAt = emailEvent.ChangeAt == DateTime.MinValue ? DateTime.UtcNow : emailEvent.ChangeAt;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(changeAt, GetVietnamTimeZone());
                
                var language = !string.IsNullOrEmpty(emailEvent.Language) ? emailEvent.Language : "vi";
                
                var emailBody = await _emailTemplateService.LoadTemplateAsync("change-password", language);
                var placeholders = new Dictionary<string, string>
                {
                    { "Username", emailEvent.Username ?? "Người dùng" }
                };
                emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                var subject = _emailTemplateService.GetSubject("changePassword", language);
                
                var mail = new MailMessage();
                mail.To.Add(emailEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");
                mail.IsBodyHtml = true;
                mail.Body = emailBody;
                
                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"Change password email sent to {emailEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send change password email to {emailEvent.To}");
            }
        }

        private async void SendDeactivateAccountMail(DeactivateAccountEmailEvent emailEvent)
        {
            try
            {
                var deactivatedAt = emailEvent.DeactivatedAt == DateTime.MinValue ? DateTime.UtcNow : emailEvent.DeactivatedAt;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(deactivatedAt, GetVietnamTimeZone());
                
                var language = !string.IsNullOrEmpty(emailEvent.Language) ? emailEvent.Language : "vi";
                
                var emailBody = await _emailTemplateService.LoadTemplateAsync("deactivate-account", language);
                var placeholders = new Dictionary<string, string>
                {
                    { "Username", emailEvent.Username }
                };
                emailBody = _emailTemplateService.ReplacePlaceholders(emailBody, placeholders);
                var subject = _emailTemplateService.GetSubject("deactivateAccount", language);
                
                var mail = new MailMessage();
                mail.To.Add(emailEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");
                mail.IsBodyHtml = true;
                mail.Body = emailBody;
                
                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"Deactivate account email sent to {emailEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send deactivate account email to {emailEvent.To}");
            }
        }

        private async void SendRegisterGoogleMail(RegisterGoogleNotificationEmailEvent emailEvent)
        {
            try
            {
                var registerAt = emailEvent.RegisterAt == DateTime.MinValue ? DateTime.UtcNow : emailEvent.RegisterAt;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(registerAt, GetVietnamTimeZone());
                
                string resetLink = "";
                if (!string.IsNullOrEmpty(emailEvent.Token))
                {
                    resetLink = $"{_config["Frontend:BaseUrl"]}/auth/reset-password.html?token={emailEvent.Token}";
                }
                var language = !string.IsNullOrEmpty(emailEvent.Language) ? emailEvent.Language : "vi";
                
                var emailBody = await _emailTemplateService.GenerateRegisterGoogleContentAsync(
                    emailEvent.Username, 
                    resetLink, 
                    language
                );
                var subject = _emailTemplateService.GetSubject("registerGoogle", language);
                
                var mail = new MailMessage();
                mail.To.Add(emailEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");     
                mail.IsBodyHtml = true;
                mail.Body = emailBody;
                
                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"Google register email sent to {emailEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send Google register email to {emailEvent.To}");
            }
        }
        
        private async Task SendRestoreAccountMail(RestoreAccountEmailEvent restoreEvent)
        {
            try
            {
                var restoredAt = restoreEvent.RestoredAt == DateTime.MinValue ? DateTime.UtcNow : restoreEvent.RestoredAt;
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(restoredAt, GetVietnamTimeZone());
                
                var language = !string.IsNullOrEmpty(restoreEvent.Language) ? restoreEvent.Language : "vi";
                
                var emailBody = await _emailTemplateService.GenerateRestoreAccountContentAsync(
                    restoreEvent.Username, 
                    restoredAt, 
                    restoreEvent.Reason,
                    language
                );
                var subject = _emailTemplateService.GetSubject("restoreAccount", language);
                
                var mail = new MailMessage();
                mail.To.Add(restoreEvent.To);
                mail.Subject = subject;
                mail.From = new MailAddress(_smtpUser, "EDISA");
                mail.IsBodyHtml = true;
                mail.Body = emailBody;

                using var smtp = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = _smtpEnableSsl
                };
                smtp.Send(mail);
                _logger.LogInformation($"Restore account email sent to {restoreEvent.To} in {language}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send restore account email to {restoreEvent.To}");
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
