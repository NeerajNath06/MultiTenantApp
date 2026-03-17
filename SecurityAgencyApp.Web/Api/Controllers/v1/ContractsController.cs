using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Contracts.Commands.CreateContract;
using SecurityAgencyApp.Application.Features.Contracts.Commands.DeleteContract;
using SecurityAgencyApp.Application.Features.Contracts.Commands.UpdateContract;
using SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractById;
using SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class ContractsController : GenericCrudControllerBase<
    ContractListResponseDto,
    SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractById.ContractDto,
    GetContractListQuery,
    GetContractByIdQuery,
    CreateContractCommand,
    UpdateContractCommand,
    DeleteContractCommand>
{
    public ContractsController(IMediator mediator) : base(mediator) { }

    protected override GetContractByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateContractCommand command, Guid id) => command.Id = id;

    protected override DeleteContractCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
