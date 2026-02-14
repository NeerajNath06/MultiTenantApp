using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternatePhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? Website { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}
