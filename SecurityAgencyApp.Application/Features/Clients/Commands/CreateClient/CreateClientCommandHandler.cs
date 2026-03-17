using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateClientCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        var clientRepo = _unitOfWork.Repository<Client>();
        var existing = await clientRepo.FirstOrDefaultAsync(
            c => c.ClientCode == request.ClientCode && c.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Client code already exists");
        }

        var client = new Client
        {
            TenantId = _tenantContext.TenantId.Value,
            ClientCode = request.ClientCode,
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AlternatePhone = request.AlternatePhone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PinCode = request.PinCode,
            BillingAddress = request.BillingAddress,
            BillingCity = request.BillingCity,
            BillingState = request.BillingState,
            BillingPinCode = request.BillingPinCode,
            GSTNumber = request.GSTNumber,
            PANNumber = request.PANNumber,
            Website = request.Website,
            AccountManagerName = request.AccountManagerName,
            BillingContactName = request.BillingContactName,
            BillingContactEmail = request.BillingContactEmail,
            EscalationContactName = request.EscalationContactName,
            EscalationContactEmail = request.EscalationContactEmail,
            CreditPeriodDays = request.CreditPeriodDays,
            BillingCycle = request.BillingCycle,
            GstState = request.GstState,
            PaymentModePreference = request.PaymentModePreference,
            TaxTreatment = request.TaxTreatment,
            InvoicePrefix = request.InvoicePrefix,
            SlaTerms = request.SlaTerms,
            EscalationTatHours = request.EscalationTatHours,
            PenaltyTerms = request.PenaltyTerms,
            Status = request.Status,
            Notes = request.Notes,
            IsActive = true
        };

        await clientRepo.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(client.Id);
    }
}
