using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SitePostConfiguration : IEntityTypeConfiguration<SitePost>
{
    public void Configure(EntityTypeBuilder<SitePost> builder)
    {
        builder.ToTable("SitePosts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PostCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PostName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ShiftName)
            .HasMaxLength(100);

        builder.Property(p => p.GenderRequirement)
            .HasMaxLength(50);

        builder.Property(p => p.SkillRequirement)
            .HasMaxLength(200);

        builder.Property(p => p.WeeklyOffPattern)
            .HasMaxLength(100);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(p => p.Site)
            .WithMany(s => s.SitePosts)
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => new { p.TenantId, p.SiteId, p.PostCode }).IsUnique();
    }
}
