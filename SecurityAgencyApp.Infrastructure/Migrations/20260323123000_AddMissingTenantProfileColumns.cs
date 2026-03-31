using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityAgencyApp.Infrastructure.Migrations;

public partial class AddMissingTenantProfileColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "ActivationStatus", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft");
        migrationBuilder.AddColumn<string>(name: "BillingContactName", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "BillingContactPhone", table: "Tenants", type: "nvarchar(20)", maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<string>(name: "BillingEmail", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<int>(name: "BranchLimit", table: "Tenants", type: "int", nullable: true);
        migrationBuilder.AddColumn<string>(name: "CinNumber", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "CompanyCode", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ComplianceOfficerName", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Currency", table: "Tenants", type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR");
        migrationBuilder.AddColumn<string>(name: "EscalationContactName", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "EscalationContactPhone", table: "Tenants", type: "nvarchar(20)", maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<string>(name: "EsicNumber", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "GstNumber", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "InvoicePrefix", table: "Tenants", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "INV");
        migrationBuilder.AddColumn<bool>(name: "IsKycVerified", table: "Tenants", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<string>(name: "LabourLicenseNumber", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "LegalName", table: "Tenants", type: "nvarchar(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<string>(name: "OnboardingStatus", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending");
        migrationBuilder.AddColumn<bool>(name: "OnboardingChecklistCompleted", table: "Tenants", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<string>(name: "OwnerName", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PanNumber", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PayrollPrefix", table: "Tenants", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PAY");
        migrationBuilder.AddColumn<string>(name: "PfNumber", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<int>(name: "SeatLimit", table: "Tenants", type: "int", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "StorageLimitGb", table: "Tenants", type: "decimal(18,2)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "SubscriptionPlan", table: "Tenants", type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Standard");
        migrationBuilder.AddColumn<string>(name: "SupportEmail", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "TimeZone", table: "Tenants", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Asia/Kolkata");
        migrationBuilder.AddColumn<string>(name: "TradeName", table: "Tenants", type: "nvarchar(200)", maxLength: 200, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ActivationStatus", table: "Tenants");
        migrationBuilder.DropColumn(name: "BillingContactName", table: "Tenants");
        migrationBuilder.DropColumn(name: "BillingContactPhone", table: "Tenants");
        migrationBuilder.DropColumn(name: "BillingEmail", table: "Tenants");
        migrationBuilder.DropColumn(name: "BranchLimit", table: "Tenants");
        migrationBuilder.DropColumn(name: "CinNumber", table: "Tenants");
        migrationBuilder.DropColumn(name: "CompanyCode", table: "Tenants");
        migrationBuilder.DropColumn(name: "ComplianceOfficerName", table: "Tenants");
        migrationBuilder.DropColumn(name: "Currency", table: "Tenants");
        migrationBuilder.DropColumn(name: "EscalationContactName", table: "Tenants");
        migrationBuilder.DropColumn(name: "EscalationContactPhone", table: "Tenants");
        migrationBuilder.DropColumn(name: "EsicNumber", table: "Tenants");
        migrationBuilder.DropColumn(name: "GstNumber", table: "Tenants");
        migrationBuilder.DropColumn(name: "InvoicePrefix", table: "Tenants");
        migrationBuilder.DropColumn(name: "IsKycVerified", table: "Tenants");
        migrationBuilder.DropColumn(name: "LabourLicenseNumber", table: "Tenants");
        migrationBuilder.DropColumn(name: "LegalName", table: "Tenants");
        migrationBuilder.DropColumn(name: "OnboardingStatus", table: "Tenants");
        migrationBuilder.DropColumn(name: "OnboardingChecklistCompleted", table: "Tenants");
        migrationBuilder.DropColumn(name: "OwnerName", table: "Tenants");
        migrationBuilder.DropColumn(name: "PanNumber", table: "Tenants");
        migrationBuilder.DropColumn(name: "PayrollPrefix", table: "Tenants");
        migrationBuilder.DropColumn(name: "PfNumber", table: "Tenants");
        migrationBuilder.DropColumn(name: "SeatLimit", table: "Tenants");
        migrationBuilder.DropColumn(name: "StorageLimitGb", table: "Tenants");
        migrationBuilder.DropColumn(name: "SubscriptionPlan", table: "Tenants");
        migrationBuilder.DropColumn(name: "SupportEmail", table: "Tenants");
        migrationBuilder.DropColumn(name: "TimeZone", table: "Tenants");
        migrationBuilder.DropColumn(name: "TradeName", table: "Tenants");
    }
}
