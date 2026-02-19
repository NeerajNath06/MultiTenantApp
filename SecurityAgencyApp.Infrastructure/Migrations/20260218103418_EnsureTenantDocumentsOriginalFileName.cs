using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureTenantDocumentsOriginalFileName : Migration
    {
        /// <inheritdoc />
        /// <summary>Adds OriginalFileName to TenantDocuments if missing (e.g. table created before this column existed).</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('TenantDocuments'))
                AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDocuments') AND name = 'OriginalFileName')
                    ALTER TABLE [TenantDocuments] ADD [OriginalFileName] nvarchar(255) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDocuments') AND name = 'OriginalFileName')
                    ALTER TABLE [TenantDocuments] DROP COLUMN [OriginalFileName];
            ");
        }
    }
}
