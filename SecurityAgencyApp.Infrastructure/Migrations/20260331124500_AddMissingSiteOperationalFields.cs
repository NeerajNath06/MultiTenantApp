using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SecurityAgencyApp.Infrastructure.Data;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260331124500_AddMissingSiteOperationalFields")]
public partial class AddMissingSiteOperationalFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "BranchId",
            table: "Sites",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactName",
            table: "Sites",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactPhone",
            table: "Sites",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MusterPoint",
            table: "Sites",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AccessZoneNotes",
            table: "Sites",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SiteInstructionBook",
            table: "Sites",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "GeofenceExceptionNotes",
            table: "Sites",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Sites_BranchId",
            table: "Sites",
            column: "BranchId");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Sites_BranchId",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "BranchId",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "EmergencyContactName",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "EmergencyContactPhone",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "MusterPoint",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "AccessZoneNotes",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "SiteInstructionBook",
            table: "Sites");

        migrationBuilder.DropColumn(
            name: "GeofenceExceptionNotes",
            table: "Sites");
    }
}
