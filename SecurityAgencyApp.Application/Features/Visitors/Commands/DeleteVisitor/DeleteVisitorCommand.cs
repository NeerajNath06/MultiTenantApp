using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Visitors.Commands.DeleteVisitor;

public class DeleteVisitorCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
