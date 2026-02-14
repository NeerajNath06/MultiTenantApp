namespace SecurityAgencyApp.Web.Models.Api;

public class TrainingRecordListResponse
{
    public List<TrainingRecordItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TrainingRecordItemDto
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

public class CreateTrainingRecordRequest
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
