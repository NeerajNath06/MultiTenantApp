using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClientCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactPerson)
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.BillingAddress)
            .HasMaxLength(500);

        builder.Property(c => c.BillingCity)
            .HasMaxLength(100);

        builder.Property(c => c.BillingState)
            .HasMaxLength(100);

        builder.Property(c => c.BillingPinCode)
            .HasMaxLength(10);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(c => c.GSTNumber)
            .HasMaxLength(50);

        builder.Property(c => c.PANNumber)
            .HasMaxLength(50);

        builder.Property(c => c.AccountManagerName)
            .HasMaxLength(100);

        builder.Property(c => c.BillingContactName)
            .HasMaxLength(100);

        builder.Property(c => c.BillingContactEmail)
            .HasMaxLength(100);

        builder.Property(c => c.EscalationContactName)
            .HasMaxLength(100);

        builder.Property(c => c.EscalationContactEmail)
            .HasMaxLength(100);

        builder.Property(c => c.BillingCycle)
            .HasMaxLength(50);

        builder.Property(c => c.GstState)
            .HasMaxLength(100);

        builder.Property(c => c.PaymentModePreference)
            .HasMaxLength(50);

        builder.Property(c => c.TaxTreatment)
            .HasMaxLength(100);

        builder.Property(c => c.InvoicePrefix)
            .HasMaxLength(20);

        builder.Property(c => c.SlaTerms)
            .HasMaxLength(500);

        builder.Property(c => c.PenaltyTerms)
            .HasMaxLength(500);

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.ClientCode);
    }
}
