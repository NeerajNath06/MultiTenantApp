using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBasicDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AadharNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PANNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UAN",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AadharNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PANNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UAN",
                table: "Users");
        }
    }
}
