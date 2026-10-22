using System.Security.Claims;
using ExpenseTracker.Dto.expense;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

/// <summary>
/// Expenses of the logged-in user. Requires a JWT access token; users only ever see their own expenses.
/// </summary>
[Authorize]
[ApiController]
[Route("api/expenses")]
public class ExpenseController(IExpenseService service): ControllerBase
{
    /// <summary>
    /// Returns the user's expenses, newest first, optionally filtered.
    /// </summary>
    /// <remarks>
    /// All filters are optional and can be combined. `dateFrom` and `dateTo` are inclusive,
    /// `currency` is case-insensitive and the amount range is inclusive on both ends.
    ///
    /// Example: `GET /api/expenses?dateFrom=2026-10-01&amp;dateTo=2026-10-31&amp;categoryId=1&amp;currency=USD`
    /// </remarks>
    /// <param name="filter">Category, date range, currency and amount range to filter by.</param>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<ICollection<ExpenseResponse>>>> GetExpenseList([FromQuery] ExpenseFilter filter)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.GetExpenses(userId, filter);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Returns the total amount and the totals per category for a date range.
    /// </summary>
    /// <remarks>
    /// Both dates are required and inclusive, and `dateFrom` cannot be later than `dateTo`.
    /// All expenses in the range must use the same currency, otherwise the request is rejected.
    /// </remarks>
    /// <param name="dateFrom">First day of the range, e.g. `2026-10-01`.</param>
    /// <param name="dateTo">Last day of the range, e.g. `2026-10-31`.</param>
    [HttpGet("summary")]
    public async Task<ActionResult<BaseResponse<ExpenseSummary>>> GetSummary([FromQuery] DateOnly? dateFrom, [FromQuery] DateOnly? dateTo)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.GetSummary(userId, dateFrom, dateTo);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Records a new expense.
    /// </summary>
    /// <remarks>
    /// The amount must be greater than zero, the currency must be three uppercase letters (e.g. `USD`),
    /// the date can't be more than one day in the future, and the category must exist and be active.
    /// All invalid fields are returned together.
    /// </remarks>
    /// <param name="request">Expense details.</param>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> CreateExpense(ExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.Create(userId, request);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Returns a single expense by its id.
    /// </summary>
    /// <remarks>
    /// Returns not found if the expense belongs to another user.
    /// </remarks>
    /// <param name="id">Expense id.</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<ExpenseResponse>>> GetExpenseById(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.GetById(userId, id);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Permanently deletes an expense.
    /// </summary>
    /// <remarks>
    /// Returns not found if the expense belongs to another user.
    /// </remarks>
    /// <param name="id">Expense id.</param>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await service.Delete(userId, id);

        return NoContent();
    }

    /// <summary>
    /// Updates an existing expense.
    /// </summary>
    /// <remarks>
    /// Uses the same rules as creating an expense, except that the expense can stay in its current category
    /// even if that category has since been deactivated. If `name` is left out, the current name is kept.
    /// </remarks>
    /// <param name="id">Expense id.</param>
    /// <param name="request">New expense details.</param>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateExpense(int id, ExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.Update(userId, id, request);

        return StatusCode(response.Status, response);
    }
}
