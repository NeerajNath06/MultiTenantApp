using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class UserSubMenuConfiguration : IEntityTypeConfiguration<UserSubMenu>
{
    public void Configure(EntityTypeBuilder<UserSubMenu> builder)
    {
        builder.ToTable("UserSubMenus");

        builder.HasKey(usm => usm.Id);

        // Relationships - Use NoAction to avoid cascade cycles
        builder.HasOne(usm => usm.User)
            .WithMany()
            .HasForeignKey(usm => usm.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(usm => usm.SubMenu)
            .WithMany(sm => sm.UserSubMenus)
            .HasForeignKey(usm => usm.SubMenuId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(usm => new { usm.UserId, usm.SubMenuId })
            .IsUnique();
    }
}
