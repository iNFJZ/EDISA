namespace AuthService.DTOs
{
    public class Disable2FADto
    {
        public string Code { get; set; } = string.Empty;
        public string? Language { get; set; }
    }
} 