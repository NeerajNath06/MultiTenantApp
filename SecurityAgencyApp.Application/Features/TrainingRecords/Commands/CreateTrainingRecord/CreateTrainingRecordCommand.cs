using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Commands.CreateTrainingRecord;

public class CreateTrainingRecordCommand : IRequest<ApiResponse<Guid>>
{
    public Guid GuardId { get; set; }
    public string TrainingType { get; set; } = string.Empty;
    public string TrainingName { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public DateTime TrainingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Completed";
    public string? CertificateNumber { get; set; }
    public decimal? Score { get; set; }
    public string? Remarks { get; set; }
}
