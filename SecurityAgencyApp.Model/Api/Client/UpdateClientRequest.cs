using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class UpdateClientRequest : CreateClientRequest
{
    public Guid Id { get; set; }
}
