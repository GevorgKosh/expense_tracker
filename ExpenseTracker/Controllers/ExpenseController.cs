using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/expense")]
public class ExpenseController(ExpenseDbContext context, IMapper mapper): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ICollection<ExpenseResponse>>> GetExpenseList()
    {
        var expenses = await context.Expenses.Select(expense =>
            mapper.Map<ExpenseResponse>(expense)
            ).ToListAsync();
        
        return Ok(expenses);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> CreateExpense(ExpenseRequest request)
    {
        var expense = mapper.Map<Expense>(request);
        expense.Date = DateTime.UtcNow;
        
        context.Expenses.Add(expense);
        var result = await context.SaveChangesAsync();
        return Ok(result > 0);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetExpenseById(int id)
    {
        var result = await context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id);
        if (result == null)
        {
            throw new Exception("No such Expense");
        }
        return Ok(mapper.Map<ExpenseResponse>(result));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> DeleteExpense(int id)
    {
        var expense = await context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id);
        if (expense == null)
        {
            throw new Exception("No such Expense");
        }
        context.Expenses.Remove(expense);
        var result = await context.SaveChangesAsync();
        return Ok(result > 0);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> UpdateExpense(int id, ExpenseRequest request)
    {
        var result = await context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id);
        if (result == null)
        {
            throw new Exception("No such Expense is found");
        }

        var expense = mapper.Map<Expense>(request);
        result.Date = expense.Date;
        result.Name = expense.Name;
        result.Description = expense.Description;
        result.Category = expense.Category;
        result.CategoryId = expense.CategoryId;

        await context.SaveChangesAsync();
        return Ok(true);
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<ExpenseResponse>>> GetFilteredExpenses([FromQuery] ExpenseFilter filter)
    {
        IQueryable<Expense> query = context.Expenses.Include(expense => expense.Category);

        if (filter.CategoryId is not null) 
        {
            query = query.Where(expense => expense.CategoryId == filter.CategoryId);
        }

        if (filter.Currency is not null)
        {
            query = query.Where(expense => expense.Currency == filter.Currency);
        }
        
        if (filter.MinAmount is not null)
        {
            query = query.Where(expense => expense.Amount >= filter.MinAmount);
        }
        
        if (filter.MaxAmount is not null)
        {
            query = query.Where(expense => expense.Amount <= filter.MaxAmount);
        }
        
        if (filter.DateFrom is not null)
        {
            query = query.Where(expense => expense.Date >= filter.DateFrom);
        }

        if (filter.DateTo is not null)
        {
            query = query.Where(expense => expense.Date <= filter.DateTo);
        }
        
        var response = await query.ToListAsync();
        var expenses = mapper.Map<ICollection<ExpenseResponse>>(response);

        return Ok(expenses);
    } 
}