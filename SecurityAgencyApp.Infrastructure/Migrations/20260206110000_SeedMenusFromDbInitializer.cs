using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>Inserts the menu list from DbInitializer for every tenant (only where menu Name does not exist). Then assigns all menus to Administrator role.</summary>
    public partial class SeedMenusFromDbInitializer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Same 23 menus as DbInitializer – insert for each tenant where (TenantId, Name) not exists
            var menuRows = new[]
            {
                ("Dashboard", "Dashboard", "fas fa-home", "Home", 1),
                ("Users", "Users", "fas fa-users", "Users", 2),
                ("Departments", "Departments", "fas fa-building", "Departments", 3),
                ("Designations", "Designations", "fas fa-briefcase", "Designations", 4),
                ("Roles", "Roles", "fas fa-user-tag", "Roles", 5),
                ("Menus", "Menus", "fas fa-list", "Menus", 6),
                ("SubMenus", "Sub Menus", "fas fa-list-ul", "SubMenus", 7),
                ("SecurityGuards", "Security Guards", "fas fa-user-shield", "SecurityGuards", 8),
                ("Sites", "Sites", "fas fa-building", "Sites", 9),
                ("GuardAssignments", "Assignments", "fas fa-user-check", "GuardAssignments", 10),
                ("Attendance", "Attendance", "fas fa-calendar-check", "Attendance", 11),
                ("Incidents", "Incidents", "fas fa-exclamation-triangle", "Incidents", 12),
                ("Shifts", "Shifts", "fas fa-clock", "Shifts", 13),
                ("FormBuilder", "Form Builder", "fas fa-file-alt", "FormBuilder", 14),
                ("Bills", "Bills", "fas fa-file-invoice", "Bills", 15),
                ("Wages", "Wages", "fas fa-money-bill-wave", "Wages", 16),
                ("Clients", "Clients", "fas fa-building", "Clients", 17),
                ("Contracts", "Contracts", "fas fa-file-contract", "Contracts", 18),
                ("Payments", "Payments", "fas fa-money-check", "Payments", 19),
                ("LeaveRequests", "Leave Requests", "fas fa-calendar-times", "LeaveRequests", 20),
                ("Expenses", "Expenses", "fas fa-receipt", "Expenses", 21),
                ("TrainingRecords", "Training", "fas fa-graduation-cap", "TrainingRecords", 22),
                ("Equipment", "Equipment", "fas fa-tools", "Equipment", 23)
            };

            foreach (var (name, displayName, icon, route, displayOrder) in menuRows)
            {
                var displayNameEscaped = displayName.Replace("'", "''");
                var iconEscaped = icon.Replace("'", "''");
                var routeEscaped = route.Replace("'", "''");
                var nameEscaped = name.Replace("'", "''");

                migrationBuilder.Sql($@"
                    INSERT INTO Menus (Id, TenantId, Name, DisplayName, Icon, Route, DisplayOrder, IsActive, IsSystemMenu, CreatedDate)
                    SELECT NEWID(), t.Id, N'{nameEscaped}', N'{displayNameEscaped}', N'{iconEscaped}', N'{routeEscaped}', {displayOrder}, 1, 0, GETUTCDATE()
                    FROM Tenants t
                    WHERE NOT EXISTS (SELECT 1 FROM Menus m WHERE m.TenantId = t.Id AND m.Name = N'{nameEscaped}');
                ");
            }

            // Assign all menus to Administrator role (Code = 'ADMIN') for each tenant where not already assigned
            migrationBuilder.Sql(@"
                INSERT INTO RoleMenus (Id, RoleId, MenuId, CreatedDate)
                SELECT NEWID(), r.Id, m.Id, GETUTCDATE()
                FROM Roles r
                INNER JOIN Menus m ON m.TenantId = r.TenantId
                WHERE r.Code = N'ADMIN'
                  AND NOT EXISTS (SELECT 1 FROM RoleMenus rm WHERE rm.RoleId = r.Id AND rm.MenuId = m.Id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down: do not remove menus (they may have been created by seed or other migrations)
        }
    }
}
