namespace SecurityAgencyApp.Model.Api;

public class UpdateDepartmentRequest : CreateDepartmentRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
