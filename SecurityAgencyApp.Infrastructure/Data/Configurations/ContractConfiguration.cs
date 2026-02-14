using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Infrastructure.Data.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContractNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.BillingCycle)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Monthly");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Draft");

        builder.Property(c => c.ContractValue)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.MonthlyAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Client)
            .WithMany(cl => cl.Contracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.ClientId);
        builder.HasIndex(c => c.ContractNumber);
    }
}

public class ContractSiteConfiguration : IEntityTypeConfiguration<ContractSite>
{
    public void Configure(EntityTypeBuilder<ContractSite> builder)
    {
        builder.ToTable("ContractSites");

        builder.HasKey(cs => cs.Id);

        builder.HasOne(cs => cs.Contract)
            .WithMany(c => c.ContractSites)
            .HasForeignKey(cs => cs.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Site)
            .WithMany(s => s.ContractSites)
            .HasForeignKey(cs => cs.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cs => cs.ContractId);
        builder.HasIndex(cs => cs.SiteId);
    }
}
