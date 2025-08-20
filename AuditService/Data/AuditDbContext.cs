using AuditService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
            
            entity.Property(e => e.Action)
                .HasColumnName("action")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ResourceType)
                .HasColumnName("resource_type")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ServiceName)
                .HasColumnName("service_name")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(100);
            
            entity.Property(e => e.UserEmail)
                .HasColumnName("user_email")
                .HasMaxLength(255);
            
            entity.Property(e => e.ResourceId)
                .HasColumnName("resource_id")
                .HasMaxLength(100);
            
            entity.Property(e => e.OldValues)
                .HasColumnName("old_values");
            
            entity.Property(e => e.NewValues)
                .HasColumnName("new_values");
            
            entity.Property(e => e.IpAddress)
                .HasColumnName("ip_address");
            
            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent");
            
            entity.Property(e => e.Success)
                .HasColumnName("success")
                .HasDefaultValue(true);
            
            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message");
            
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata");
            
            entity.Property(e => e.RequestId)
                .HasColumnName("request_id")
                .HasMaxLength(100);
            
            entity.Property(e => e.SessionId)
                .HasColumnName("session_id")
                .HasMaxLength(100);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_audit_logs_user_id");
            
            entity.HasIndex(e => e.Action)
                .HasDatabaseName("idx_audit_logs_action");
            
            entity.HasIndex(e => e.ResourceType)
                .HasDatabaseName("idx_audit_logs_resource_type");
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_audit_logs_created_at");
            
            entity.HasIndex(e => e.ServiceName)
                .HasDatabaseName("idx_audit_logs_service_name");
            
            entity.HasIndex(e => e.RequestId)
                .HasDatabaseName("idx_audit_logs_request_id");
        });
    }
}
