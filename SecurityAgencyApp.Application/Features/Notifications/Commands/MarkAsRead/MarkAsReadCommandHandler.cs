using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<Notification>().GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.UserId != request.UserId)
            return ApiResponse<bool>.ErrorResponse("Notification not found");

        entity.IsRead = true;
        await _unitOfWork.Repository<Notification>().UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Marked as read");
    }
}
