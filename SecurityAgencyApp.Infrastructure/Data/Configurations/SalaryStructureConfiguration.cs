using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.ToTable("SalaryStructures");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StructureName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.BasicAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.HraAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.WashingAllowance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ConveyanceAllowance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.BonusAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.OtherAllowanceAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.EpfPercent).HasColumnType("decimal(18,2)");
        builder.Property(s => s.EsicPercent).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ProfessionalTaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TdsAmount).HasColumnType("decimal(18,2)");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Site)
            .WithMany()
            .HasForeignKey(s => s.SiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.TenantId, s.StructureName, s.EffectiveFrom });
    }
}
