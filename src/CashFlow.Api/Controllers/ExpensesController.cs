using CashFlow.Application.UseCases.Expense.Register;
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpensesController : ControllerBase
{
    [HttpPost]
    public IActionResult Register([FromBody] RequestRegisterExpenseJson request)
    {
        try
        {
            var useCase = new RegisterExpenseUseCase();

            var reponse = useCase.Execute(request);

            return Created(string.Empty, reponse);
        }
        catch (ArgumentException ex)
        {
            var errorResponse = new ResponseErrorJson(ex.Message);

            return BadRequest(errorResponse);
        }
        catch
        {
            var errorResponse = new ResponseErrorJson("Unknown error");

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}