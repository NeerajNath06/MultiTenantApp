using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExpenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Cash");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Site)
            .WithMany(s => s.Expenses)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Guard)
            .WithMany(g => g.Expenses)
            .HasForeignKey(e => e.GuardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.ExpenseDate);
        builder.HasIndex(e => e.Category);
    }
}
