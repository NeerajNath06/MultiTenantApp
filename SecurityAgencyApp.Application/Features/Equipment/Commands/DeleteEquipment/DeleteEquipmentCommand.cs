using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Equipment.Commands.DeleteEquipment;

public class DeleteEquipmentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
