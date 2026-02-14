using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EquipmentCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EquipmentName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Available");

        builder.Property(e => e.PurchaseCost)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AssignedToGuard)
            .WithMany(g => g.AssignedEquipment)
            .HasForeignKey(e => e.AssignedToGuardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AssignedToSite)
            .WithMany(s => s.AssignedEquipment)
            .HasForeignKey(e => e.AssignedToSiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.EquipmentCode);
        builder.HasIndex(e => e.Status);
    }
}
