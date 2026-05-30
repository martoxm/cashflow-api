using CashFlow.Communication.Requests;

namespace CashFlow.Application.UseCases.Expense.Update;

public interface IUpdateExpenseUseCase
{
    Task Execute(long id, RequestExpenseJson request);
}
