using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementList;

public class GetAnnouncementListQueryHandler : IRequestHandler<GetAnnouncementListQuery, ApiResponse<AnnouncementListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetAnnouncementListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<AnnouncementListResponseDto>> Handle(GetAnnouncementListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<AnnouncementListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var repo = _unitOfWork.Repository<Announcement>();
        var query = repo.GetQueryable()
            .Where(a => a.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || a.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(search) ||
                (a.Content != null && a.Content.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(a => a.Category == request.Category);
        }

        if (request.IsPinned.HasValue)
        {
            query = query.Where(a => a.IsPinned == request.IsPinned.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDirection == "asc"
                ? query.OrderBy(a => a.Title)
                : query.OrderByDescending(a => a.Title),
            "postedat" => request.SortDirection == "asc"
                ? query.OrderBy(a => a.PostedAt)
                : query.OrderByDescending(a => a.PostedAt),
            "category" => request.SortDirection == "asc"
                ? query.OrderBy(a => a.Category)
                : query.OrderByDescending(a => a.Category),
            _ => query.OrderByDescending(a => a.IsPinned).ThenByDescending(a => a.PostedAt)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var list = await repo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var items = list.Select(a => new AnnouncementDto
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            Category = a.Category,
            PostedByName = a.PostedByName,
            PostedAt = a.PostedAt,
            IsPinned = a.IsPinned,
            IsActive = a.IsActive,
            CreatedDate = a.CreatedDate
        }).ToList();

        var response = new AnnouncementListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<AnnouncementListResponseDto>.SuccessResponse(response, "Announcements retrieved successfully");
    }
}
