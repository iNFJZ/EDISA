using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<(string token, string username, string suggestedUsername, string errorCode, string message)> RegisterAsync(RegisterWithAcceptDto dto, bool acceptSuggestedUsername = false);
        Task<LoginResultDto> LoginAsync(LoginDto dto);
        Task<bool> LogoutAsync(string token);
        Task<bool> ValidateTokenAsync(string token);
        Task<IEnumerable<string>> GetUserSessionsAsync(Guid userId);
        Task<bool> RemoveUserSessionAsync(Guid userId, string sessionId);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, string clientIp);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResendVerificationEmailAsync(string email, string language);
        Task<string> GetEmailFromResetTokenAsync(string token);
        Task<(string qrCodeImage, string secret)> EnableTwoFactorAsync(Guid userId);
        Task<bool> VerifyTwoFactorAsync(Guid userId, string code);
        Task<bool> DisableTwoFactorAsync(Guid userId, string code, string? language = null);
        Task<User?> GetUserByIdAsync(Guid userId);
    }
}
