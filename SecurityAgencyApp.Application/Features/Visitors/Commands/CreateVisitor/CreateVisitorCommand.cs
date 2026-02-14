using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Visitors.Commands.CreateVisitor;

public class CreateVisitorCommand : IRequest<ApiResponse<CreateVisitorResultDto>>
{
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorType { get; set; } = "Individual";
    public string? CompanyName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? HostName { get; set; }
    public string? HostDepartment { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
}

public class CreateVisitorResultDto
{
    public Guid Id { get; set; }
    public string BadgeNumber { get; set; } = string.Empty;
}
