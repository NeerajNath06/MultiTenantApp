using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class UserMenuConfiguration : IEntityTypeConfiguration<UserMenu>
{
    public void Configure(EntityTypeBuilder<UserMenu> builder)
    {
        builder.ToTable("UserMenus");

        builder.HasKey(um => um.Id);

        // Relationships - Use NoAction to avoid cascade cycles
        builder.HasOne(um => um.User)
            .WithMany(u => u.UserMenus)
            .HasForeignKey(um => um.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(um => um.Menu)
            .WithMany(m => m.UserMenus)
            .HasForeignKey(um => um.MenuId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(um => new { um.UserId, um.MenuId })
            .IsUnique();
    }
}
