using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardList;

public class GetGuardListQueryHandler : IRequestHandler<GetGuardListQuery, ApiResponse<GuardListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetGuardListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<GuardListResponseDto>> Handle(GetGuardListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<GuardListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var guardRepo = _unitOfWork.Repository<SecurityGuard>();
        var query = guardRepo.GetQueryable()
            .Where(g => g.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || g.IsActive));

        if (request.SupervisorId.HasValue)
        {
            var supId = request.SupervisorId.Value;
            var supervisedSiteIds = (await _unitOfWork.Repository<SiteSupervisor>().FindAsync(
                ss => ss.UserId == supId,
                cancellationToken)).Select(ss => ss.SiteId).Distinct().ToList();
            var guardIdsFromSites = supervisedSiteIds.Count > 0
                ? (await _unitOfWork.Repository<GuardAssignment>().FindAsync(
                    ga => supervisedSiteIds.Contains(ga.SiteId),
                    cancellationToken)).Select(ga => ga.GuardId).Distinct().ToList()
                : new List<Guid>();
            var guardIdsFromSupervisor = (await guardRepo.FindAsync(
                g => g.SupervisorId == supId,
                cancellationToken)).Select(g => g.Id).ToList();
            var allGuardIds = guardIdsFromSupervisor.Union(guardIdsFromSites).Distinct().ToList();
            if (allGuardIds.Count == 0)
                query = query.Where(_ => false);
            else
                query = query.Where(g => allGuardIds.Contains(g.Id));
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(g =>
                g.FirstName.ToLower().Contains(search) ||
                g.LastName.ToLower().Contains(search) ||
                g.GuardCode.ToLower().Contains(search) ||
                (g.Email != null && g.Email.ToLower().Contains(search)) ||
                (g.PhoneNumber != null && g.PhoneNumber.Contains(search)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc" 
                ? query.OrderByDescending(g => g.FirstName).ThenByDescending(g => g.LastName)
                : query.OrderBy(g => g.FirstName).ThenBy(g => g.LastName),
            "code" => request.SortDirection == "desc"
                ? query.OrderByDescending(g => g.GuardCode)
                : query.OrderBy(g => g.GuardCode),
            "email" => request.SortDirection == "desc"
                ? query.OrderByDescending(g => g.Email)
                : query.OrderBy(g => g.Email),
            "phone" => request.SortDirection == "desc"
                ? query.OrderByDescending(g => g.PhoneNumber)
                : query.OrderBy(g => g.PhoneNumber),
            "created" => request.SortDirection == "desc"
                ? query.OrderByDescending(g => g.CreatedDate)
                : query.OrderBy(g => g.CreatedDate),
            _ => query.OrderBy(g => g.GuardCode)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var guards = await guardRepo.GetPagedAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var supervisorIds = guards.Where(g => g.SupervisorId.HasValue).Select(g => g.SupervisorId!.Value).Distinct().ToList();
        var supervisors = supervisorIds.Any()
            ? await _unitOfWork.Repository<User>().FindAsync(u => supervisorIds.Contains(u.Id), cancellationToken)
            : new List<User>();

        var guardDtos = new List<GuardDto>();
        foreach (var guard in guards)
        {
            var supervisor = guard.SupervisorId.HasValue ? supervisors.FirstOrDefault(s => s.Id == guard.SupervisorId.Value) : null;
            guardDtos.Add(new GuardDto
            {
                Id = guard.Id,
                GuardCode = guard.GuardCode,
                FirstName = guard.FirstName,
                LastName = guard.LastName,
                Email = guard.Email,
                PhoneNumber = guard.PhoneNumber,
                Gender = guard.Gender.ToString(),
                DateOfBirth = guard.DateOfBirth,
                IsActive = guard.IsActive,
                CreatedDate = guard.CreatedDate,
                SupervisorId = guard.SupervisorId,
                SupervisorName = supervisor != null ? $"{supervisor.FirstName} {supervisor.LastName}".Trim() : null
            });
        }

        var response = new GuardListResponseDto
        {
            Items = guardDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<GuardListResponseDto>.SuccessResponse(response, "Security guards retrieved successfully");
    }
}
