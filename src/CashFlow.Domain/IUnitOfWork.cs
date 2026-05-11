namespace CashFlow.Domain;

public interface IUnitOfWork
{
    void Commit();
}
