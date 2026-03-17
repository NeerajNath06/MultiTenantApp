namespace SecurityAgencyApp.Model.Api;

public class CreateShiftRequest
{
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int BreakDuration { get; set; }
}
