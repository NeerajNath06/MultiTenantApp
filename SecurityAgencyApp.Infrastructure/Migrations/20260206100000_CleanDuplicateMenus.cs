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
            // 1. Point RoleMenus to the kept menu (min Id per TenantId+Name); then remove duplicate (RoleId, MenuId) rows
            migrationBuilder.Sql(@"
                -- Update RoleMenus: set MenuId to kept menu where current MenuId is a duplicate
                UPDATE rm SET rm.MenuId = k.KeptId
                FROM RoleMenus rm
                INNER JOIN (
                    SELECT m.Id AS DuplicateId,
                        (SELECT MIN(m2.Id) FROM Menus m2 WHERE m2.TenantId = m.TenantId AND m2.Name = m.Name) AS KeptId
                    FROM Menus m
                    WHERE EXISTS (SELECT 1 FROM Menus m2 WHERE m2.TenantId = m.TenantId AND m2.Name = m.Name AND m2.Id < m.Id)
                ) k ON rm.MenuId = k.DuplicateId
                WHERE rm.MenuId <> k.KeptId;
            ");

            // Remove duplicate (RoleId, MenuId) in RoleMenus - keep one per (RoleId, MenuId)
            migrationBuilder.Sql(@"
                WITH kept AS (SELECT MIN(Id) AS Id FROM RoleMenus GROUP BY RoleId, MenuId)
                DELETE FROM RoleMenus WHERE Id NOT IN (SELECT Id FROM kept);
            ");

            // 2. Point UserMenus to the kept menu
            migrationBuilder.Sql(@"
                UPDATE um SET um.MenuId = k.KeptId
                FROM UserMenus um
                INNER JOIN (
                    SELECT m.Id AS DuplicateId,
                        (SELECT MIN(m2.Id) FROM Menus m2 WHERE m2.TenantId = m.TenantId AND m2.Name = m.Name) AS KeptId
                    FROM Menus m
                    WHERE EXISTS (SELECT 1 FROM Menus m2 WHERE m2.TenantId = m.TenantId AND m2.Name = m.Name AND m2.Id < m.Id)
                ) k ON um.MenuId = k.DuplicateId
                WHERE um.MenuId <> k.KeptId;
            ");

            migrationBuilder.Sql(@"
                WITH kept AS (SELECT MIN(Id) AS Id FROM UserMenus GROUP BY UserId, MenuId)
                DELETE FROM UserMenus WHERE Id NOT IN (SELECT Id FROM kept);
            ");

            // 3. Delete duplicate Menus (keep one per TenantId, Name - the one with min Id)
            migrationBuilder.Sql(@"
                WITH dups AS (SELECT Id, ROW_NUMBER() OVER (PARTITION BY TenantId, Name ORDER BY Id) AS rn FROM Menus)
                DELETE FROM Menus WHERE Id IN (SELECT Id FROM dups WHERE rn > 1);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No way to restore deleted duplicates; Down is a no-op
        }
    }
}
