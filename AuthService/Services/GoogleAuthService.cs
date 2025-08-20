using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using Shared.EmailModels;
using System.Security.Cryptography;
using Shared.AuditModels;
using Shared.Services;
using System.Net.Http;

namespace AuthService.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionService _sessionService;
        private readonly IJwtService _jwtService;
        private readonly IEmailMessageService _emailMessageService;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IAuditHelper _auditHelper;

        public GoogleAuthService(
            IUserRepository userRepository,
            ISessionService sessionService,
            IJwtService jwtService,
            IEmailMessageService emailMessageService,
            IConfiguration config,
            HttpClient httpClient,
            ILogger<GoogleAuthService> logger,
            IAuditHelper auditHelper)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
            _jwtService = jwtService;
            _emailMessageService = emailMessageService;
            _config = config;
            _httpClient = httpClient;
            _logger = logger;
            _auditHelper = auditHelper;
        }

        public async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidGoogleTokenException("Invalid Google access token");
                }

                var content = await response.Content.ReadAsStringAsync();
                
                var googleUserInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (googleUserInfo == null || string.IsNullOrEmpty(googleUserInfo.Email))
                {
                    throw new InvalidGoogleTokenException("Invalid user info from Google");
                }

                if (!string.IsNullOrEmpty(googleUserInfo.Picture))
                {
                    googleUserInfo.Picture = ValidateAndFixGooglePictureUrl(googleUserInfo.Picture);
                }

                return googleUserInfo;
            }
            catch (Exception ex) when (ex is not InvalidGoogleTokenException)
            {
                throw new InvalidGoogleTokenException("Failed to verify Google token");
            }
            catch (Exception ex)
            {
                // Audit: google login failed (unexpected)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new AuthAuditEvent
                        {
                            UserEmail = null,
                            Action = "GOOGLE_LOGIN_FAILED",
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch { }
                });
                throw;
            }
        }

        private string ValidateAndFixGooglePictureUrl(string pictureUrl)
        {
            try
            {
                if (!Uri.TryCreate(pictureUrl, UriKind.Absolute, out var uri))
                {
                    return string.Empty;
                }

                if (uri.Scheme != "https")
                {
                    return pictureUrl.Replace("http://", "https://");
                }

                return pictureUrl;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool VerifyOtpCode(string otpCode, string twoFactorSecret)
        {
            if (string.IsNullOrWhiteSpace(otpCode) || string.IsNullOrWhiteSpace(twoFactorSecret))
                return false;
                
            try
            {
                var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(twoFactorSecret));
                return totp.VerifyTotp(otpCode.Trim(), out long _, OtpNet.VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch
            {
                return false;
            }
        }

        public async Task<(string? token, bool require2FA, Guid? userId, string? message)> LoginWithGoogleAsync(GoogleLoginDto dto)
        {
            _logger.LogInformation("GoogleAuthService.LoginWithGoogleAsync started");
            
            if (dto == null || string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.RedirectUri))
            {
                return (null, false, null, "Invalid request parameters");
            }

            try
            {
                var clientId = _config["GoogleAuth:ClientId"];
                var clientSecret = _config["GoogleAuth:ClientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogError("Google OAuth configuration is missing");
                    return (null, false, null, "Google OAuth configuration error");
                }

                var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", dto.Code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", dto.RedirectUri)
                });

                var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Google token request failed: {StatusCode}, {Content}", tokenResponse.StatusCode, tokenContent);
                    throw new Exceptions.InvalidGoogleTokenException("Failed to exchange Google OAuth code for token");
                }

                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
                var accessToken = tokenData.GetProperty("access_token").GetString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exceptions.InvalidGoogleTokenException("Access token not found in Google response");
                }

                var googleUserInfo = await GetGoogleUserInfoAsync(accessToken);
                if (googleUserInfo == null)
                {
                    throw new Exceptions.InvalidGoogleTokenException("Failed to get user info from Google");
                }

                var existingUser = await _userRepository.GetByGoogleIdAsync(googleUserInfo.Sub);
                bool isNewUser = false;

                if (existingUser == null)
                {
                    existingUser = await _userRepository.GetByEmailAsync(googleUserInfo.Email);
                    if (existingUser != null)
                    {
                        existingUser.GoogleId = googleUserInfo.Sub;
                        existingUser.LoginProvider = "Google";
                        existingUser.UpdatedAt = DateTime.UtcNow;
                        await _userRepository.UpdateAsync(existingUser);
                    }
                    else
                    {
                        existingUser = new User
                        {
                            Username = await GenerateUniqueUsernameFromGoogleInfoAsync(googleUserInfo),
                            FullName = googleUserInfo.Name,
                            Email = googleUserInfo.Email,
                            GoogleId = googleUserInfo.Sub,
                            ProfilePicture = ValidateAndFixGooglePictureUrl(googleUserInfo.Picture),
                            LoginProvider = "Google",
                            CreatedAt = DateTime.UtcNow,
                            IsVerified = true,
                            Status = UserStatus.Active,
                        };

                        await _userRepository.AddAsync(existingUser);
                        isNewUser = true;
                    }
                }

                string resetToken = "";
                if (isNewUser)
                {
                    resetToken = GenerateResetToken();
                    var resetTokenExpiry = TimeSpan.FromHours(1);
                    await _sessionService.StoreResetTokenAsync(resetToken, existingUser.Id, resetTokenExpiry);
                    
                    await _emailMessageService.PublishRegisterGoogleNotificationAsync(new RegisterGoogleNotificationEmailEvent
                    {
                        To = existingUser.Email,
                        Username = existingUser.FullName ?? existingUser.Username,
                        Token = resetToken,
                        RegisterAt = DateTime.UtcNow,
                        Language = dto?.Language ?? "en"
                    });
                }

                if (existingUser.TwoFactorEnabled && !string.IsNullOrEmpty(existingUser.TwoFactorSecret))
                {
                    if (string.IsNullOrWhiteSpace(dto?.OtpCode))
                    {
                        _logger.LogInformation("2FA enabled but no OTP code provided for user {UserId}", existingUser.Id);
                        // Audit: google login requires 2FA
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var auditEvent = new AuthAuditEvent
                                {
                                    UserId = existingUser.Id.ToString(),
                                    UserEmail = existingUser.Email,
                                    Action = "GOOGLE_LOGIN_2FA_REQUIRED",
                                    ResourceId = existingUser.Id.ToString(),
                                    Success = false,
                                    ErrorMessage = "2FA code required"
                                };
                                await _auditHelper.LogEventAsync(auditEvent);
                            }
                            catch { }
                        });
                        return (null, true, existingUser.Id, "2FA code required");
                    }
                    
                    var otpCode = dto.OtpCode ?? string.Empty;
                    if (!VerifyOtpCode(otpCode, existingUser.TwoFactorSecret))
                    {
                        _logger.LogWarning("Invalid OTP code for user {UserId}", existingUser.Id);
                        // Audit: google login 2FA failed
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var auditEvent = new AuthAuditEvent
                                {
                                    UserId = existingUser.Id.ToString(),
                                    UserEmail = existingUser.Email,
                                    Action = "GOOGLE_LOGIN_2FA_FAILED",
                                    ResourceId = existingUser.Id.ToString(),
                                    Success = false,
                                    ErrorMessage = "Invalid 2FA code"
                                };
                                await _auditHelper.LogEventAsync(auditEvent);
                            }
                            catch { }
                        });
                        return (null, true, existingUser.Id, "Invalid 2FA code");
                    }
                    _logger.LogInformation("OTP verification successful for user {UserId}", existingUser.Id);
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var auditEvent = new AuthAuditEvent
                            {
                                UserId = existingUser.Id.ToString(),
                                UserEmail = existingUser.Email,
                                Action = "GOOGLE_LOGIN_2FA_VERIFIED",
                                ResourceId = existingUser.Id.ToString(),
                                Success = true
                            };
                            await _auditHelper.LogEventAsync(auditEvent);
                        }
                        catch { }
                    });
                }

                var token = _jwtService.GenerateToken(existingUser, dto?.Language ?? "en");
                var tokenExpiry = _jwtService.GetTokenExpirationTimeSpan(token);

                await _sessionService.StoreActiveTokenAsync(token, existingUser.Id, tokenExpiry);
                
                var sessionId = Guid.NewGuid().ToString();
                await _sessionService.CreateUserSessionAsync(existingUser.Id, sessionId, tokenExpiry);
                await _sessionService.SetUserLoginStatusAsync(existingUser.Id, true, tokenExpiry);
                
                existingUser.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(existingUser);

                // Audit: google login success
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new AuthAuditEvent
                        {
                            UserId = existingUser.Id.ToString(),
                            UserEmail = existingUser.Email,
                            Action = "GOOGLE_LOGIN",
                            ResourceId = existingUser.Id.ToString(),
                            Success = true,
                            Metadata = new Dictionary<string, object>
                            {
                                { "LoginProvider", "Google" },
                                { "TwoFactorUsed", existingUser.TwoFactorEnabled && !string.IsNullOrEmpty(existingUser.TwoFactorSecret) && !string.IsNullOrWhiteSpace(dto?.OtpCode) },
                                { "Language", dto?.Language ?? "en" }
                            }
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch { }
                });

                return (token, false, existingUser.Id, "Login successful");
            }
            catch (Exceptions.InvalidGoogleTokenException ex)
            {
                // Audit: google login failed
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var auditEvent = new AuthAuditEvent
                        {
                            UserEmail = null,
                            Action = "GOOGLE_LOGIN_FAILED",
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                        await _auditHelper.LogEventAsync(auditEvent);
                    }
                    catch { }
                });
                throw;
            }
        }

        public async Task<(bool success, string? token, string? message)> VerifyGoogleOtpAsync(Guid userId, string otpCode, string? language)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, null, "User not found");
            }
            
            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                return (false, null, "2FA not enabled");
            }
            
            if (!VerifyOtpCode(otpCode, user.TwoFactorSecret))
            {
                return (false, null, "Invalid 2FA code");
            }
            
            var token = _jwtService.GenerateToken(user, language ?? "en");
            var tokenExpiry = _jwtService.GetTokenExpirationTimeSpan(token);
            await _sessionService.StoreActiveTokenAsync(token, user.Id, tokenExpiry);
            var sessionId = Guid.NewGuid().ToString();
            await _sessionService.CreateUserSessionAsync(user.Id, sessionId, tokenExpiry);
            await _sessionService.SetUserLoginStatusAsync(user.Id, true, tokenExpiry);
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return (true, token, "Login successful");
        }

        private async Task<string> GenerateUniqueUsernameFromGoogleInfoAsync(GoogleUserInfo googleUserInfo)
        {
            var baseUsername = googleUserInfo.Email != null && googleUserInfo.Email.Contains("@")
                ? googleUserInfo.Email.Split('@')[0]
                : GenerateBaseUsernameFromGoogleInfo(googleUserInfo);
            
            baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, @"[^a-zA-Z0-9_]", "");
            
            if (string.IsNullOrEmpty(baseUsername))
                baseUsername = "user";

            var username = baseUsername;
            var counter = 1;
            const int maxAttempts = 100;
            
            while (counter <= maxAttempts)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(username);
                if (existingUser == null)
                {
                    return username;
                }
                
                username = $"{baseUsername}{counter}";
                counter++;
            }
            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            username = $"{baseUsername}{timestamp}";
            
            var finalCheck = await _userRepository.GetByUsernameAsync(username);
            if (finalCheck == null)
            {
                return username;
            }
            
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{baseUsername}{guid}";
        }

        private string GenerateBaseUsernameFromGoogleInfo(GoogleUserInfo googleUserInfo)
        {
            var baseUsername = googleUserInfo.GivenName?.ToLower() ?? googleUserInfo.Name?.ToLower() ?? "user";
            
            baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, @"[^a-zA-Z0-9_]", "");
            
            if (string.IsNullOrEmpty(baseUsername))
                baseUsername = "user";

            var randomSuffix = new Random().Next(1000, 9999);
            return $"{baseUsername}{randomSuffix}";
        }

        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
} 