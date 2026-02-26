using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class VehicleLogConfiguration : IEntityTypeConfiguration<VehicleLog>
{
    public void Configure(EntityTypeBuilder<VehicleLog> builder)
    {
        builder.ToTable("VehicleLogs");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(30);
        builder.Property(v => v.VehicleType).IsRequired().HasMaxLength(20).HasDefaultValue("Car");
        builder.Property(v => v.DriverName).IsRequired().HasMaxLength(200);
        builder.Property(v => v.DriverPhone).HasMaxLength(20);
        builder.Property(v => v.Purpose).IsRequired().HasMaxLength(100);
        builder.Property(v => v.ParkingSlot).HasMaxLength(20);

        builder.HasOne(v => v.Tenant)
            .WithMany()
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.Site)
            .WithMany()
            .HasForeignKey(v => v.SiteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Guard)
            .WithMany()
            .HasForeignKey(v => v.GuardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.TenantId);
        builder.HasIndex(v => v.SiteId);
        builder.HasIndex(v => v.GuardId);
        builder.HasIndex(v => v.EntryTime);
    }
}
