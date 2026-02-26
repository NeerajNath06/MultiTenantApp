using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanDuplicateMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL: UPDATE FROM subquery (works on SQL Server and PostgreSQL). PostgreSQL has no MIN(uuid), use ORDER BY Id LIMIT 1.
            // 1. Point RoleMenus to the kept menu (min Id per TenantId+Name)
            migrationBuilder.Sql(@"
                UPDATE ""RoleMenus"" SET ""MenuId"" = k.""KeptId""
                FROM (
                    SELECT m.""Id"" AS ""DuplicateId"",
                        (SELECT m2.""Id"" FROM ""Menus"" m2 WHERE m2.""TenantId"" = m.""TenantId"" AND m2.""Name"" = m.""Name"" ORDER BY m2.""Id"" LIMIT 1) AS ""KeptId""
                    FROM ""Menus"" m
                    WHERE EXISTS (SELECT 1 FROM ""Menus"" m2 WHERE m2.""TenantId"" = m.""TenantId"" AND m2.""Name"" = m.""Name"" AND m2.""Id"" < m.""Id"")
                ) k
                WHERE ""RoleMenus"".""MenuId"" = k.""DuplicateId"" AND ""RoleMenus"".""MenuId"" <> k.""KeptId"";
            ");

            // Remove duplicate (RoleId, MenuId): delete rows that have another row with same RoleId,MenuId and smaller Id (no MIN(uuid) needed)
            migrationBuilder.Sql(@"DELETE FROM ""RoleMenus"" WHERE ""Id"" IN (SELECT a.""Id"" FROM ""RoleMenus"" a INNER JOIN ""RoleMenus"" b ON a.""RoleId"" = b.""RoleId"" AND a.""MenuId"" = b.""MenuId"" AND a.""Id"" > b.""Id"");");

            // 2. Point UserMenus to the kept menu
            migrationBuilder.Sql(@"
                UPDATE ""UserMenus"" SET ""MenuId"" = k.""KeptId""
                FROM (
                    SELECT m.""Id"" AS ""DuplicateId"",
                        (SELECT m2.""Id"" FROM ""Menus"" m2 WHERE m2.""TenantId"" = m.""TenantId"" AND m2.""Name"" = m.""Name"" ORDER BY m2.""Id"" LIMIT 1) AS ""KeptId""
                    FROM ""Menus"" m
                    WHERE EXISTS (SELECT 1 FROM ""Menus"" m2 WHERE m2.""TenantId"" = m.""TenantId"" AND m2.""Name"" = m.""Name"" AND m2.""Id"" < m.""Id"")
                ) k
                WHERE ""UserMenus"".""MenuId"" = k.""DuplicateId"" AND ""UserMenus"".""MenuId"" <> k.""KeptId"";
            ");

            migrationBuilder.Sql(@"DELETE FROM ""UserMenus"" WHERE ""Id"" IN (SELECT a.""Id"" FROM ""UserMenus"" a INNER JOIN ""UserMenus"" b ON a.""UserId"" = b.""UserId"" AND a.""MenuId"" = b.""MenuId"" AND a.""Id"" > b.""Id"");");

            // 3. Delete duplicate Menus (keep one per TenantId, Name)
            migrationBuilder.Sql(@"
                WITH dups AS (SELECT ""Id"", ROW_NUMBER() OVER (PARTITION BY ""TenantId"", ""Name"" ORDER BY ""Id"") AS rn FROM ""Menus"")
                DELETE FROM ""Menus"" WHERE ""Id"" IN (SELECT ""Id"" FROM dups WHERE rn > 1);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No way to restore deleted duplicates; Down is a no-op
        }
    }
}
