using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class SubMenuConfiguration : IEntityTypeConfiguration<SubMenu>
{
    public void Configure(EntityTypeBuilder<SubMenu> builder)
    {
        builder.ToTable("SubMenus");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sm => sm.DisplayName)
            .HasMaxLength(200);

        builder.Property(sm => sm.Icon)
            .HasMaxLength(100);

        builder.Property(sm => sm.Route)
            .HasMaxLength(500);

        builder.Property(sm => sm.DisplayOrder)
            .IsRequired();

        builder.Property(sm => sm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
            builder.HasOne(sm => sm.Tenant)
                .WithMany()
                .HasForeignKey(sm => sm.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sm => sm.Menu)
            .WithMany(m => m.SubMenus)
            .HasForeignKey(sm => sm.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
