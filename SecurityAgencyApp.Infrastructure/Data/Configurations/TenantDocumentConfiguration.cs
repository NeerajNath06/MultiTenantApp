using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class TenantDocumentConfiguration : IEntityTypeConfiguration<TenantDocument>
{
    public void Configure(EntityTypeBuilder<TenantDocument> builder)
    {
        builder.ToTable("TenantDocuments");

        builder.HasKey(td => td.Id);

        builder.Property(td => td.DocumentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(td => td.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(td => td.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(td => td.OriginalFileName)
            .HasMaxLength(255);

        builder.HasOne(td => td.Tenant)
            .WithMany(t => t.Documents)
            .HasForeignKey(td => td.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
