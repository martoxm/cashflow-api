namespace CashFlow.Domain;

public interface IUnitOfWork
{
    Task Commit();
}
