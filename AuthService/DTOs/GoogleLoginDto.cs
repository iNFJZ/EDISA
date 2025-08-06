using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class GoogleLoginDto
    {
        public string Code { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string? Language { get; set; }
        public string? OtpCode { get; set; }
    }
} 