using System.Security.Claims;
using ExpenseTracker.Dto;
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
[Produces("application/json")]
// Every action requires a token, so every action can return 401
[ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
public class ExpenseController(IExpenseService service): ControllerBase
{
    /// <summary>
    /// Returns one page of the user's expenses, optionally filtered and sorted.
    /// </summary>
    /// <remarks>
    /// All filters are optional and can be combined. `dateFrom` and `dateTo` are inclusive,
    /// `currency` is case-insensitive and the amount range is inclusive on both ends.
    ///
    /// Results are paged: `page` starts at 1 and `pageSize` is 1 to 100 (default 20). The response contains
    /// the page's `items` plus `totalCount` and `totalPages` for all matching expenses.
    ///
    /// Sorting: `sortBy` is `expenseDate` (default), `amount` or `createdAt`; `sortDirection` is `desc` (default) or `asc`.
    ///
    /// Example: `GET /api/expenses?dateFrom=2026-10-01&amp;dateTo=2026-10-31&amp;categoryId=1&amp;sortBy=amount&amp;sortDirection=asc&amp;page=1&amp;pageSize=10`
    /// </remarks>
    /// <param name="filter">Filters, paging and sorting.</param>
    /// <response code="200">The requested page (an empty `items` list if nothing matches).</response>
    /// <response code="400">A filter value has the wrong format, or page, page size, sort field or sort direction is invalid.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<PagedResponse<ExpenseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BaseResponse<PagedResponse<ExpenseResponse>>>> GetExpenseList([FromQuery] ExpenseFilter filter)
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
    /// <response code="200">The summary (zero totals if there are no expenses in the range).</response>
    /// <response code="400">A date is missing, `dateFrom` is after `dateTo`, or the expenses use more than one currency.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(BaseResponse<ExpenseSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
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
    /// <response code="201">The expense was created.</response>
    /// <response code="400">One or more fields are invalid, or the category doesn't exist or is inactive.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
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
    /// <response code="200">The expense.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="404">The expense doesn't exist or belongs to another user.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<ExpenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
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
    /// <response code="204">The expense was deleted.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="404">The expense doesn't exist or belongs to another user.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
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
    /// <response code="200">The expense was updated.</response>
    /// <response code="400">One or more fields are invalid, or the new category doesn't exist or is inactive.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="404">The expense doesn't exist or belongs to another user.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateExpense(int id, ExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await service.Update(userId, id, request);

        return StatusCode(response.Status, response);
    }
}
