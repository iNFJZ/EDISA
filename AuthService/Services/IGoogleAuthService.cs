using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken);
        Task<(string? token, bool require2FA, Guid? userId, string? message)> LoginWithGoogleAsync(GoogleLoginDto dto);
        Task<(bool success, string? token, string? message)> VerifyGoogleOtpAsync(Guid userId, string otpCode, string? language);
    }
} 