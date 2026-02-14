using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Dashboard.Queries.GetDashboardData;

public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, ApiResponse<DashboardDataDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetDashboardDataQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<DashboardDataDto>> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<DashboardDataDto>.ErrorResponse("Tenant context not found");
        }

        var tenantId = _tenantContext.TenantId.Value;

        // Get counts
        var totalUsers = await _unitOfWork.Repository<User>().CountAsync(
            u => u.TenantId == tenantId, cancellationToken);

        var activeUsers = await _unitOfWork.Repository<User>().CountAsync(
            u => u.TenantId == tenantId && u.IsActive, cancellationToken);

        var totalDepartments = await _unitOfWork.Repository<Department>().CountAsync(
            d => d.TenantId == tenantId && d.IsActive, cancellationToken);

        var totalRoles = await _unitOfWork.Repository<Role>().CountAsync(
            r => r.TenantId == tenantId && r.IsActive, cancellationToken);

        var totalGuards = await _unitOfWork.Repository<SecurityGuard>().CountAsync(
            g => g.TenantId == tenantId, cancellationToken);

        var activeGuards = await _unitOfWork.Repository<SecurityGuard>().CountAsync(
            g => g.TenantId == tenantId && g.IsActive, cancellationToken);

        var totalMenus = await _unitOfWork.Repository<Menu>().CountAsync(
            m => m.TenantId == tenantId && m.IsActive, cancellationToken);

        var totalFormSubmissions = await _unitOfWork.Repository<FormSubmission>().CountAsync(
            fs => fs.TenantId == tenantId, cancellationToken);

        var pendingFormSubmissions = await _unitOfWork.Repository<FormSubmission>().CountAsync(
            fs => fs.TenantId == tenantId && fs.Status == Domain.Enums.FormSubmissionStatus.Submitted, cancellationToken);

        // Get recent activities (from audit logs or recent user creations)
        var recentUsers = await _unitOfWork.Repository<User>().FindAsync(
            u => u.TenantId == tenantId,
            cancellationToken);

        var recentActivities = recentUsers
            .OrderByDescending(u => u.CreatedDate)
            .Take(5)
            .Select(u => new RecentActivityDto
            {
                ActivityType = "User Created",
                Description = $"New user '{u.FirstName} {u.LastName}' was created",
                ActivityDate = u.CreatedDate,
                UserName = u.UserName
            })
            .ToList();

        var dashboardData = new DashboardDataDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalDepartments = totalDepartments,
            TotalRoles = totalRoles,
            TotalSecurityGuards = totalGuards,
            ActiveGuards = activeGuards,
            TotalMenus = totalMenus,
            TotalFormSubmissions = totalFormSubmissions,
            PendingFormSubmissions = pendingFormSubmissions,
            RecentActivities = recentActivities
        };

        return ApiResponse<DashboardDataDto>.SuccessResponse(dashboardData, "Dashboard data retrieved successfully");
    }
}
