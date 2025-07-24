using System;

namespace AuthService.DTOs
{
    public class LoginResultDto
    {
        public string? Token { get; set; }
        public bool Require2FA { get; set; }
        public Guid? UserId { get; set; }
        public string? Message { get; set; }
    }
} 