using CashFlow.Application.UseCases.Expense.Register;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Shouldly;

namespace Validators.Tests.Expenses.Resgister;

public class RegisterExpenserValidatorTests
{
    [Fact]
    public void Success()
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseJsonBuilder.Build();


        //Act
        var result = validator.Validate(request);

        //Assert fluentassertions
        //result.IsValid.Should().BeTrue();

        //shouldly
        result.IsValid.ShouldBeTrue();
    }
}