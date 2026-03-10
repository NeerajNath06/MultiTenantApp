using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Bills.Commands.DeleteBill;

public class DeleteBillCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
