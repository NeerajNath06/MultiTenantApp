namespace SecurityAgencyApp.Web.Models.Api;

public class TenantDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
