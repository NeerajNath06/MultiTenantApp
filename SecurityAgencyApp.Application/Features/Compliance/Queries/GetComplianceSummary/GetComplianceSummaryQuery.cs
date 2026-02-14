using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Compliance.Queries.GetComplianceSummary;

public class GetComplianceSummaryQuery : IRequest<ApiResponse<ComplianceSummaryDto>>
{
    /// <summary>When set, return compliance only for guards under this supervisor.</summary>
    public Guid? SupervisorId { get; set; }
}

public class ComplianceSummaryDto
{
    public int CompliantCount { get; set; }
    public int WarningCount { get; set; }
    public int NonCompliantCount { get; set; }
    public int OverallScorePercent { get; set; }
    public List<ComplianceItemDto> Items { get; set; } = new();
}

public class ComplianceItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // license, training, audit, document
    public string Status { get; set; } = string.Empty; // compliant, warning, non-compliant
    public string? DueDate { get; set; }
    public string Details { get; set; } = string.Empty;
    public int AffectedCount { get; set; }
}
