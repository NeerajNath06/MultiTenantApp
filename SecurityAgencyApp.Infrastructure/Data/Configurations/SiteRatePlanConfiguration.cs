using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SiteRatePlanConfiguration : IEntityTypeConfiguration<SiteRatePlan>
{
    public void Configure(EntityTypeBuilder<SiteRatePlan> builder)
    {
        builder.ToTable("SiteRatePlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RateAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Site)
            .WithMany()
            .HasForeignKey(x => x.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.SiteId, x.EffectiveFrom });
        builder.HasIndex(x => new { x.TenantId, x.SiteId });
    }
}

