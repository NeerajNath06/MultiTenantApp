using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(p => p.GrossAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NetAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Notes).HasMaxLength(1000);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Site)
            .WithMany()
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Wage)
            .WithMany()
            .HasForeignKey(p => p.WageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => new { p.TenantId, p.SiteId, p.PayrollYear, p.PayrollMonth });
    }
}
