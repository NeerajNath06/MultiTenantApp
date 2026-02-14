using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
{
    public void Configure(EntityTypeBuilder<MenuPermission> builder)
    {
        builder.ToTable("MenuPermissions");

        builder.HasKey(mp => mp.Id);

        // Relationships - Use NoAction to avoid cascade cycles
        builder.HasOne(mp => mp.Menu)
            .WithMany()
            .HasForeignKey(mp => mp.MenuId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(mp => mp.Permission)
            .WithMany(p => p.MenuPermissions)
            .HasForeignKey(mp => mp.PermissionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(mp => new { mp.MenuId, mp.PermissionId })
            .IsUnique();
    }
}
