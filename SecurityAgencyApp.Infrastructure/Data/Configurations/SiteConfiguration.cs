using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SiteCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.SiteCode);

        builder.Property(s => s.SiteName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.PinCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.ContactPerson)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ContactPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.ContactEmail)
            .HasMaxLength(100);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.Latitude);
        builder.Property(s => s.Longitude);
        builder.Property(s => s.GeofenceRadiusMeters);

        // Relationships
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.Client)
            .WithMany(c => c.Sites)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.ClientId);
    }
}
