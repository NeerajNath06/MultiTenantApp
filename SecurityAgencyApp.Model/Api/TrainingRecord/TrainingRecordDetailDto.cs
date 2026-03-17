namespace SecurityAgencyApp.Model.Api;

public class TrainingRecordDetailDto
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
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
