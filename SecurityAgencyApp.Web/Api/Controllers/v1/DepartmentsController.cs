using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Departments.Commands.CreateDepartment;
using SecurityAgencyApp.Application.Features.Departments.Commands.DeleteDepartment;
using SecurityAgencyApp.Application.Features.Departments.Commands.UpdateDepartment;
using SecurityAgencyApp.Application.Features.Departments.Queries.GetDepartmentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class DepartmentsController : GenericListCreateUpdateDeleteControllerBase<
    DepartmentListResponseDto,
    GetDepartmentListQuery,
    CreateDepartmentCommand,
    UpdateDepartmentCommand,
    DeleteDepartmentCommand>
{
    public DepartmentsController(IMediator mediator) : base(mediator) { }

    protected override void SetUpdateCommandId(UpdateDepartmentCommand command, Guid id) => command.Id = id;

    protected override DeleteDepartmentCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
