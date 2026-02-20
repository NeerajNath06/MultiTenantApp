using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IncidentReports_TenantId",
                table: "IncidentReports");

            migrationBuilder.DropIndex(
                name: "IX_GuardAttendance_TenantId",
                table: "GuardAttendance");

            migrationBuilder.DropIndex(
                name: "IX_GuardAssignments_TenantId",
                table: "GuardAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_TenantId_IncidentDate",
                table: "IncidentReports",
                columns: new[] { "TenantId", "IncidentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuardAttendance_TenantId_AttendanceDate",
                table: "GuardAttendance",
                columns: new[] { "TenantId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuardAssignments_TenantId_AssignmentStartDate",
                table: "GuardAssignments",
                columns: new[] { "TenantId", "AssignmentStartDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IncidentReports_TenantId_IncidentDate",
                table: "IncidentReports");

            migrationBuilder.DropIndex(
                name: "IX_GuardAttendance_TenantId_AttendanceDate",
                table: "GuardAttendance");

            migrationBuilder.DropIndex(
                name: "IX_GuardAssignments_TenantId_AssignmentStartDate",
                table: "GuardAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_TenantId",
                table: "IncidentReports",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardAttendance_TenantId",
                table: "GuardAttendance",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardAssignments_TenantId",
                table: "GuardAssignments",
                column: "TenantId");
        }
    }
}
