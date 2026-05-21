using CashFlow.Communication.Responses;

namespace CashFlow.Application.UseCases.Expense.GetById;

public interface IGetExpenseByIdUseCase
{
    Task<ResponseExpenseJson> Execute(long id);
}
