namespace SecurityAgencyApp.Model.Api;

public class SubMenuListResponse
{
    public List<SubMenuItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
