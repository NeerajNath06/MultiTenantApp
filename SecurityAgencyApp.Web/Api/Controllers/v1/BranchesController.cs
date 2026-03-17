using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Branches;
using SecurityAgencyApp.Application.Features.Branches.Commands.CreateBranch;
using SecurityAgencyApp.Application.Features.Branches.Commands.DeleteBranch;
using SecurityAgencyApp.Application.Features.Branches.Commands.UpdateBranch;
using SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchById;
using SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class BranchesController : GenericCrudControllerBase<
    BranchListResponseDto,
    BranchDto,
    GetBranchListQuery,
    GetBranchByIdQuery,
    CreateBranchCommand,
    UpdateBranchCommand,
    DeleteBranchCommand>
{
    public BranchesController(IMediator mediator) : base(mediator) { }

    protected override GetBranchByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateBranchCommand command, Guid id) => command.Id = id;

    protected override DeleteBranchCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
