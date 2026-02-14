using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class PatrolScanConfiguration : IEntityTypeConfiguration<PatrolScan>
{
    public void Configure(EntityTypeBuilder<PatrolScan> builder)
    {
        builder.ToTable("PatrolScans");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.LocationName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.CheckpointCode).HasMaxLength(100);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Success");
        builder.HasOne(s => s.Tenant).WithMany().HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.Guard).WithMany().HasForeignKey(s => s.GuardId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Site).WithMany().HasForeignKey(s => s.SiteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.GuardId);
        builder.HasIndex(s => s.ScannedAt);
    }
}
