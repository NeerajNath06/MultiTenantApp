using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class BranchListResponse
{
    public List<BranchDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
