using ExpenseTracker.Dto;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/expense")]
public class ExpenseController(IExpenseService service): ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<ICollection<ExpenseResponse>>>> GetExpenseList()
    {
        var response = await service.GetExpenses();

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> CreateExpense(ExpenseRequest request)
    {
        var response = await service.Create(request);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<ExpenseResponse>>> GetExpenseById(int id)
    {
        var response = await service.GetById(id);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse<bool>>> DeleteExpense(int id)
    {
        var response = await service.Delete(id);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateExpense(int id, ExpenseRequest request)
    {
        var response = await service.Update(id, request);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpGet("filter")]
    public async Task<ActionResult<BaseResponse<ICollection<ExpenseResponse>>>> GetFilteredExpenses([FromQuery] ExpenseFilter filter)
    {
        var response = await service.GetFiltered(filter);

        return StatusCode(response.Status, response);
    }
}
