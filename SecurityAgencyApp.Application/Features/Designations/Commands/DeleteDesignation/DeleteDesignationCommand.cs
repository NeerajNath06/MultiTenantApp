using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Designations.Commands.DeleteDesignation;

public class DeleteDesignationCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
