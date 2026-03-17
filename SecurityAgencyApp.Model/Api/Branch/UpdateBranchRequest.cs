using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class UpdateBranchRequest : CreateBranchRequest
{
    public Guid Id { get; set; }
}
