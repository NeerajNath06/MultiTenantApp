using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Clients.Queries.GetClientList;

public class GetClientListQueryHandler : IRequestHandler<GetClientListQuery, ApiResponse<ClientListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetClientListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ClientListResponseDto>> Handle(GetClientListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ClientListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var clientRepo = _unitOfWork.Repository<Client>();
        var query = clientRepo.GetQueryable()
            .Where(c => c.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || c.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c =>
                c.ClientCode.ToLower().Contains(search) ||
                c.CompanyName.ToLower().Contains(search) ||
                (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(search)) ||
                (c.Email != null && c.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        query = request.SortBy?.ToLower() switch
        {
            "companyname" => request.SortDirection == "asc"
                ? query.OrderBy(c => c.CompanyName)
                : query.OrderByDescending(c => c.CompanyName),
            "clientcode" => request.SortDirection == "asc"
                ? query.OrderBy(c => c.ClientCode)
                : query.OrderByDescending(c => c.ClientCode),
            _ => query.OrderBy(c => c.CompanyName)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var clients = await clientRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var clientDtos = clients.Select(c => new ClientDto
        {
            Id = c.Id,
            ClientCode = c.ClientCode,
            CompanyName = c.CompanyName,
            ContactPerson = c.ContactPerson,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            City = c.City,
            State = c.State,
            Status = c.Status,
            IsActive = c.IsActive,
            CreatedDate = c.CreatedDate
        }).ToList();

        var response = new ClientListResponseDto
        {
            Items = clientDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<ClientListResponseDto>.SuccessResponse(response, "Clients retrieved successfully");
    }
}
