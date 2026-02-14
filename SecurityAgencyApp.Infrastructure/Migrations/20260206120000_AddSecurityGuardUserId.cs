using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityGuardUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SecurityGuards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityGuards_UserId",
                table: "SecurityGuards",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityGuards_Users_UserId",
                table: "SecurityGuards",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityGuards_Users_UserId",
                table: "SecurityGuards");

            migrationBuilder.DropIndex(
                name: "IX_SecurityGuards_UserId",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SecurityGuards");
        }
    }
}
