using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Service;

public class ExpenseService(ExpenseTrackerDbContext context, IMapper mapper): IExpenseService
{
    public async Task<BaseResponse<ICollection<ExpenseResponse>>> GetExpenses()
    {
        var expenses = await context.Expenses.ToListAsync();

        return new BaseResponse<ICollection<ExpenseResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expenses retrieved successfully",
            Details = mapper.Map<ICollection<ExpenseResponse>>(expenses)
        };
    }

    public async Task<BaseResponse<ExpenseResponse>> GetById(int id)
    {
        var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null)
        {
            return NotFound<ExpenseResponse>(id);
        }

        return new BaseResponse<ExpenseResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expense retrieved successfully",
            Details = mapper.Map<ExpenseResponse>(expense)
        };
    }

    public async Task<BaseResponse<ICollection<ExpenseResponse>>> GetFiltered(ExpenseFilter filter)
    {
        IQueryable<Expense> query = context.Expenses.Include(expense => expense.Category);

        if (filter.UserId is not null)
        {
            query = query.Where(expense => expense.UserId == filter.UserId);
        }

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

        var expenses = await query.ToListAsync();

        return new BaseResponse<ICollection<ExpenseResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expenses retrieved successfully",
            Details = mapper.Map<ICollection<ExpenseResponse>>(expenses)
        };
    }

    public async Task<BaseResponse<bool>> Create(ExpenseRequest request)
    {
        var expense = mapper.Map<Expense>(request);
        expense.Date = DateTime.UtcNow;

        context.Expenses.Add(expense);
        var result = await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status201Created,
            Message = "Expense created successfully",
            Details = result > 0
        };
    }

    public async Task<BaseResponse<bool>> Update(int id, ExpenseRequest request)
    {
        var existing = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            return NotFound<bool>(id);
        }

        var expense = mapper.Map<Expense>(request);
        existing.Name = expense.Name;
        existing.Description = expense.Description;
        existing.Amount = expense.Amount;
        existing.CategoryId = expense.CategoryId;

        await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expense updated successfully",
            Details = true
        };
    }

    public async Task<BaseResponse<bool>> Delete(int id)
    {
        var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null)
        {
            return NotFound<bool>(id);
        }

        context.Expenses.Remove(expense);
        var result = await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expense deleted successfully",
            Details = result > 0
        };
    }

    private static BaseResponse<T> NotFound<T>(int id)
    {
        return new BaseResponse<T>
        {
            Status = StatusCodes.Status404NotFound,
            Error = "Not Found",
            Message = $"Expense with id {id} not found"
        };
    }
}
