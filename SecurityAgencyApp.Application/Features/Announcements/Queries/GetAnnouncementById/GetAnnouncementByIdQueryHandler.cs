using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementById;

public class GetAnnouncementByIdQueryHandler : IRequestHandler<GetAnnouncementByIdQuery, ApiResponse<AnnouncementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetAnnouncementByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<AnnouncementDto>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<AnnouncementDto>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Announcement>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<AnnouncementDto>.ErrorResponse("Announcement not found");

        var dto = new AnnouncementDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            Category = entity.Category,
            PostedByUserId = entity.PostedByUserId,
            PostedByName = entity.PostedByName,
            PostedAt = entity.PostedAt,
            IsPinned = entity.IsPinned,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate
        };
        return ApiResponse<AnnouncementDto>.SuccessResponse(dto, "Announcement retrieved");
    }
}
