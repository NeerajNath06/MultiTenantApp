using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.TrainingRecords.Commands.CreateTrainingRecord;
using SecurityAgencyApp.Application.Features.TrainingRecords.Commands.DeleteTrainingRecord;
using SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordById;
using SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class TrainingRecordsController : GenericReadCreateDeleteControllerBase<
    TrainingRecordListResponseDto,
    TrainingRecordDetailDto,
    GetTrainingRecordListQuery,
    GetTrainingRecordByIdQuery,
    CreateTrainingRecordCommand,
    DeleteTrainingRecordCommand>
{
    public TrainingRecordsController(IMediator mediator) : base(mediator) { }

    protected override GetTrainingRecordByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override DeleteTrainingRecordCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
