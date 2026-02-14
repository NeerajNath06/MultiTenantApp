using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateClientCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var clientRepo = _unitOfWork.Repository<Client>();
        var client = await clientRepo.GetByIdAsync(request.Id, cancellationToken);

        if (client == null || client.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Client not found");
        }

        // Check if code already exists (excluding current client)
        var existing = await clientRepo.FirstOrDefaultAsync(
            c => c.ClientCode == request.ClientCode && 
                 c.Id != request.Id && 
                 c.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<bool>.ErrorResponse("Client code already exists");
        }

        client.ClientCode = request.ClientCode;
        client.CompanyName = request.CompanyName;
        client.ContactPerson = request.ContactPerson;
        client.Email = request.Email;
        client.PhoneNumber = request.PhoneNumber;
        client.AlternatePhone = request.AlternatePhone;
        client.Address = request.Address;
        client.City = request.City;
        client.State = request.State;
        client.PinCode = request.PinCode;
        client.GSTNumber = request.GSTNumber;
        client.PANNumber = request.PANNumber;
        client.Website = request.Website;
        client.Status = request.Status;
        client.Notes = request.Notes;
        client.ModifiedDate = DateTime.UtcNow;

        await clientRepo.UpdateAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Client updated successfully");
    }
}
