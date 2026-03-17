namespace SecurityAgencyApp.Model.Api;

public class FormTemplateListResponse
{
    public List<FormTemplateItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
