using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Car"),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DriverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParkingSlot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExitTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleLogs_SecurityGuards_GuardId",
                        column: x => x.GuardId,
                        principalTable: "SecurityGuards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleLogs_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLogs_EntryTime",
                table: "VehicleLogs",
                column: "EntryTime");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLogs_GuardId",
                table: "VehicleLogs",
                column: "GuardId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLogs_SiteId",
                table: "VehicleLogs",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLogs_TenantId",
                table: "VehicleLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLogs_TenantId_EntryTime",
                table: "VehicleLogs",
                columns: new[] { "TenantId", "EntryTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "VehicleLogs");
        }
    }
}
