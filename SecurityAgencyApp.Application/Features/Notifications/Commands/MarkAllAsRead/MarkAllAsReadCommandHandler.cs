using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, ApiResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<int>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Repository<Notification>().FindAsync(
            n => n.UserId == request.UserId && !n.IsRead,
            cancellationToken);
        var count = 0;
        foreach (var n in list)
        {
            n.IsRead = true;
            await _unitOfWork.Repository<Notification>().UpdateAsync(n, cancellationToken);
            count++;
        }
        if (count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<int>.SuccessResponse(count, $"{count} notification(s) marked as read");
    }
}
