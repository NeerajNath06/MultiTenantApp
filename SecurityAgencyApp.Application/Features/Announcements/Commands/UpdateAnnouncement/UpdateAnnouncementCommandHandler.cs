using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Announcements.Commands.UpdateAnnouncement;

public class UpdateAnnouncementCommandHandler : IRequestHandler<UpdateAnnouncementCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateAnnouncementCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Announcement>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Announcement not found");

        entity.Title = request.Title.Trim();
        entity.Content = request.Content?.Trim() ?? string.Empty;
        entity.Category = string.IsNullOrWhiteSpace(request.Category) ? "general" : request.Category.Trim();
        entity.IsPinned = request.IsPinned;
        entity.IsActive = request.IsActive;
        entity.ModifiedDate = DateTime.UtcNow;

        await repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Announcement updated");
    }
}
