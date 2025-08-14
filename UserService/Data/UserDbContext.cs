using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Username).HasColumnName("Username");
            entity.Property(e => e.Email).HasColumnName("Email");
            entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash");
            entity.Property(e => e.FullName).HasColumnName("FullName");
            entity.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber");
            entity.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth");
            entity.Property(e => e.Address).HasColumnName("Address");
            entity.Property(e => e.Bio).HasColumnName("Bio");
            entity.Property(e => e.Status).HasColumnName("Status");
            entity.Property(e => e.IsVerified).HasColumnName("IsVerified");
            entity.Property(e => e.GoogleId).HasColumnName("GoogleId");
            entity.Property(e => e.ProfilePicture).HasColumnName("ProfilePicture");
            entity.Property(e => e.LoginProvider).HasColumnName("LoginProvider");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            entity.Property(e => e.LastLoginAt).HasColumnName("LastLoginAt");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
            entity.Property(e => e.TwoFactorSecret).HasColumnName("TwoFactorSecret");
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("TwoFactorEnabled");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.GoogleId).IsUnique().HasFilter("\"GoogleId\" IS NOT NULL");
        });
    }
} 