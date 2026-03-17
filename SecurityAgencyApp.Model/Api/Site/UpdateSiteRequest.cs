using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class UpdateSiteRequest : CreateSiteRequest
{
    public Guid Id { get; set; }
    public Guid? ClientId { get; set; }
}
