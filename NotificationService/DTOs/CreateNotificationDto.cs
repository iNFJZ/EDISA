using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Icon { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Data { get; set; }
    }
} 