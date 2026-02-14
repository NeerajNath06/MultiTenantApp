using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Compliance.Queries.GetComplianceSummary;

public class GetComplianceSummaryQueryHandler : IRequestHandler<GetComplianceSummaryQuery, ApiResponse<ComplianceSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetComplianceSummaryQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ComplianceSummaryDto>> Handle(GetComplianceSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<ComplianceSummaryDto>.ErrorResponse("Tenant context not found");

        var tenantId = _tenantContext.TenantId.Value;
        var now = DateTime.UtcNow;
        var expiringSoonEnd = now.AddDays(30);
        var items = new List<ComplianceItemDto>();

        List<Guid> guardIds = new();
        if (request.SupervisorId.HasValue)
        {
            var guards = await _unitOfWork.Repository<SecurityGuard>().FindAsync(
                g => g.TenantId == tenantId && g.SupervisorId == request.SupervisorId.Value,
                cancellationToken);
            guardIds = guards.Select(g => g.Id).Distinct().ToList();
        }

        var trainingRepo = _unitOfWork.Repository<TrainingRecord>();
        var trainingList = guardIds.Count > 0
            ? (await trainingRepo.FindAsync(t => t.TenantId == tenantId && t.IsActive && guardIds.Contains(t.GuardId), cancellationToken)).ToList()
            : (await trainingRepo.FindAsync(t => t.TenantId == tenantId && t.IsActive, cancellationToken)).ToList();

        var trainingByType = trainingList.GroupBy(t => t.TrainingType ?? t.TrainingName ?? "Other").ToDictionary(g => g.Key, g => g.ToList());

        foreach (var grp in trainingByType)
        {
            var list = grp.Value;
            var expired = list.Count(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value < now);
            var expiringSoon = list.Count(t => t.ExpiryDate.HasValue && t.ExpiryDate.Value >= now && t.ExpiryDate.Value <= expiringSoonEnd);
            var minExpiry = list.Where(t => t.ExpiryDate.HasValue).Select(t => t.ExpiryDate!.Value).OrderBy(d => d).FirstOrDefault();
            string status;
            string? dueDate = null;
            string details;
            int affected;
            if (expired > 0)
            {
                status = "non-compliant";
                affected = expired;
                details = $"{expired} guard(s) have expired {grp.Key}";
            }
            else if (expiringSoon > 0)
            {
                status = "warning";
                dueDate = minExpiry == default ? null : minExpiry.ToString("MMM dd, yyyy");
                details = $"{expiringSoon} guard(s) have {grp.Key} expiring soon";
                affected = expiringSoon;
            }
            else
            {
                status = "compliant";
                details = $"All guards with {grp.Key} are valid";
                affected = 0;
            }
            items.Add(new ComplianceItemDto
            {
                Id = "training-" + grp.Key.Replace(" ", "-"),
                Title = grp.Key,
                Category = "training",
                Status = status,
                DueDate = dueDate,
                Details = details,
                AffectedCount = affected
            });
        }

        var docRepo = _unitOfWork.Repository<GuardDocument>();
        List<Guid> guardsForDoc;
        if (guardIds.Count > 0)
            guardsForDoc = guardIds;
        else
            guardsForDoc = (await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => g.TenantId == tenantId, cancellationToken)).Select(g => g.Id).ToList();
        var docs = guardsForDoc.Count > 0
            ? (await docRepo.FindAsync(d => guardsForDoc.Contains(d.GuardId), cancellationToken)).ToList()
            : new List<GuardDocument>();

        var docByType = docs.GroupBy(d => d.DocumentType ?? "Document").ToDictionary(g => g.Key, g => g.ToList());
        foreach (var grp in docByType)
        {
            var list = grp.Value;
            var expired = list.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < now);
            var expiringSoon = list.Count(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value >= now && d.ExpiryDate.Value <= expiringSoonEnd);
            var minExpiry = list.Where(d => d.ExpiryDate.HasValue).Select(d => d.ExpiryDate!.Value).OrderBy(d => d).FirstOrDefault();
            string status;
            string? dueDate = null;
            string details;
            int affected;
            if (expired > 0)
            {
                status = "non-compliant";
                affected = expired;
                details = $"{expired} document(s) expired for {grp.Key}";
            }
            else if (expiringSoon > 0)
            {
                status = "warning";
                dueDate = minExpiry == default ? null : minExpiry.ToString("MMM dd, yyyy");
                details = $"{expiringSoon} document(s) for {grp.Key} expiring soon";
                affected = expiringSoon;
            }
            else
            {
                status = "compliant";
                details = $"All {grp.Key} documents valid";
                affected = 0;
            }
            items.Add(new ComplianceItemDto
            {
                Id = "document-" + grp.Key.Replace(" ", "-"),
                Title = grp.Key,
                Category = "document",
                Status = status,
                DueDate = dueDate,
                Details = details,
                AffectedCount = affected
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ComplianceItemDto
            {
                Id = "overview",
                Title = "Compliance Overview",
                Category = "audit",
                Status = "compliant",
                Details = "No training or document data to assess. Add guards, training records and documents.",
                AffectedCount = 0
            });
        }

        var compliantCount = items.Count(i => i.Status == "compliant");
        var warningCount = items.Count(i => i.Status == "warning");
        var nonCompliantCount = items.Count(i => i.Status == "non-compliant");
        var total = items.Count;
        var overallScore = total > 0 ? (int)Math.Round((compliantCount * 100.0) / total) : 100;

        var result = new ComplianceSummaryDto
        {
            CompliantCount = compliantCount,
            WarningCount = warningCount,
            NonCompliantCount = nonCompliantCount,
            OverallScorePercent = overallScore,
            Items = items.OrderByDescending(i => i.Status == "non-compliant" ? 2 : i.Status == "warning" ? 1 : 0).ThenBy(i => i.Title).ToList()
        };

        return ApiResponse<ComplianceSummaryDto>.SuccessResponse(result, "Compliance summary retrieved");
    }
}
