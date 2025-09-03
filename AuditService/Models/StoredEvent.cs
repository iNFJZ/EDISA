using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditService.Models;

[Table("audit_events")]
public class StoredEvent
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("aggregate_type")]
    public string AggregateType { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("aggregate_id")]
    public string? AggregateId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [Column("event_data")]
    public string EventData { get; set; } = string.Empty;

    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("version")]
    public int Version { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    [Column("correlation_id")]
    public string? CorrelationId { get; set; }

    [MaxLength(100)]
    [Column("causation_id")]
    public string? CausationId { get; set; }

    [MaxLength(100)]
    [Column("user_id")]
    public string? UserId { get; set; }

    [MaxLength(255)]
    [Column("user_email")]
    public string? UserEmail { get; set; }
}


