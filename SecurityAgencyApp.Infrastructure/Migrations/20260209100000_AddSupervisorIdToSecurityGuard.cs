using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorIdToSecurityGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SupervisorId",
                table: "SecurityGuards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityGuards_SupervisorId",
                table: "SecurityGuards",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityGuards_Users_SupervisorId",
                table: "SecurityGuards",
                column: "SupervisorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityGuards_Users_SupervisorId",
                table: "SecurityGuards");

            migrationBuilder.DropIndex(
                name: "IX_SecurityGuards_SupervisorId",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "SecurityGuards");
        }
    }
}
