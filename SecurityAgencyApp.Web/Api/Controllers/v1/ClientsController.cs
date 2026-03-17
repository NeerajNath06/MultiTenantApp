using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Clients.Commands.CreateClient;
using SecurityAgencyApp.Application.Features.Clients.Commands.DeleteClient;
using SecurityAgencyApp.Application.Features.Clients.Commands.UpdateClient;
using SecurityAgencyApp.Application.Features.Clients.Queries.GetClientById;
using SecurityAgencyApp.Application.Features.Clients.Queries.GetClientList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class ClientsController : GenericCrudControllerBase<
    ClientListResponseDto,
    SecurityAgencyApp.Application.Features.Clients.Queries.GetClientById.ClientDto,
    GetClientListQuery,
    GetClientByIdQuery,
    CreateClientCommand,
    UpdateClientCommand,
    DeleteClientCommand>
{
    public ClientsController(IMediator mediator) : base(mediator) { }

    protected override GetClientByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateClientCommand command, Guid id) => command.Id = id;

    protected override DeleteClientCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
