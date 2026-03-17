using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class UpdateSitePostRequest : CreateSitePostRequest
{
    public Guid Id { get; set; }
}
