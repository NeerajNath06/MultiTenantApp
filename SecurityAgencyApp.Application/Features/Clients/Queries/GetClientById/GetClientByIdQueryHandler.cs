using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Clients.Queries.GetClientById;

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ApiResponse<ClientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetClientByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ClientDto>.ErrorResponse("Tenant context not found");
        }

        var clientRepo = _unitOfWork.Repository<Client>();
        var client = await clientRepo.GetByIdAsync(request.Id, cancellationToken);

        if (client == null || client.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<ClientDto>.ErrorResponse("Client not found");
        }

        var clientDto = new ClientDto
        {
            Id = client.Id,
            ClientCode = client.ClientCode,
            CompanyName = client.CompanyName,
            ContactPerson = client.ContactPerson,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            AlternatePhone = client.AlternatePhone,
            Address = client.Address,
            City = client.City,
            State = client.State,
            PinCode = client.PinCode,
            GSTNumber = client.GSTNumber,
            PANNumber = client.PANNumber,
            Website = client.Website,
            Status = client.Status,
            Notes = client.Notes,
            IsActive = client.IsActive,
            CreatedDate = client.CreatedDate,
            ModifiedDate = client.ModifiedDate
        };

        return ApiResponse<ClientDto>.SuccessResponse(clientDto, "Client retrieved successfully");
    }
}
