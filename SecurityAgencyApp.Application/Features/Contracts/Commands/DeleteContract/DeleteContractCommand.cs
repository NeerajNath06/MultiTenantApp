using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Contracts.Commands.DeleteContract;

public class DeleteContractCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
