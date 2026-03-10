using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Payments.Commands.DeletePayment;

public class DeletePaymentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
