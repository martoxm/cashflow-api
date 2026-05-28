using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);
   /// <summary>
   /// This function retuns TRUE if the deletion was successful. Otherswise, it returns FALSE
   /// </summary>
   /// <param name="id"></param>
   /// <returns></returns>
    Task<bool> Delete(long id);
}
