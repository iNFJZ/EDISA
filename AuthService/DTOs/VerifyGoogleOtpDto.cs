using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class VerifyGoogleOtpDto
    {
        public Guid? UserId { get; set; }
        public string? OtpCode { get; set; }
        public string? Language { get; set; }
    }
} 