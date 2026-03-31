using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureTenantDocumentsOriginalFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.AddColumn<string>(
                    name: "OriginalFileName",
                    table: "TenantDocuments",
                    type: "character varying(255)",
                    maxLength: 255,
                    nullable: true);
            }
            else
            {
                migrationBuilder.AddColumn<string>(
                    name: "OriginalFileName",
                    table: "TenantDocuments",
                    type: "nvarchar(255)",
                    maxLength: 255,
                    nullable: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "TenantDocuments");
        }
    }
}
