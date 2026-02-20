using SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;
using TenantProfileDto = SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile.TenantProfileDto;

namespace SecurityAgencyApp.API.Services;

/// <summary>Build enterprise report header from tenant profile for export (PDF/Excel).</summary>
public static class ExportReportHeaderBuilder
{
    public static ExportReportHeader? Build(TenantProfileDto? profile, string reportTitle, string? reportSubTitle = null)
    {
        if (profile == null) return null;
        return new ExportReportHeader
        {
            AgencyName = profile.CompanyName,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            PinCode = profile.PinCode,
            Phone = profile.Phone,
            Email = profile.Email,
            ReportTitle = reportTitle,
            ReportSubTitle = reportSubTitle,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    public static ExportReportHeader? BuildWithDateRange(TenantProfileDto? profile, string reportTitle, DateTime? startDate, DateTime? endDate)
    {
        var sub = startDate.HasValue || endDate.HasValue
            ? "Period: " + (startDate?.ToString("dd-MMM-yyyy") ?? "…") + " to " + (endDate?.ToString("dd-MMM-yyyy") ?? "…")
            : null;
        return Build(profile, reportTitle, sub);
    }
}
