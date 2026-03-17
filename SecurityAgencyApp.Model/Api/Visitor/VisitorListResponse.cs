namespace SecurityAgencyApp.Model.Api;

public class VisitorListResponse
{
    public List<VisitorItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
