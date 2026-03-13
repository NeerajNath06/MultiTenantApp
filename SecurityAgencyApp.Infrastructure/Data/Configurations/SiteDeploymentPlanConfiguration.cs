using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SiteDeploymentPlanConfiguration : IEntityTypeConfiguration<SiteDeploymentPlan>
{
    public void Configure(EntityTypeBuilder<SiteDeploymentPlan> builder)
    {
        builder.ToTable("SiteDeploymentPlans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReservePoolMapping)
            .HasMaxLength(500);

        builder.Property(p => p.AccessZones)
            .HasMaxLength(1000);

        builder.Property(p => p.EmergencyContactSet)
            .HasMaxLength(500);

        builder.Property(p => p.InstructionSummary)
            .HasMaxLength(1000);

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(p => p.Site)
            .WithMany(s => s.DeploymentPlans)
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => new { p.TenantId, p.SiteId, p.EffectiveFrom });
    }
}
