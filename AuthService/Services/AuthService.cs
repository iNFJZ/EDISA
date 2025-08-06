using AuthService.Data;
using AuthService.DTOs;
using AuthService.Exceptions;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Shared.EmailModels;
using Shared.LanguageService;
using OtpNet;
using System.Drawing;
using QRCoder;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly ISessionService _sessionService;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailMessageService _emailMessageService;
        private readonly IConfiguration _config;
        private readonly int _maxFailedLoginAttempts;
        private readonly int _accountLockMinutes;
        private readonly int _resetPasswordTokenExpiryMinutes;
        private readonly IHunterEmailVerifierService _emailVerifierService;
        private readonly INotificationService _notificationService;

        public AuthService(
            IUserRepository repo, 
            ISessionService sessionService,
            IJwtService jwtService,
            IPasswordService passwordService,
            ILogger<AuthService> logger,
            IEmailMessageService emailMessageService,
            IConfiguration config,
            IHunterEmailVerifierService emailVerifierService,
            INotificationService notificationService)
        {
            _repo = repo;
            _sessionService = sessionService;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _logger = logger;
            _emailMessageService = emailMessageService;
            _config = config;
            _emailVerifierService = emailVerifierService;
            _notificationService = notificationService;
            _maxFailedLoginAttempts = int.Parse(_config["AuthSettings:MaxFailedLoginAttempts"] ?? "5");
            _accountLockMinutes = int.Parse(_config["AuthSettings:AccountLockMinutes"] ?? "30");
            _resetPasswordTokenExpiryMinutes = int.Parse(_config["AuthSettings:ResetPasswordTokenExpiryMinutes"] ?? "60");
        }

        public async Task<(string token, string username, string suggestedUsername, string errorCode, string message)> RegisterAsync(RegisterWithAcceptDto dto, bool acceptSuggestedUsername = false)
        {
            var sanitizedEmail = dto.Email?.Trim().ToLowerInvariant();
            var sanitizedUsername = dto.Username?.Trim();
            var sanitizedFullName = dto.FullName?.Trim();
            
            if (string.IsNullOrWhiteSpace(sanitizedEmail) || string.IsNullOrWhiteSpace(sanitizedUsername))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Email and username are required");
            
            try
            {
                var emailAddress = new System.Net.Mail.MailAddress(sanitizedEmail);
                if (emailAddress.Address != sanitizedEmail)
                    throw new AuthException("INVALID_EMAIL_FORMAT", "Invalid email format");
            }
            catch
            {
                throw new AuthException("INVALID_EMAIL_FORMAT", "Invalid email format");
            }
            
            if (sanitizedUsername.Length < 3 || sanitizedUsername.Length > 50)
                throw new AuthException("FIELD_TOO_SHORT", "Username must be between 3 and 50 characters");
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(sanitizedUsername, @"^[a-zA-Z0-9]+$"))
                throw new AuthException("INVALID_CHARACTERS", "Username can only contain letters and numbers");
            
            if (sanitizedFullName != null && !System.Text.RegularExpressions.Regex.IsMatch(sanitizedFullName, @"^[a-zA-ZÀ-ỹ\s]+$"))
                throw new AuthException("INVALID_CHARACTERS", "Full name can only contain letters, spaces, and Vietnamese characters");
            
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                var phoneNumber = dto.PhoneNumber.Trim();
                if (phoneNumber.Length > 11)
                    throw new AuthException("INVALID_PHONE", "Phone number must be less than 11 characters");
                if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[0-9]{10,11}$"))
                    throw new AuthException("INVALID_PHONE", "Phone number must be 10-11 digits and contain only numbers");
            }
            
            var emailExists = await _emailVerifierService.VerifyEmailAsync(sanitizedEmail);
            if (!emailExists)
                throw new EmailNotExistsException(sanitizedEmail);

            var existingUser = await _repo.GetByEmailIncludeDeletedAsync(sanitizedEmail);
            if (existingUser != null)
            {
                if (existingUser.IsDeleted)
                    throw new AccountDeletedException();
                else
                    throw new UserAlreadyExistsException(sanitizedEmail);
            }

            var existingUsername = await _repo.GetByUsernameAsync(sanitizedUsername);
            if (existingUsername != null)
            {
                if (!acceptSuggestedUsername)
                {
                    var suggestedUsername = await GenerateUniqueUsernameAsync(sanitizedUsername);
                    return (null, sanitizedUsername, suggestedUsername, "USERNAME_ALREADY_EXISTS", "usernameSuggestionMessage");
                }
                else
                {
                    sanitizedUsername = await GenerateUniqueUsernameAsync(sanitizedUsername);
                }
            }

            var user = new User
            {
                Username = sanitizedUsername,
                FullName = sanitizedFullName,
                Email = sanitizedEmail,
                PhoneNumber = dto.PhoneNumber?.Trim(),
                PasswordHash = _passwordService.HashPassword(dto.Password),
                LoginProvider = "Local",
                Status = UserStatus.Inactive,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(user);
                    var token = _jwtService.GenerateToken(user, dto.Language ?? "en");
                    var tokenExpiry = _jwtService.GetTokenExpirationTimeSpan(token);
                    await _sessionService.StoreActiveTokenAsync(token, user.Id, tokenExpiry);
                    var sessionId = Guid.NewGuid().ToString();
                    await _sessionService.CreateUserSessionAsync(user.Id, sessionId, tokenExpiry);
                    await _sessionService.SetUserLoginStatusAsync(user.Id, true, tokenExpiry);

                    var language = dto.Language ?? "en";
                    await _notificationService.SendNotificationAsync(
                        user.Id.ToString(),
                        "welcomeToEdisa",
                        "welcomeMessage",
                        "success"
                    );

                    var verifyToken = GenerateEmailVerifyToken(user.Id, user.Email);
                    var verifyLink = $"{_config["Frontend:BaseUrl"]}/auth/account-activated.html?token={verifyToken}&lang={language}";
                    await _emailMessageService.PublishRegisterNotificationAsync(new RegisterNotificationEmailEvent
                    {
                        To = user.Email,
                        Username = user.Username,
                        RegisterAt = DateTime.UtcNow,
                        VerifyLink = verifyLink,
                        Language = language
                    });
                    return (token, sanitizedUsername, null, null, null);
        }

        private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
        {
            var usernames = await _repo.GetUsernamesByPrefixAsync(baseUsername);
            var maxNumber = 0;
            var hasExactMatch = false;
            var regex = new System.Text.RegularExpressions.Regex($"^{System.Text.RegularExpressions.Regex.Escape(baseUsername)}(\\d+)$");
            
            foreach (var name in usernames)
            {
                if (name.Equals(baseUsername, StringComparison.OrdinalIgnoreCase))
                {
                    hasExactMatch = true;
                }
                else
                {
                    var match = regex.Match(name);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                    {
                        if (num > maxNumber) maxNumber = num;
                    }
                }
            }
            
            if (hasExactMatch)
            {
                var usedNumbers = new HashSet<int>();
                foreach (var name in usernames)
                {
                    var match = regex.Match(name);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                    {
                        usedNumbers.Add(num);
                    }
                }
                
                int nextNumber = 1;
                while (usedNumbers.Contains(nextNumber))
                {
                    nextNumber++;
                }
                
                return $"{baseUsername}{nextNumber}";
            }
            
            return baseUsername;
        }

        private string GenerateEmailVerifyToken(Guid userId, string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("type", "verify")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationTime = DateTime.UtcNow.AddHours(1);
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: expirationTime,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                if (await _sessionService.IsTokenBlacklistedAsync(token))
                    return false;
                var jwt = handler.ReadJwtToken(token);
                var typeClaim = jwt.Claims.FirstOrDefault(c => c.Type == "type");
                if (typeClaim == null || typeClaim.Value != "verify") return false;
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId)) return false;
                var user = await _repo.GetByIdAsync(userId);
                if (user == null) return false;
                if (user.IsVerified) {
                    var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == "exp");
                    if (expClaim != null && long.TryParse(expClaim.Value, out long expUnix)) {
                        var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                        var ttl = expDate - DateTime.UtcNow;
                        if (ttl > TimeSpan.Zero)
                            await _sessionService.BlacklistTokenAsync(token, ttl);
                    }
                    return false;
                }
                user.IsVerified = true;
                user.Status = UserStatus.Active;
                user.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(user);

                await _notificationService.SendNotificationAsync(
                    user.Id.ToString(),
                    "accountActivatedTitle",
                    "accountActivatedDesc",
                    "success"
                );
                var expClaim2 = jwt.Claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim2 != null && long.TryParse(expClaim2.Value, out long expUnix2)) {
                    var expDate2 = DateTimeOffset.FromUnixTimeSeconds(expUnix2).UtcDateTime;
                    var ttl2 = expDate2 - DateTime.UtcNow;
                    if (ttl2 > TimeSpan.Zero)
                        await _sessionService.BlacklistTokenAsync(token, ttl2);
                }
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> ResendVerificationEmailAsync(string email, string language)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Email is required");

            var user = await _repo.GetByEmailIncludeDeletedAsync(email);
            if (user == null)
                throw new UserNotFoundException(email);

            if (user.IsDeleted)
                throw new AccountDeletedException();

            if (user.IsVerified)
                throw new AuthException("EMAIL_ALREADY_VERIFIED", "Email is already verified");

            var token = GenerateEmailVerifyToken(user.Id, user.Email);
            await _sessionService.SetEmailVerifyTokenAsync(user.Id, token, TimeSpan.FromMinutes(15));

            await _emailMessageService.PublishRegisterNotificationAsync(new RegisterNotificationEmailEvent
            {
                To = user.Email,
                Username = user.FullName ?? user.Username,
                VerifyLink = $"{_config["Frontend:BaseUrl"]}/auth/account-activated.html?token={token}&lang={language ?? "en"}",
                RegisterAt = user.CreatedAt,
                Language = language ?? "en"
            });

            return true;
        }

        public async Task<string> GetEmailFromResetTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var userId = await _sessionService.GetUserIdFromResetTokenAsync(token);
            if (!userId.HasValue)
                return null;

            var user = await _repo.GetByIdAsync(userId.Value);
            return user?.Email;
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

        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var sanitizedEmail = dto.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(sanitizedEmail))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Email is required");
            var user = await _repo.GetByEmailIncludeDeletedAsync(sanitizedEmail);
            if (user == null)
                throw new InvalidCredentialsException();
            if (user.IsDeleted)
                throw new AccountDeletedException();
            if (user.Status == UserStatus.Banned)
                throw new AccountBannedException();
            if (!user.IsVerified)
                throw new AccountNotVerifiedException();
            try
            {
                var emailAddress = new System.Net.Mail.MailAddress(sanitizedEmail);
                if (emailAddress.Address != sanitizedEmail)
                    throw new AuthException("INVALID_EMAIL_FORMAT", "Invalid email format");
            }
            catch
            {
                throw new AuthException("INVALID_EMAIL_FORMAT", "Invalid email format");
            }
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Password is required");
            if (dto.Password.Length < 6)
                throw new AuthException("WEAK_PASSWORD", "Password must be at least 6 characters");
            var isLocked = await _sessionService.IsUserLockedAsync(user.Id);
            if (isLocked)
            {
                var lockExpiry = await _sessionService.GetUserLockExpiryAsync(user.Id);
                if (lockExpiry.HasValue && lockExpiry.Value > DateTime.UtcNow)
                {
                    var remainingMinutes = Math.Ceiling((lockExpiry.Value - DateTime.UtcNow).TotalMinutes);
                    throw new UserLockedException($"Account is locked. Please try again in {remainingMinutes} minutes.");
                }
                else
                {
                    await _sessionService.ResetFailedLoginAttemptsAsync(user.Id);
                }
            }
            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                var failedAttempts = await _sessionService.IncrementFailedLoginAttemptsAsync(user.Id);
                if (failedAttempts >= _maxFailedLoginAttempts)
                {
                    var lockExpiry = DateTime.UtcNow.AddMinutes(_accountLockMinutes);
                    await _sessionService.LockUserAsync(user.Id, lockExpiry);
                    throw new UserLockedException($"Account locked due to {_maxFailedLoginAttempts} failed login attempts. Please try again in {_accountLockMinutes} minutes.");
                }
                throw new InvalidCredentialsException();
            }
            await _sessionService.ResetFailedLoginAttemptsAsync(user.Id);
            if (user.LoginProvider != "Local")
            {
                user.LoginProvider = "Local";
                user.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(user);
            }
            if (user.TwoFactorEnabled && !string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                if (string.IsNullOrWhiteSpace(dto.OtpCode))
                {
                    return new LoginResultDto { Require2FA = true, UserId = user.Id, Message = "2FA code required" };
                }
                if (!VerifyOtpCode(dto.OtpCode, user.TwoFactorSecret))
                {
                    return new LoginResultDto { Require2FA = true, UserId = user.Id, Message = "Invalid 2FA code" };
                }
            }
            var token = _jwtService.GenerateToken(user, dto.Language ?? "en");
            var tokenExpiry = _jwtService.GetTokenExpirationTimeSpan(token);
            await _sessionService.StoreActiveTokenAsync(token, user.Id, tokenExpiry);
            var sessionId = Guid.NewGuid().ToString();
            await _sessionService.CreateUserSessionAsync(user.Id, sessionId, tokenExpiry);
            await _sessionService.SetUserLoginStatusAsync(user.Id, true, tokenExpiry);
            user.LastLoginAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);

            await _notificationService.SendNotificationAsync(
                user.Id.ToString(),
                "loginWelcome",
                "Welcome back! You have successfully logged in.",
                "info"
            );

            return new LoginResultDto { Token = token, Require2FA = false, UserId = user.Id, Message = "Login successful" };
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var userId = _jwtService.GetUserIdFromToken(token);
                if (!userId.HasValue)
                    return false;

                var user = await _repo.GetByIdAsync(userId.Value);

                await _sessionService.RemoveActiveTokenAsync(token);
                
                var tokenExpiry = _jwtService.GetTokenExpirationTimeSpan(token);
                await _sessionService.BlacklistTokenAsync(token, tokenExpiry);
                
                await _sessionService.SetUserLoginStatusAsync(userId.Value, false);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;
                
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                    return false;
                
                if (token.Length < 50 || token.Length > 2000)
                    return false;

                if (await _sessionService.IsTokenBlacklistedAsync(token))
                    return false;

                var userId = _jwtService.GetUserIdFromToken(token);
                if (!userId.HasValue)
                    return false;

                var user = await _repo.GetByIdAsync(userId.Value);
                if (user == null)
                    return false;

                var isActiveToken = await _sessionService.IsTokenActiveAsync(token);
                if (!isActiveToken)
                    return false;

                if (!_jwtService.ValidateToken(token))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserSessionsAsync(Guid userId)
        {
            return await _sessionService.GetUserSessionsAsync(userId);
        }

        public async Task<bool> RemoveUserSessionAsync(Guid userId, string sessionId)
        {
            return await _sessionService.RemoveUserSessionAsync(userId, sessionId);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, string clientIp)
        {
            var sanitizedEmail = dto.Email?.Trim().ToLowerInvariant();
            var user = await _repo.GetByEmailIncludeDeletedAsync(sanitizedEmail);
            if (user == null)
                throw new InvalidCredentialsException();

            if (user.IsDeleted)
                throw new AccountDeletedException();

            if (user.Status == UserStatus.Banned)
                throw new AccountBannedException();

            if (!user.IsVerified)
                throw new AccountNotVerifiedException();

            var resetToken = GenerateResetToken();
            var tokenExpiry = TimeSpan.FromMinutes(_resetPasswordTokenExpiryMinutes);

            await _sessionService.StoreResetTokenAsync(resetToken, user.Id, tokenExpiry);

            var resetLink = $"{_config["Frontend:BaseUrl"]}/auth/reset-password.html?token={resetToken}";
            await _emailMessageService.PublishResetPasswordNotificationAsync(new ResetPasswordEmailEvent
            {
                To = user.Email,
                Username = user.FullName ?? user.Username,
                ResetToken = resetToken,
                ResetLink = resetLink,
                RequestedAt = DateTime.UtcNow,
                Language = dto.Language ?? "en",
                UserId = user.Id.ToString(),
                IpAddress = clientIp ?? "Unknown"
            });

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Reset token is required");
            
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new AuthException("REQUIRED_FIELD_MISSING", "New password is required");
            
            if (dto.NewPassword.Length < 6)
                throw new AuthException("WEAK_PASSWORD", "New password must be at least 6 characters");
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$"))
                throw new AuthException("WEAK_PASSWORD", "New password must contain at least one uppercase letter, one lowercase letter, and one number");
            
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Confirm password is required");
            
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new AuthException("PASSWORD_MISMATCH", "Passwords do not match");
            
            var userId = await _sessionService.GetUserIdFromResetTokenAsync(dto.Token);
            if (!userId.HasValue)
                throw new InvalidResetTokenException();

            var user = await _repo.GetByIdAsync(userId.Value);
            if (user == null)
                throw new UserNotFoundException(userId.Value);

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);

            await _sessionService.RemoveResetTokenAsync(dto.Token);

            await _sessionService.RemoveAllUserSessionsAsync(user.Id);
            await _sessionService.RemoveAllActiveTokensForUserAsync(user.Id);
            await _sessionService.SetUserLoginStatusAsync(user.Id, false);

            await _emailMessageService.PublishChangePasswordNotificationAsync(new ChangePasswordEmailEvent
            {
                To = user.Email,
                Username = user.FullName ?? user.Username,
                ChangeAt = DateTime.UtcNow,
                Language = dto.Language ?? "en"
            });

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Current password is required");
            
            if (dto.CurrentPassword.Length < 6)
                throw new AuthException("WEAK_PASSWORD", "Current password must be at least 6 characters");

            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                throw new PasswordMismatchException();

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new AuthException("REQUIRED_FIELD_MISSING", "New password is required");
            
            if (dto.NewPassword.Length < 6)
                throw new AuthException("WEAK_PASSWORD", "New password must be at least 6 characters");
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$"))
                throw new AuthException("WEAK_PASSWORD", "New password must contain at least one uppercase letter, one lowercase letter, and one number");
            
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                throw new AuthException("REQUIRED_FIELD_MISSING", "Confirm password is required");
            
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new AuthException("PASSWORD_MISMATCH", "Passwords do not match");

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);

            await _sessionService.RemoveAllUserSessionsAsync(user.Id);
            await _sessionService.RemoveAllActiveTokensForUserAsync(user.Id);
            await _sessionService.SetUserLoginStatusAsync(user.Id, false);

            await _emailMessageService.PublishChangePasswordNotificationAsync(new ChangePasswordEmailEvent
            {
                To = user.Email,
                Username = user.FullName ?? user.Username,
                ChangeAt = DateTime.UtcNow,
                Language = dto.Language ?? "en"
            });

            await _notificationService.SendNotificationAsync(
                userId.ToString(),
                "passwordChangedSuccessfully",
                "Your password has been changed successfully.",
                "success"
            );

            return true;
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

        public async Task<(string qrCodeImage, string secret)> EnableTwoFactorAsync(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            if (!string.IsNullOrEmpty(user.TwoFactorSecret) && user.TwoFactorEnabled)
                throw new AuthException("2FA_ALREADY_ENABLED", "2FA already enabled");
            var secret = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secret);
            user.TwoFactorSecret = base32Secret;
            user.TwoFactorEnabled = false;
            await _repo.UpdateAsync(user);
            var issuer = "EDISA";
            var label = user.Email;
            var otpUrl = new OtpUri(OtpType.Totp, base32Secret, label, issuer).ToString();
            var qrGen = new QRCodeGenerator();
            var qrData = qrGen.CreateQrCode(otpUrl, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(qrData);
            var qrBytes = pngQr.GetGraphic(20);
            var qrBase64 = Convert.ToBase64String(qrBytes);
            return ($"data:image/png;base64,{qrBase64}", base32Secret);
        }

        public async Task<bool> VerifyTwoFactorAsync(Guid userId, string code)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            if (!VerifyOtpCode(code, user.TwoFactorSecret))
                return false;

            if (!user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(user);
            }

            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(Guid userId, string code, string? language = null)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            if (!VerifyOtpCode(code, user.TwoFactorSecret))
                return false;

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);

            return true;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _repo.GetByIdAsync(userId);
        }


    }
}
