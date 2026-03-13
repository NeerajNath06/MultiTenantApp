using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? GstNumber { get; set; }
    public string? LabourLicenseNumber { get; set; }
    public string? NumberPrefix { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; } = true;
}
