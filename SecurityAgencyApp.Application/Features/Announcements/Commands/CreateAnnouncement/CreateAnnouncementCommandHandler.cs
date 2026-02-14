using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Announcements.Commands.CreateAnnouncement;

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateAnnouncementCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");

        var entity = new Announcement
        {
            TenantId = _tenantContext.TenantId.Value,
            Title = request.Title.Trim(),
            Content = request.Content?.Trim() ?? string.Empty,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "general" : request.Category.Trim(),
            PostedByUserId = request.PostedByUserId,
            PostedByName = request.PostedByName?.Trim(),
            PostedAt = DateTime.UtcNow,
            IsPinned = request.IsPinned,
            IsActive = true
        };

        await _unitOfWork.Repository<Announcement>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(entity.Id, "Announcement created");
    }
}
