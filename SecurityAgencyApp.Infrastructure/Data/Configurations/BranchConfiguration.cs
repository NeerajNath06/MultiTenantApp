using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BranchCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.BranchName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.PinCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(b => b.ContactPerson)
            .HasMaxLength(100);

        builder.Property(b => b.ContactPhone)
            .HasMaxLength(20);

        builder.Property(b => b.ContactEmail)
            .HasMaxLength(100);

        builder.Property(b => b.GstNumber)
            .HasMaxLength(50);

        builder.Property(b => b.LabourLicenseNumber)
            .HasMaxLength(50);

        builder.Property(b => b.NumberPrefix)
            .HasMaxLength(20);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(b => new { b.TenantId, b.BranchCode }).IsUnique();
        builder.HasIndex(b => new { b.TenantId, b.BranchName });
    }
}
