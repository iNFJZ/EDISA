using System.ComponentModel.DataAnnotations;

namespace EmailService.DTOs
{
    public class CreateEmailTemplateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Language { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? CreatedBy { get; set; }
    }
}
