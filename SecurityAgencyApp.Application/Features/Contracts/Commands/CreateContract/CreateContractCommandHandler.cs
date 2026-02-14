using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Contracts.Commands.CreateContract;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateContractCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        if (request.EndDate <= request.StartDate)
        {
            return ApiResponse<Guid>.ErrorResponse("End date must be after start date");
        }

        // Validate client
        var client = await _unitOfWork.Repository<Client>().GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null || client.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Invalid client");
        }

        // Check if contract number exists
        var contractRepo = _unitOfWork.Repository<Contract>();
        var existing = await contractRepo.FirstOrDefaultAsync(
            c => c.ContractNumber == request.ContractNumber && c.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Contract number already exists");
        }

        var contract = new Contract
        {
            TenantId = _tenantContext.TenantId.Value,
            ContractNumber = request.ContractNumber,
            ClientId = request.ClientId,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ContractValue = request.ContractValue,
            BillingCycle = request.BillingCycle,
            MonthlyAmount = request.MonthlyAmount,
            Status = request.Status,
            TermsAndConditions = request.TermsAndConditions,
            PaymentTerms = request.PaymentTerms,
            NumberOfGuards = request.NumberOfGuards,
            AutoRenewal = request.AutoRenewal,
            RenewalDate = request.RenewalDate,
            Notes = request.Notes,
            IsActive = true
        };

        await contractRepo.AddAsync(contract, cancellationToken);

        // Add contract sites
        if (request.Sites != null && request.Sites.Any())
        {
            foreach (var siteDto in request.Sites)
            {
                var site = await _unitOfWork.Repository<Site>().GetByIdAsync(siteDto.SiteId, cancellationToken);
                if (site == null || site.TenantId != _tenantContext.TenantId.Value)
                {
                    continue; // Skip invalid sites
                }

                var contractSite = new ContractSite
                {
                    ContractId = contract.Id,
                    SiteId = siteDto.SiteId,
                    RequiredGuards = siteDto.RequiredGuards,
                    ShiftDetails = siteDto.ShiftDetails,
                    IsActive = true
                };

                await _unitOfWork.Repository<ContractSite>().AddAsync(contractSite, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(contract.Id, "Contract created successfully");
    }
}
