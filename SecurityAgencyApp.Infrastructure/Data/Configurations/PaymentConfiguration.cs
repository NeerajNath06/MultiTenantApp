using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaymentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Cash");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Bill)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Client)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Contract)
            .WithMany()
            .HasForeignKey(p => p.ContractId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.BillId);
        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.PaymentDate);
    }
}
