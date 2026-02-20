using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndGuardExportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Wages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Wages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "WageDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "WageDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Visitors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Visitors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserSubMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "UserSubMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "UserPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "UserMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "UserMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TrainingRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "TrainingRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TenantDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "TenantDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SubMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "SubMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SubMenuPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "SubMenuPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SiteSupervisors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "SiteSupervisors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Sites",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Sites",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "SecurityGuards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SecurityGuards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "SecurityGuards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "SecurityGuards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IFSCCode",
                table: "SecurityGuards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "SecurityGuards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UAN",
                table: "SecurityGuards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RoleSubMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "RoleSubMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RoleMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "RoleMenus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatrolScans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "PatrolScans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Menus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Menus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MenuPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "MenuPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LeaveRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "LeaveRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "IncidentReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "IncidentReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "IncidentAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "IncidentAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "GuardDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "GuardDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "GuardAttendance",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "GuardAttendance",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "GuardAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "FormTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "FormSubmissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "FormSubmissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "FormSubmissionData",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "FormSubmissionData",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "FormFields",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "FormFields",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Expenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Expenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Equipment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Equipment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Designations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Designations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ContractSites",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "ContractSites",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Bills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Bills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "BillItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "BillItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Announcements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Announcements",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Wages");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Wages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WageDetails");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "WageDetails");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserSubMenus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "UserSubMenus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserMenus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "UserMenus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TrainingRecords");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TrainingRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TenantDocuments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TenantDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SubMenus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SubMenus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SubMenuPermissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SubMenuPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SiteSupervisors");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SiteSupervisors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "IFSCCode",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "UAN",
                table: "SecurityGuards");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RoleSubMenus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RoleSubMenus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatrolScans");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PatrolScans");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MenuPermissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MenuPermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "IncidentReports");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "IncidentReports");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "IncidentAttachments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "IncidentAttachments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "GuardDocuments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "GuardDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "GuardAttendance");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "GuardAttendance");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "GuardAssignments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "FormTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FormSubmissionData");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "FormSubmissionData");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "FormFields");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ContractSites");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ContractSites");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "BillItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Announcements");
        }
    }
}
