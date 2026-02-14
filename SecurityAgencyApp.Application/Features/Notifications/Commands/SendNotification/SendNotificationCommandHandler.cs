using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, ApiResponse<SendNotificationResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public SendNotificationCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SendNotificationResultDto>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<SendNotificationResultDto>.ErrorResponse("Tenant context not found");

        var userIds = (request.UserIds ?? new List<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();
        if (userIds.Count == 0)
            return ApiResponse<SendNotificationResultDto>.ErrorResponse("At least one recipient is required");

        var title = (request.Title ?? string.Empty).Trim();
        var body = (request.Body ?? string.Empty).Trim();
        var type = string.IsNullOrWhiteSpace(request.Type) ? "Info" : request.Type.Trim();
        if (title.Length == 0)
            return ApiResponse<SendNotificationResultDto>.ErrorResponse("Title is required");

        var repo = _unitOfWork.Repository<Notification>();
        foreach (var userId in userIds)
        {
            var entity = new Notification
            {
                TenantId = _tenantContext.TenantId.Value,
                UserId = userId,
                Title = title,
                Body = body,
                Type = type,
                IsRead = false
            };
            await repo.AddAsync(entity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<SendNotificationResultDto>.SuccessResponse(
            new SendNotificationResultDto { SentCount = userIds.Count },
            $"Notification sent to {userIds.Count} recipient(s)");
    }
}
