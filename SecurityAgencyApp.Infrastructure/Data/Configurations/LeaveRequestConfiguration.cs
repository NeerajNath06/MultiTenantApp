using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");

        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.LeaveType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Casual");

        builder.Property(lr => lr.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(lr => lr.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(lr => lr.Tenant)
            .WithMany()
            .HasForeignKey(lr => lr.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lr => lr.Guard)
            .WithMany(g => g.LeaveRequests)
            .HasForeignKey(lr => lr.GuardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lr => lr.ApprovedByUser)
            .WithMany()
            .HasForeignKey(lr => lr.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(lr => lr.TenantId);
        builder.HasIndex(lr => lr.GuardId);
        builder.HasIndex(lr => lr.StartDate);
        builder.HasIndex(lr => lr.Status);
    }
}
