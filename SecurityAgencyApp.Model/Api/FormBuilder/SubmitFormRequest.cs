namespace SecurityAgencyApp.Model.Api;

public class SubmitFormRequest
{
    public Guid FormTemplateId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string? Remarks { get; set; }
}
