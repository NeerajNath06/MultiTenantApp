using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SecurityGuardConfiguration : IEntityTypeConfiguration<SecurityGuard>
{
    public void Configure(EntityTypeBuilder<SecurityGuard> builder)
    {
        builder.ToTable("SecurityGuards");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.GuardCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(g => g.GuardCode);

        builder.Property(g => g.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Email)
            .HasMaxLength(100);

        builder.Property(g => g.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(g => g.PhoneNumber);

        builder.Property(g => g.AlternatePhone)
            .HasMaxLength(20);

        builder.Property(g => g.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.PinCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(g => g.AadharNumber)
            .HasMaxLength(20);

        builder.Property(g => g.PANNumber)
            .HasMaxLength(20);

        builder.Property(g => g.EmergencyContactName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.EmergencyContactPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(g => g.PhotoPath)
            .HasMaxLength(500);

        builder.Property(g => g.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(g => g.IsActive);

        builder.Property(g => g.UserId)
            .IsRequired(false);

        builder.HasIndex(g => g.UserId);

        builder.Property(g => g.SupervisorId)
            .IsRequired(false);

        builder.HasIndex(g => g.SupervisorId);

        // Relationships
        builder.HasOne(g => g.Tenant)
            .WithMany()
            .HasForeignKey(g => g.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(g => g.Supervisor)
            .WithMany(u => u.SupervisedGuards)
            .HasForeignKey(g => g.SupervisorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
