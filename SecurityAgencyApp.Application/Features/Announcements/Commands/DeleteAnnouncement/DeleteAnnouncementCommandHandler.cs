using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Announcements.Commands.DeleteAnnouncement;

public class DeleteAnnouncementCommandHandler : IRequestHandler<DeleteAnnouncementCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteAnnouncementCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Announcement>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Announcement not found");

        await repo.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Announcement deleted");
    }
}
