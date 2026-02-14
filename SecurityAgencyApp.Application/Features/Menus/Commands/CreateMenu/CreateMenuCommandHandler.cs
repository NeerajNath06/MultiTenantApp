using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.CreateMenu;

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateMenuCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        var menu = new Menu
        {
            TenantId = _tenantContext.TenantId.Value,
            Name = request.Name,
            DisplayName = request.DisplayName ?? request.Name,
            Icon = request.Icon,
            Route = request.Route,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.Repository<Menu>().AddAsync(menu, cancellationToken);

        // Add permissions if provided
        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            foreach (var permissionId in request.PermissionIds)
            {
                var menuPermission = new MenuPermission
                {
                    MenuId = menu.Id,
                    PermissionId = permissionId
                };
                await _unitOfWork.Repository<MenuPermission>().AddAsync(menuPermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(menu.Id, "Menu created successfully");
    }
}
