using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class GuardAssignmentConfiguration : IEntityTypeConfiguration<GuardAssignment>
{
    public void Configure(EntityTypeBuilder<GuardAssignment> builder)
    {
        builder.ToTable("GuardAssignments");

        builder.HasKey(ga => ga.Id);

        builder.Property(ga => ga.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ga => ga.Remarks)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(ga => ga.Tenant)
            .WithMany()
            .HasForeignKey(ga => ga.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.Guard)
            .WithMany(g => g.Assignments)
            .HasForeignKey(ga => ga.GuardId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.Site)
            .WithMany(s => s.GuardAssignments)
            .HasForeignKey(ga => ga.SiteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.Supervisor)
            .WithMany(u => u.SupervisedAssignments)
            .HasForeignKey(ga => ga.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ga => ga.Shift)
            .WithMany(s => s.GuardAssignments)
            .HasForeignKey(ga => ga.ShiftId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ga => ga.CreatedByUser)
            .WithMany(u => u.CreatedAssignments)
            .HasForeignKey(ga => ga.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
