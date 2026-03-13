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

        builder.Property(t => t.LegalName)
            .HasMaxLength(200);

        builder.Property(t => t.TradeName)
            .HasMaxLength(200);

        builder.Property(t => t.CompanyCode)
            .HasMaxLength(50);

        builder.Property(t => t.CinNumber)
            .HasMaxLength(50);

        builder.Property(t => t.GstNumber)
            .HasMaxLength(50);

        builder.Property(t => t.PanNumber)
            .HasMaxLength(50);

        builder.Property(t => t.PfNumber)
            .HasMaxLength(50);

        builder.Property(t => t.EsicNumber)
            .HasMaxLength(50);

        builder.Property(t => t.LabourLicenseNumber)
            .HasMaxLength(50);

        builder.Property(t => t.OwnerName)
            .HasMaxLength(100);

        builder.Property(t => t.ComplianceOfficerName)
            .HasMaxLength(100);

        builder.Property(t => t.BillingContactName)
            .HasMaxLength(100);

        builder.Property(t => t.BillingContactPhone)
            .HasMaxLength(20);

        builder.Property(t => t.BillingEmail)
            .HasMaxLength(100);

        builder.Property(t => t.EscalationContactName)
            .HasMaxLength(100);

        builder.Property(t => t.EscalationContactPhone)
            .HasMaxLength(20);

        builder.Property(t => t.SupportEmail)
            .HasMaxLength(100);

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

        builder.Property(t => t.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Asia/Kolkata");

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(t => t.InvoicePrefix)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("INV");

        builder.Property(t => t.PayrollPrefix)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("PAY");

        builder.Property(t => t.SubscriptionPlan)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Standard");

        builder.Property(t => t.StorageLimitGb)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.OnboardingStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(t => t.ActivationStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(t => t.LogoPath)
            .HasMaxLength(500);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(t => t.IsActive);
    }
}
