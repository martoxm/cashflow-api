using CashFlow.Application.UseCases.Expense.Register;
using CashFlow.Communication.Enums;
using CashFlow.Communication.Requests;

namespace Validators.Tests.Expenses.Resgister;

public class RegisterExpenserValidatorTests
{
    [Fact]
    public void Success()
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = new RequestRegisterExpenseJson
        {
            Amount = 100,
            Date = DateTime.Now.AddDays(-1),
            Descrition = "Test",
            PaymentType = PaymentType.CreditCard,
            Title = "Apple"
        };

        //Act
        var result = validator.Validate(request);

        //Assert
        Assert.True(result.IsValid);
    }
}