using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantProfileAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Tenants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Tenants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Tenants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TenantDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantDocuments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantDocuments_TenantId",
                table: "TenantDocuments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('TenantDocuments'))
                    DROP TABLE [TenantDocuments];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'LogoPath')
                    ALTER TABLE [Tenants] DROP COLUMN [LogoPath];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'TaxId')
                    ALTER TABLE [Tenants] DROP COLUMN [TaxId];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Tenants') AND name = 'Website')
                    ALTER TABLE [Tenants] DROP COLUMN [Website];
            ");
        }
    }
}
