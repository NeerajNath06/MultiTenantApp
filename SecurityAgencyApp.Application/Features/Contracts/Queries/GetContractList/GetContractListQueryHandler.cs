using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractList;

public class GetContractListQueryHandler : IRequestHandler<GetContractListQuery, ApiResponse<ContractListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetContractListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ContractListResponseDto>> Handle(GetContractListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ContractListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var contractRepo = _unitOfWork.Repository<Contract>();
        var query = contractRepo.GetQueryable()
            .Where(c => c.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || c.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c =>
                c.ContractNumber.ToLower().Contains(search) ||
                c.Title.ToLower().Contains(search));
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(c => c.ClientId == request.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        query = request.SortBy?.ToLower() switch
        {
            "contractnumber" => request.SortDirection == "asc"
                ? query.OrderBy(c => c.ContractNumber)
                : query.OrderByDescending(c => c.ContractNumber),
            "startdate" => request.SortDirection == "asc"
                ? query.OrderBy(c => c.StartDate)
                : query.OrderByDescending(c => c.StartDate),
            "contractvalue" => request.SortDirection == "asc"
                ? query.OrderBy(c => c.ContractValue)
                : query.OrderByDescending(c => c.ContractValue),
            _ => query.OrderByDescending(c => c.StartDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var contracts = await contractRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get client names
        var clientIds = contracts.Select(c => c.ClientId).Distinct().ToList();
        var clients = clientIds.Any()
            ? await _unitOfWork.Repository<Client>().FindAsync(c => clientIds.Contains(c.Id), cancellationToken)
            : new List<Client>();

        var contractDtos = contracts.Select(c => new ContractDto
        {
            Id = c.Id,
            ContractNumber = c.ContractNumber,
            ClientId = c.ClientId,
            ClientName = clients.FirstOrDefault(cl => cl.Id == c.ClientId)?.CompanyName ?? "",
            Title = c.Title,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            ContractValue = c.ContractValue,
            MonthlyAmount = c.MonthlyAmount,
            BillingCycle = c.BillingCycle,
            Status = c.Status,
            IsActive = c.IsActive,
            CreatedDate = c.CreatedDate
        }).ToList();

        var response = new ContractListResponseDto
        {
            Items = contractDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<ContractListResponseDto>.SuccessResponse(response, "Contracts retrieved successfully");
    }
}
