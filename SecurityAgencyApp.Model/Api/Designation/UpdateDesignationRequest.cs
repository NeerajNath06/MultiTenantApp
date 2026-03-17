namespace SecurityAgencyApp.Model.Api;

public class UpdateDesignationRequest : CreateDesignationRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
