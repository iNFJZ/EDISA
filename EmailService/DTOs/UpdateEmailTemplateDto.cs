using System.ComponentModel.DataAnnotations;

namespace EmailService.DTOs
{
    public class UpdateEmailTemplateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(10)]
        public string? Language { get; set; }
        
        [StringLength(200)]
        public string? Subject { get; set; }
        
        public string? Body { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool? IsActive { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
}
