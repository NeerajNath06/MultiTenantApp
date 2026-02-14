using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.DisplayName)
            .HasMaxLength(200);

        builder.Property(m => m.Icon)
            .HasMaxLength(100);

        builder.Property(m => m.Route)
            .HasMaxLength(500);

        builder.Property(m => m.DisplayOrder)
            .IsRequired();

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.IsSystemMenu)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(m => m.Tenant)
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(m => m.SubMenus)
            .WithOne(sm => sm.Menu)
            .HasForeignKey(sm => sm.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
