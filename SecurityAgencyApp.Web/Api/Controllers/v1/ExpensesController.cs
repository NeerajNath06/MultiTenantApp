using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Expenses.Commands.CreateExpense;
using SecurityAgencyApp.Application.Features.Expenses.Commands.DeleteExpense;
using SecurityAgencyApp.Application.Features.Expenses.Queries.GetExpenseById;
using SecurityAgencyApp.Application.Features.Expenses.Queries.GetExpenseList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class ExpensesController : GenericReadCreateDeleteControllerBase<
    ExpenseListResponseDto,
    ExpenseDetailDto,
    GetExpenseListQuery,
    GetExpenseByIdQuery,
    CreateExpenseCommand,
    DeleteExpenseCommand>
{
    public ExpensesController(IMediator mediator) : base(mediator) { }

    protected override GetExpenseByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override DeleteExpenseCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
