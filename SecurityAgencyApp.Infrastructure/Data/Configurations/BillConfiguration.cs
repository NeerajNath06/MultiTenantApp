using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("Bills");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BillNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(b => b.PaymentTerms)
            .HasMaxLength(200);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        builder.Property(b => b.SubTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Site)
            .WithMany()
            .HasForeignKey(b => b.SiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Client)
            .WithMany(c => c.Bills)
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Bill)
            .HasForeignKey(i => i.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.TenantId);
        builder.HasIndex(b => b.ClientId);
        builder.HasIndex(b => b.BillNumber);
        builder.HasIndex(b => b.BillDate);
    }
}

public class BillItemConfiguration : IEntityTypeConfiguration<BillItem>
{
    public void Configure(EntityTypeBuilder<BillItem> builder)
    {
        builder.ToTable("BillItems");

        builder.HasKey(bi => bi.Id);

        builder.Property(bi => bi.ItemName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(bi => bi.Description)
            .HasMaxLength(500);

        builder.Property(bi => bi.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(bi => bi.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(bi => bi.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(bi => bi.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(bi => bi.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(bi => bi.Bill)
            .WithMany(b => b.Items)
            .HasForeignKey(bi => bi.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bi => bi.BillId);
    }
}
