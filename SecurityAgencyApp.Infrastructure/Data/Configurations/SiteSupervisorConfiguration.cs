using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SiteSupervisorConfiguration : IEntityTypeConfiguration<SiteSupervisor>
{
    public void Configure(EntityTypeBuilder<SiteSupervisor> builder)
    {
        builder.ToTable("SiteSupervisors");

        builder.HasKey(ss => ss.Id);

        builder.HasIndex(ss => new { ss.SiteId, ss.UserId }).IsUnique();

        builder.HasOne(ss => ss.Site)
            .WithMany(s => s.SiteSupervisors)
            .HasForeignKey(ss => ss.SiteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ss => ss.User)
            .WithMany(u => u.SupervisedSites)
            .HasForeignKey(ss => ss.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(ss => ss.SiteId);
        builder.HasIndex(ss => ss.UserId);
    }
}
