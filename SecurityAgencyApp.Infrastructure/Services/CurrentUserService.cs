using SecurityAgencyApp.Application.Interfaces;

namespace SecurityAgencyApp.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}
