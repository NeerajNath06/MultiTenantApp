using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureTenantDocumentsOriginalFileName : Migration
    {
        private static bool IsPostgres()
        {
            var dir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "appsettings.json")))
                {
                    var config = new ConfigurationBuilder().SetBasePath(dir).AddJsonFile("appsettings.json", optional: true).Build();
                    return string.Equals(config["Database:Provider"] ?? "", "PostgreSQL", StringComparison.OrdinalIgnoreCase);
                }
                dir = Path.GetDirectoryName(dir);
            }
            return false;
        }

        /// <inheritdoc />
        /// <summary>Adds OriginalFileName to TenantDocuments if missing (e.g. table created before this column existed).</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (IsPostgres())
            {
                migrationBuilder.Sql(@"
                    DO $$ BEGIN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TenantDocuments' AND column_name = 'OriginalFileName')
                        THEN ALTER TABLE ""TenantDocuments"" ADD COLUMN ""OriginalFileName"" character varying(255) NULL;
                        END IF;
                    END $$;
                ");
            }
            else
            {
                migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('TenantDocuments'))
                AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDocuments') AND name = 'OriginalFileName')
                    ALTER TABLE [TenantDocuments] ADD [OriginalFileName] nvarchar(255) NULL;
            ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (IsPostgres())
            {
                migrationBuilder.Sql(@"DO $$ BEGIN IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'TenantDocuments' AND column_name = 'OriginalFileName') THEN ALTER TABLE ""TenantDocuments"" DROP COLUMN ""OriginalFileName""; END IF; END $$;");
            }
            else
            {
                migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TenantDocuments') AND name = 'OriginalFileName')
                    ALTER TABLE [TenantDocuments] DROP COLUMN [OriginalFileName];
            ");
            }
        }
    }
}
