using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractById;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ApiResponse<ContractDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetContractByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ContractDto>> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ContractDto>.ErrorResponse("Tenant context not found");
        }

        var contractRepo = _unitOfWork.Repository<Contract>();
        var contract = await contractRepo.GetByIdAsync(request.Id, cancellationToken);

        if (contract == null || contract.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<ContractDto>.ErrorResponse("Contract not found");
        }

        var client = await _unitOfWork.Repository<Client>().GetByIdAsync(contract.ClientId, cancellationToken);
        var contractSites = await _unitOfWork.Repository<ContractSite>().FindAsync(
            cs => cs.ContractId == contract.Id && cs.IsActive, cancellationToken);

        var siteIds = contractSites.Select(cs => cs.SiteId).ToList();
        var sites = siteIds.Any()
            ? await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)
            : new List<Site>();

        var contractDto = new ContractDto
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            ClientId = contract.ClientId,
            ClientName = client?.CompanyName ?? "",
            Title = contract.Title,
            Description = contract.Description,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            ContractValue = contract.ContractValue,
            BillingCycle = contract.BillingCycle,
            MonthlyAmount = contract.MonthlyAmount,
            Status = contract.Status,
            TermsAndConditions = contract.TermsAndConditions,
            PaymentTerms = contract.PaymentTerms,
            NumberOfGuards = contract.NumberOfGuards,
            AutoRenewal = contract.AutoRenewal,
            RenewalDate = contract.RenewalDate,
            Notes = contract.Notes,
            IsActive = contract.IsActive,
            CreatedDate = contract.CreatedDate,
            Sites = contractSites.Select(cs => new ContractSiteDto
            {
                SiteId = cs.SiteId,
                SiteName = sites.FirstOrDefault(s => s.Id == cs.SiteId)?.SiteName ?? "",
                RequiredGuards = cs.RequiredGuards,
                ShiftDetails = cs.ShiftDetails
            }).ToList()
        };

        return ApiResponse<ContractDto>.SuccessResponse(contractDto, "Contract retrieved successfully");
    }
}
