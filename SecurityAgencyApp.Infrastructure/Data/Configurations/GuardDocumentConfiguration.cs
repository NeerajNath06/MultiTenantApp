using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class GuardDocumentConfiguration : IEntityTypeConfiguration<GuardDocument>
{
    public void Configure(EntityTypeBuilder<GuardDocument> builder)
    {
        builder.ToTable("GuardDocuments");

        builder.HasKey(gd => gd.Id);

        builder.Property(gd => gd.DocumentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(gd => gd.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(gd => gd.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(gd => gd.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(gd => gd.DocumentType);

        // Relationships
        builder.HasOne(gd => gd.Guard)
            .WithMany(g => g.Documents)
            .HasForeignKey(gd => gd.GuardId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(gd => gd.VerifiedByUser)
            .WithMany(u => u.VerifiedDocuments)
            .HasForeignKey(gd => gd.VerifiedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
