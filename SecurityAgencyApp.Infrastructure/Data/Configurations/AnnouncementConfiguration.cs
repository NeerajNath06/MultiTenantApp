using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.Category)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("general");

        builder.Property(a => a.PostedByName)
            .HasMaxLength(200);

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PostedByUser)
            .WithMany()
            .HasForeignKey(a => a.PostedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.PostedAt);
        builder.HasIndex(a => a.IsPinned);
    }
}
