using System.Security.Claims;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.expense;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/expenses")]
public class ExpenseController(IExpenseService service): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BaseResponse<ICollection<ExpenseResponse>>>> GetExpenseList([FromQuery] ExpenseFilter filter)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.GetExpenses(userId, filter);

        return StatusCode(response.Status, response);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BaseResponse<ExpenseSummary>>> GetSummary([FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.GetSummary(userId, dateFrom, dateTo);

        return StatusCode(response.Status, response);
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> CreateExpense(ExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.Create(userId, request);

        return StatusCode(response.Status, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<ExpenseResponse>>> GetExpenseById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.GetById(userId, id);

        return StatusCode(response.Status, response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await service.Delete(userId, id);

        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateExpense(int id, ExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.Update(userId, id, request);

        return StatusCode(response.Status, response);
    }
}
