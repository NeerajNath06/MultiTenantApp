using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Commands.DeleteTrainingRecord;

public class DeleteTrainingRecordCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
