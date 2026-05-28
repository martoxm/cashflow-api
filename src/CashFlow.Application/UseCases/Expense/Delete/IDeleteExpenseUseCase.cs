namespace CashFlow.Application.UseCases.Expense.Delete;

public interface IDeleteExpenseUseCase
{
    Task Execute(long id);
}
