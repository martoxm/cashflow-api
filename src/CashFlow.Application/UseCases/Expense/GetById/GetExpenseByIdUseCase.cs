using AutoMapper;
using CashFlow.Communication.Responses;
using CashFlow.Domain.Repositories.Expenses;
using CashFlow.Exception;
using CashFlow.Exception.ExceptionBase;

namespace CashFlow.Application.UseCases.Expense.GetById;

public class GetExpenseByIdUseCase : IGetExpenseByIdUseCase
{
    private readonly IExpensesReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetExpenseByIdUseCase(IExpensesReadOnlyRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<ResponseExpenseJson> Execute(long id)
    {
        var result = await _repository.GetById(id);

        if (result is null)
        {
            throw new NotFoundException(ResourceErroMessages.EXPENSE_NOT_FOUND);
        }

        return _mapper.Map<ResponseExpenseJson>(result);
    }
}