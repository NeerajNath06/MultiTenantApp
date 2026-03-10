using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations;

[DbContext(typeof(SecurityAgencyApp.Infrastructure.Data.ApplicationDbContext))]
[Migration("20260310090000_AddSiteRatePlansAndMonthlySnapshots")]
public class AddSiteRatePlansAndMonthlySnapshots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Bills: add month/year + rate snapshot
        migrationBuilder.AddColumn<int>(
            name: "BillMonth",
            table: "Bills",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "BillYear",
            table: "Bills",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "RateAmount",
            table: "Bills",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Bills_TenantId_SiteId_BillYear_BillMonth",
            table: "Bills",
            columns: new[] { "TenantId", "SiteId", "BillYear", "BillMonth" });

        // Wages: add site + month/year + rate snapshot
        migrationBuilder.AddColumn<Guid>(
            name: "SiteId",
            table: "Wages",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "WageMonth",
            table: "Wages",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "WageYear",
            table: "Wages",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "RateAmount",
            table: "Wages",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Wages_TenantId_SiteId_WageYear_WageMonth",
            table: "Wages",
            columns: new[] { "TenantId", "SiteId", "WageYear", "WageMonth" });

        // SiteRatePlans
        migrationBuilder.CreateTable(
            name: "SiteRatePlans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RateAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SiteRatePlans", x => x.Id);
                table.ForeignKey(
                    name: "FK_SiteRatePlans_Clients_ClientId",
                    column: x => x.ClientId,
                    principalTable: "Clients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SiteRatePlans_Sites_SiteId",
                    column: x => x.SiteId,
                    principalTable: "Sites",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SiteRatePlans_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SiteRatePlans_TenantId_SiteId_EffectiveFrom",
            table: "SiteRatePlans",
            columns: new[] { "TenantId", "SiteId", "EffectiveFrom" });

        migrationBuilder.CreateIndex(
            name: "IX_SiteRatePlans_TenantId_SiteId",
            table: "SiteRatePlans",
            columns: new[] { "TenantId", "SiteId" });

        migrationBuilder.CreateIndex(
            name: "IX_SiteRatePlans_ClientId",
            table: "SiteRatePlans",
            column: "ClientId");

        migrationBuilder.CreateIndex(
            name: "IX_SiteRatePlans_SiteId",
            table: "SiteRatePlans",
            column: "SiteId");

        // Optional FK from Wages.SiteId to Sites (nullable)
        migrationBuilder.CreateIndex(
            name: "IX_Wages_SiteId",
            table: "Wages",
            column: "SiteId");

        migrationBuilder.AddForeignKey(
            name: "FK_Wages_Sites_SiteId",
            table: "Wages",
            column: "SiteId",
            principalTable: "Sites",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Wages_Sites_SiteId", table: "Wages");

        migrationBuilder.DropTable(name: "SiteRatePlans");

        migrationBuilder.DropIndex(name: "IX_Bills_TenantId_SiteId_BillYear_BillMonth", table: "Bills");
        migrationBuilder.DropIndex(name: "IX_Wages_TenantId_SiteId_WageYear_WageMonth", table: "Wages");
        migrationBuilder.DropIndex(name: "IX_Wages_SiteId", table: "Wages");

        migrationBuilder.DropColumn(name: "BillMonth", table: "Bills");
        migrationBuilder.DropColumn(name: "BillYear", table: "Bills");
        migrationBuilder.DropColumn(name: "RateAmount", table: "Bills");

        migrationBuilder.DropColumn(name: "SiteId", table: "Wages");
        migrationBuilder.DropColumn(name: "WageMonth", table: "Wages");
        migrationBuilder.DropColumn(name: "WageYear", table: "Wages");
        migrationBuilder.DropColumn(name: "RateAmount", table: "Wages");
    }
}

