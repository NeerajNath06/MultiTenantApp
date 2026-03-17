namespace SecurityAgencyApp.Model.Api;

public class AnnouncementItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? PostedByName { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
