using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.DeleteMenu;

public class DeleteMenuCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
