using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class GuardAttendanceConfiguration : IEntityTypeConfiguration<GuardAttendance>
{
    public void Configure(EntityTypeBuilder<GuardAttendance> builder)
    {
        builder.ToTable("GuardAttendance");

        builder.HasKey(ga => ga.Id);

        builder.Property(ga => ga.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ga => ga.CheckInLocation)
            .HasMaxLength(500);

        builder.Property(ga => ga.CheckOutLocation)
            .HasMaxLength(500);

        builder.Property(ga => ga.Remarks)
            .HasMaxLength(1000);

        builder.HasIndex(ga => new { ga.GuardId, ga.AttendanceDate, ga.AssignmentId })
            .IsUnique();

        // Relationships
        builder.HasOne(ga => ga.Tenant)
            .WithMany()
            .HasForeignKey(ga => ga.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.Guard)
            .WithMany(g => g.Attendances)
            .HasForeignKey(ga => ga.GuardId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.Assignment)
            .WithMany(a => a.Attendances)
            .HasForeignKey(ga => ga.AssignmentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.MarkedByUser)
            .WithMany()
            .HasForeignKey(ga => ga.MarkedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
