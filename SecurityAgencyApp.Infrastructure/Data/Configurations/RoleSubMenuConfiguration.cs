using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class RoleSubMenuConfiguration : IEntityTypeConfiguration<RoleSubMenu>
{
    public void Configure(EntityTypeBuilder<RoleSubMenu> builder)
    {
        builder.ToTable("RoleSubMenus");

        builder.HasKey(rsm => rsm.Id);

        // Relationships - Use NoAction to avoid cascade cycles
        builder.HasOne(rsm => rsm.Role)
            .WithMany()
            .HasForeignKey(rsm => rsm.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(rsm => rsm.SubMenu)
            .WithMany(sm => sm.RoleSubMenus)
            .HasForeignKey(rsm => rsm.SubMenuId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(rsm => new { rsm.RoleId, rsm.SubMenuId })
            .IsUnique();
    }
}
