using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Sites.Queries.GetSupervisorsBySite;

public class GetSupervisorsBySiteQueryHandler : IRequestHandler<GetSupervisorsBySiteQuery, ApiResponse<GetSupervisorsBySiteResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSupervisorsBySiteQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<GetSupervisorsBySiteResponse>> Handle(GetSupervisorsBySiteQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<GetSupervisorsBySiteResponse>.ErrorResponse("Tenant context not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<GetSupervisorsBySiteResponse>.ErrorResponse("Site not found");

        var siteSupervisors = await _unitOfWork.Repository<SiteSupervisor>().FindAsync(
            ss => ss.SiteId == request.SiteId && ss.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);
        var userIds = siteSupervisors.Select(ss => ss.UserId).Distinct().ToList();
        if (userIds.Count == 0)
        {
            return ApiResponse<GetSupervisorsBySiteResponse>.SuccessResponse(
                new GetSupervisorsBySiteResponse { Items = new List<SupervisorItemDto>() },
                "No supervisors assigned to this site.");
        }

        var users = await _unitOfWork.Repository<User>().FindAsync(
            u => userIds.Contains(u.Id) && u.TenantId == _tenantContext.TenantId.Value && u.IsActive,
            cancellationToken);
        var items = users.Select(u =>
        {
            var displayName = string.IsNullOrWhiteSpace($"{u.FirstName} {u.LastName}".Trim())
                ? (u.UserName ?? "")
                : $"{u.FirstName} {u.LastName}".Trim();
            return new SupervisorItemDto
            {
                Id = u.Id,
                UserName = u.UserName ?? "",
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                Email = u.Email ?? "",
                DisplayName = displayName
            };
        }).OrderBy(x => x.DisplayName).ToList();

        return ApiResponse<GetSupervisorsBySiteResponse>.SuccessResponse(
            new GetSupervisorsBySiteResponse { Items = items },
            "Supervisors for site retrieved.");
    }
}
