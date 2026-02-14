using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Contracts.Commands.UpdateContract;

public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateContractCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        if (request.EndDate <= request.StartDate)
        {
            return ApiResponse<bool>.ErrorResponse("End date must be after start date");
        }

        var contractRepo = _unitOfWork.Repository<Contract>();
        var contract = await contractRepo.GetByIdAsync(request.Id, cancellationToken);

        if (contract == null || contract.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Contract not found");
        }

        // Check if contract number exists (excluding current contract)
        var existing = await contractRepo.FirstOrDefaultAsync(
            c => c.ContractNumber == request.ContractNumber && 
                 c.Id != request.Id && 
                 c.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<bool>.ErrorResponse("Contract number already exists");
        }

        // Validate client
        var client = await _unitOfWork.Repository<Client>().GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null || client.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Invalid client");
        }

        contract.ContractNumber = request.ContractNumber;
        contract.ClientId = request.ClientId;
        contract.Title = request.Title;
        contract.Description = request.Description;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.ContractValue = request.ContractValue;
        contract.BillingCycle = request.BillingCycle;
        contract.MonthlyAmount = request.MonthlyAmount;
        contract.Status = request.Status;
        contract.TermsAndConditions = request.TermsAndConditions;
        contract.PaymentTerms = request.PaymentTerms;
        contract.NumberOfGuards = request.NumberOfGuards;
        contract.AutoRenewal = request.AutoRenewal;
        contract.RenewalDate = request.RenewalDate;
        contract.Notes = request.Notes;
        contract.ModifiedDate = DateTime.UtcNow;

        await contractRepo.UpdateAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Contract updated successfully");
    }
}
