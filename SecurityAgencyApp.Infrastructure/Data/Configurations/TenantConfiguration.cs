using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.RegistrationNumber)
            .IsUnique();

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Email);

        builder.Property(t => t.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Address)
            .HasMaxLength(500);

        builder.Property(t => t.City)
            .HasMaxLength(100);

        builder.Property(t => t.State)
            .HasMaxLength(100);

        builder.Property(t => t.Country)
            .HasMaxLength(100);

        builder.Property(t => t.PinCode)
            .HasMaxLength(10);

        builder.Property(t => t.Website)
            .HasMaxLength(200);

        builder.Property(t => t.TaxId)
            .HasMaxLength(50);

        builder.Property(t => t.LogoPath)
            .HasMaxLength(500);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(t => t.IsActive);
    }
}
