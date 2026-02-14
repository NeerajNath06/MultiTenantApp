using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class RoleMenuConfiguration : IEntityTypeConfiguration<RoleMenu>
{
    public void Configure(EntityTypeBuilder<RoleMenu> builder)
    {
        builder.ToTable("RoleMenus");

        builder.HasKey(rm => rm.Id);

        // Relationships - Use NoAction to avoid cascade cycles
        builder.HasOne(rm => rm.Role)
            .WithMany()
            .HasForeignKey(rm => rm.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(rm => rm.Menu)
            .WithMany()
            .HasForeignKey(rm => rm.MenuId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(rm => new { rm.RoleId, rm.MenuId })
            .IsUnique();
    }
}
