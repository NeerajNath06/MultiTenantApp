namespace SecurityAgencyApp.Model.Api;

public class ShiftItemDto
{
    public Guid Id { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int BreakDuration { get; set; }
    public bool IsActive { get; set; }
}
