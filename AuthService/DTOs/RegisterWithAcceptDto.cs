using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class RegisterWithAcceptDto : RegisterDto
    {
        public bool AcceptSuggestedUsername { get; set; } = false;
    }
} 