using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("FormSubmissions");

        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(fs => fs.Remarks)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(fs => fs.Tenant)
            .WithMany()
            .HasForeignKey(fs => fs.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(fs => fs.FormTemplate)
            .WithMany(ft => ft.Submissions)
            .HasForeignKey(fs => fs.FormTemplateId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(fs => fs.SubmittedByUser)
            .WithMany(u => u.FormSubmissions)
            .HasForeignKey(fs => fs.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fs => fs.ApprovedByUser)
            .WithMany()
            .HasForeignKey(fs => fs.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
