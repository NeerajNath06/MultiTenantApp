using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations;

/// <summary>Add EPF/ESIC/Allowance % and EpfWageCap to SiteRatePlans for wages configuration.</summary>
public partial class AddWagePercentsToSiteRatePlan : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "EpfPercent",
            table: "SiteRatePlans",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "EsicPercent",
            table: "SiteRatePlans",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "AllowancePercent",
            table: "SiteRatePlans",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "EpfWageCap",
            table: "SiteRatePlans",
            type: "decimal(18,2)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "EpfPercent", table: "SiteRatePlans");
        migrationBuilder.DropColumn(name: "EsicPercent", table: "SiteRatePlans");
        migrationBuilder.DropColumn(name: "AllowancePercent", table: "SiteRatePlans");
        migrationBuilder.DropColumn(name: "EpfWageCap", table: "SiteRatePlans");
    }
}
