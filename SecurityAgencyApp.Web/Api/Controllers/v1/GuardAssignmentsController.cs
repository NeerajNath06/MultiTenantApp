using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.GuardAssignments.Commands.CreateAssignment;
using SecurityAgencyApp.Application.Features.GuardAssignments.Commands.UpdateAssignment;
using SecurityAgencyApp.Application.Features.GuardAssignments.Queries.GetAssignmentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class GuardAssignmentsController : GenericListCreateUpdateControllerBase<
    AssignmentListResponseDto,
    GetAssignmentListQuery,
    CreateAssignmentCommand,
    UpdateAssignmentCommand>
{
    public GuardAssignmentsController(IMediator mediator) : base(mediator) { }

    protected override void SetUpdateCommandId(UpdateAssignmentCommand command, Guid id) => command.Id = id;
}
