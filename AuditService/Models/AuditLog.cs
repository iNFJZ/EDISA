using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditService.Models;

[Table("audit_logs")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("user_id")]
    [MaxLength(100)]
    public string? UserId { get; set; }
    
    [Column("user_email")]
    [MaxLength(255)]
    public string? UserEmail { get; set; }
    
    [Column("action")]
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Column("resource_type")]
    [Required]
    [MaxLength(100)]
    public string ResourceType { get; set; } = string.Empty;
    
    [Column("resource_id")]
    [MaxLength(100)]
    public string? ResourceId { get; set; }
    
    [Column("old_values")]
    public string? OldValues { get; set; }
    
    [Column("new_values")]
    public string? NewValues { get; set; }
    
    [Column("ip_address")]
    public string? IpAddress { get; set; }
    
    [Column("user_agent")]
    public string? UserAgent { get; set; }
    
    [Column("success")]
    public bool Success { get; set; } = true;
    
    [Column("error_message")]
    public string? ErrorMessage { get; set; }
    
    [Column("metadata")]
    public string? Metadata { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("service_name")]
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    
    [Column("request_id")]
    [MaxLength(100)]
    public string? RequestId { get; set; }
    
    [Column("session_id")]
    [MaxLength(100)]
    public string? SessionId { get; set; }
}
