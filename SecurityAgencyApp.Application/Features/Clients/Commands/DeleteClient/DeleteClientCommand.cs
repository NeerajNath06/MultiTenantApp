using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Clients.Commands.DeleteClient;

public class DeleteClientCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
