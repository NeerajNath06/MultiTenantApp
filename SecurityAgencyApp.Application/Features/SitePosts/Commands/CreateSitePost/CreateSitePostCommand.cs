using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SitePosts.Commands.CreateSitePost;

public class CreateSitePostCommand : IRequest<ApiResponse<Guid>>
{
    public Guid SiteId { get; set; }
    public Guid? BranchId { get; set; }
    public string PostCode { get; set; } = string.Empty;
    public string PostName { get; set; } = string.Empty;
    public string? ShiftName { get; set; }
    public int SanctionedStrength { get; set; }
    public string? GenderRequirement { get; set; }
    public string? SkillRequirement { get; set; }
    public bool RequiresWeapon { get; set; }
    public bool RelieverRequired { get; set; }
    public string? WeeklyOffPattern { get; set; }
    public bool IsActive { get; set; } = true;
}
