using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Announcements.Commands.CreateAnnouncement;
using SecurityAgencyApp.Application.Features.Announcements.Commands.DeleteAnnouncement;
using SecurityAgencyApp.Application.Features.Announcements.Commands.UpdateAnnouncement;
using SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementById;
using SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementList;
using AnnouncementByIdDto = SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementById.AnnouncementDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class AnnouncementsController : GenericCrudControllerBase<
    AnnouncementListResponseDto,
    AnnouncementByIdDto,
    GetAnnouncementListQuery,
    GetAnnouncementByIdQuery,
    CreateAnnouncementCommand,
    UpdateAnnouncementCommand,
    DeleteAnnouncementCommand>
{
    public AnnouncementsController(IMediator mediator) : base(mediator) { }

    protected override GetAnnouncementByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateAnnouncementCommand command, Guid id) => command.Id = id;

    protected override DeleteAnnouncementCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
