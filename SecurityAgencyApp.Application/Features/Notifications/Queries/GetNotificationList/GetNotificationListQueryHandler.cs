using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Notifications.Queries.GetNotificationList;

public class GetNotificationListQueryHandler : IRequestHandler<GetNotificationListQuery, ApiResponse<NotificationListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetNotificationListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<NotificationListResponseDto>> Handle(GetNotificationListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<NotificationListResponseDto>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Notification>();
        var query = repo.GetQueryable()
            .Where(n => n.TenantId == _tenantContext.TenantId.Value && n.UserId == request.UserId);

        var unreadCount = query.Count(n => !n.IsRead);

        if (request.IsRead.HasValue)
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        if (!string.IsNullOrWhiteSpace(request.Type))
            query = query.Where(n => n.Type == request.Type);

        query = query.OrderByDescending(n => n.CreatedDate);

        var totalCount = query.Count();
        var list = await repo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var items = list.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedDate
        }).ToList();

        var response = new NotificationListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            UnreadCount = unreadCount
        };
        return ApiResponse<NotificationListResponseDto>.SuccessResponse(response, "Notifications retrieved");
    }
}
