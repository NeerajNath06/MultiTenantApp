using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SubMenuPermissionConfiguration : IEntityTypeConfiguration<SubMenuPermission>
{
    public void Configure(EntityTypeBuilder<SubMenuPermission> builder)
    {
        builder.ToTable("SubMenuPermissions");

        builder.HasKey(smp => smp.Id);

        // Relationships - Use NoAction to avoid cascade cycles
        builder.HasOne(smp => smp.SubMenu)
            .WithMany(sm => sm.SubMenuPermissions)
            .HasForeignKey(smp => smp.SubMenuId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(smp => smp.Permission)
            .WithMany()
            .HasForeignKey(smp => smp.PermissionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(smp => new { smp.SubMenuId, smp.PermissionId })
            .IsUnique();
    }
}
