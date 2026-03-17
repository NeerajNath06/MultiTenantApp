using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.SubMenus.Commands.CreateSubMenu;
using SecurityAgencyApp.Application.Features.SubMenus.Commands.DeleteSubMenu;
using SecurityAgencyApp.Application.Features.SubMenus.Commands.UpdateSubMenu;
using SecurityAgencyApp.Application.Features.SubMenus.Queries.GetSubMenuList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class SubMenusController : GenericListCreateUpdateDeleteControllerBase<
    SubMenuListResponseDto,
    GetSubMenuListQuery,
    CreateSubMenuCommand,
    UpdateSubMenuCommand,
    DeleteSubMenuCommand>
{
    public SubMenusController(IMediator mediator) : base(mediator) { }

    protected override void SetUpdateCommandId(UpdateSubMenuCommand command, Guid id) => command.Id = id;

    protected override DeleteSubMenuCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
