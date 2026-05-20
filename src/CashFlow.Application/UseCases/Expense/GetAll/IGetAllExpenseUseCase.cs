using CashFlow.Communication.Responses;

namespace CashFlow.Application.UseCases.Expense.GetAll;

public interface IGetAllExpenseUseCase
{
    Task<ResponseExpensesJson> Execute();
}