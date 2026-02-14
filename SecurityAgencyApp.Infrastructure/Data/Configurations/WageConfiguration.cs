using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class WageConfiguration : IEntityTypeConfiguration<Wage>
{
    public void Configure(EntityTypeBuilder<Wage> builder)
    {
        builder.ToTable("Wages");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WageSheetNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(w => w.Notes)
            .HasMaxLength(1000);

        builder.Property(w => w.TotalWages)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.TotalDeductions)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.TotalAllowances)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.NetAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(w => w.Tenant)
            .WithMany()
            .HasForeignKey(w => w.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.WageDetails)
            .WithOne(wd => wd.Wage)
            .HasForeignKey(wd => wd.WageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.TenantId);
        builder.HasIndex(w => w.WageSheetNumber);
        builder.HasIndex(w => w.WagePeriodStart);
        builder.HasIndex(w => w.WagePeriodEnd);
    }
}

public class WageDetailConfiguration : IEntityTypeConfiguration<WageDetail>
{
    public void Configure(EntityTypeBuilder<WageDetail> builder)
    {
        builder.ToTable("WageDetails");

        builder.HasKey(wd => wd.Id);

        builder.Property(wd => wd.Remarks)
            .HasMaxLength(500);

        builder.Property(wd => wd.BasicRate)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.BasicAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.OvertimeRate)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.OvertimeAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.Allowances)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.Deductions)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.GrossAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(wd => wd.NetAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(wd => wd.Wage)
            .WithMany(w => w.WageDetails)
            .HasForeignKey(wd => wd.WageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wd => wd.Guard)
            .WithMany()
            .HasForeignKey(wd => wd.GuardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wd => wd.Site)
            .WithMany()
            .HasForeignKey(wd => wd.SiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(wd => wd.Shift)
            .WithMany()
            .HasForeignKey(wd => wd.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(wd => wd.WageId);
        builder.HasIndex(wd => wd.GuardId);
    }
}
