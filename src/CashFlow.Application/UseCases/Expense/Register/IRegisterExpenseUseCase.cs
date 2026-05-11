using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;

namespace CashFlow.Application.UseCases.Expense.Register;

public interface IRegisterExpenseUseCase
{
    ResponseRegisterExpenseJson Execute(RequestRegisterExpenseJson request);
}
