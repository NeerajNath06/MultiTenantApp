using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordList;

public class GetTrainingRecordListQuery : IRequest<ApiResponse<TrainingRecordListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public Guid? GuardId { get; set; }
    public string? TrainingType { get; set; }
    public string? Status { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class TrainingRecordListResponseDto
{
    public List<TrainingRecordDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TrainingRecordDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string GuardCode { get; set; } = string.Empty;
    public string TrainingType { get; set; } = string.Empty;
    public string TrainingName { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public DateTime TrainingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CertificateNumber { get; set; }
    public decimal? Score { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
