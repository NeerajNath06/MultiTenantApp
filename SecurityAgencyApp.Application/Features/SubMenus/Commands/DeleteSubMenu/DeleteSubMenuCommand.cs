using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SubMenus.Commands.DeleteSubMenu;

public class DeleteSubMenuCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
