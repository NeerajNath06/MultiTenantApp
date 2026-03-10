using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Expenses.Commands.DeleteExpense;

public class DeleteExpenseCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
