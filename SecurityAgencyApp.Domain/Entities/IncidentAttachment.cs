using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class IncidentAttachment : BaseEntity
{
    public Guid IncidentReportId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // Navigation properties
    public virtual IncidentReport IncidentReport { get; set; } = null!;
}
