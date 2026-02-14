using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class TrainingRecordConfiguration : IEntityTypeConfiguration<TrainingRecord>
{
    public void Configure(EntityTypeBuilder<TrainingRecord> builder)
    {
        builder.ToTable("TrainingRecords");

        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.TrainingType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tr => tr.TrainingName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tr => tr.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Completed");

        builder.Property(tr => tr.Score)
            .HasColumnType("decimal(5,2)");

        builder.HasOne(tr => tr.Tenant)
            .WithMany()
            .HasForeignKey(tr => tr.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tr => tr.Guard)
            .WithMany(g => g.TrainingRecords)
            .HasForeignKey(tr => tr.GuardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tr => tr.TenantId);
        builder.HasIndex(tr => tr.GuardId);
        builder.HasIndex(tr => tr.TrainingDate);
    }
}
