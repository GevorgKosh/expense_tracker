using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Dto.expense;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Service;

public class ExpenseService(ExpenseTrackerDbContext context, IMapper mapper): IExpenseService
{
    public async Task<BaseResponse<ExpenseResponse>> GetById(int userId, int id)
    {
        var expense = await context.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
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

    public async Task<BaseResponse<ICollection<ExpenseResponse>>> GetExpenses(int userId, ExpenseFilter filter)
    {
        IQueryable<Expense> query = context.Expenses
            .AsNoTracking()
            .Include(expense => expense.Category)
            .Where(expense => expense.UserId == userId);

        if (filter.CategoryId is not null)
        {
            query = query.Where(expense => expense.CategoryId == filter.CategoryId);
        }

        if (filter.Currency is not null)
        {
            var currency = filter.Currency.ToUpperInvariant();
            query = query.Where(expense => expense.Currency == currency);
        }

        if (filter.MinimumAmount is not null)
        {
            query = query.Where(expense => expense.Amount >= filter.MinimumAmount);
        }

        if (filter.MaximumAmount is not null)
        {
            query = query.Where(expense => expense.Amount <= filter.MaximumAmount);
        }

        if (filter.DateFrom is not null)
        {
            var dateFrom = filter.DateFrom.Value.Date;
            query = query.Where(expense => expense.ExpenseDate >= dateFrom);
        }

        if (filter.DateTo is not null)
        {
            // Exclusive upper bound so expenses later on the last day are included
            var dateToExclusive = filter.DateTo.Value.Date.AddDays(1);
            query = query.Where(expense => expense.ExpenseDate < dateToExclusive);
        }

        var expenses = await query.OrderByDescending(expense => expense.ExpenseDate).ToListAsync();

        return new BaseResponse<ICollection<ExpenseResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expenses retrieved successfully",
            Details = mapper.Map<ICollection<ExpenseResponse>>(expenses)
        };
    }

    public async Task<BaseResponse<ExpenseSummary>> GetSummary(int userId, DateOnly? dateFrom, DateOnly? dateTo)
    {
        if (dateFrom is null || dateTo is null)
        {
            return InvalidSummaryRequest("Both dateFrom and dateTo are required");
        }

        if (dateFrom > dateTo)
        {
            return InvalidSummaryRequest("dateFrom cannot be later than dateTo");
        }

        var from = dateFrom.Value.ToDateTime(TimeOnly.MinValue);
        // Exclusive upper bound so expenses later on the last day are included
        var toExclusive = dateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var groups = await context.Expenses
            .AsNoTracking()
            .Where(expense => expense.UserId == userId && expense.ExpenseDate >= from && expense.ExpenseDate < toExclusive)
            .GroupBy(expense => new { expense.CategoryId, CategoryName = expense.Category.Name, expense.Currency })
            .Select(group => new
            {
                group.Key.CategoryId,
                group.Key.CategoryName,
                group.Key.Currency,
                Count = group.Count(),
                TotalAmount = group.Sum(expense => expense.Amount)
            })
            .ToListAsync();

        var currencies = groups.Select(group => group.Currency).Distinct().ToList();
        if (currencies.Count > 1)
        {
            return InvalidSummaryRequest(
                $"All expenses in the selected period must have the same currency, found: {string.Join(", ", currencies.Order())}");
        }

        return new BaseResponse<ExpenseSummary>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expense summary retrieved successfully",
            Details = new ExpenseSummary
            {
                DateFrom = dateFrom.Value,
                DateTo = dateTo.Value,
                ExpenseCount = groups.Sum(group => group.Count),
                TotalAmount = (decimal)groups.Sum(group => group.TotalAmount),
                Currency = currencies.SingleOrDefault(),
                Categories = groups
                    .OrderBy(group => group.CategoryId)
                    .Select(group => new CategorySummary
                    {
                        CategoryId = group.CategoryId,
                        CategoryName = group.CategoryName,
                        TotalAmount = (decimal)group.TotalAmount
                    })
                    .ToList()
            }
        };
    }

    public async Task<BaseResponse<bool>> Create(int userId, ExpenseRequest request)
    {
        var invalidFields = ValidateFields(request);
        if (invalidFields is not null)
        {
            return invalidFields;
        }

        var invalidCategory = await ValidateCategory(request.CategoryId, currentCategoryId: null);
        if (invalidCategory is not null)
        {
            return invalidCategory;
        }

        var expense = mapper.Map<Expense>(request);
        expense.Name = request.Name ?? string.Empty;
        expense.UserId = userId;
        expense.Currency = request.Currency.ToUpperInvariant();
        expense.CreatedDateTime = DateTime.UtcNow;

        context.Expenses.Add(expense);
        await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status201Created,
            Message = "Expense created successfully",
            Details = true
        };
    }

    public async Task<BaseResponse<bool>> Update(int userId, int id, ExpenseRequest request)
    {
        var invalidFields = ValidateFields(request);
        if (invalidFields is not null)
        {
            return invalidFields;
        }

        var existing = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (existing is null)
        {
            return NotFound<bool>(id);
        }

        var invalidCategory = await ValidateCategory(request.CategoryId, existing.CategoryId);
        if (invalidCategory is not null)
        {
            return invalidCategory;
        }

        existing.Name = request.Name ?? existing.Name;
        existing.Description = request.Description;
        existing.Amount = request.Amount;
        existing.Currency = request.Currency.ToUpperInvariant();
        existing.ExpenseDate = request.ExpenseDate;
        existing.CategoryId = request.CategoryId;

        await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status200OK,
            Message = "Expense updated successfully",
            Details = true
        };
    }

    public async Task<BaseResponse<bool>> Delete(int userId, int id)
    {
        var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
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

    private static BaseResponse<bool>? ValidateFields(ExpenseRequest request)
    {
        if (request.Amount <= 0)
        {
            return InvalidAmount();
        }

        if (request.Currency.Length != 3 || request.Currency.Any(c => !char.IsAsciiLetter(c)))
        {
            return InvalidCurrency();
        }

        if (request.ExpenseDate.Date > DateTime.UtcNow.Date.AddDays(1))
        {
            return InvalidDate();
        }

        return null;
    }

    // An expense may stay in its current category even if that category was deactivated later
    private async Task<BaseResponse<bool>?> ValidateCategory(int categoryId, int? currentCategoryId)
    {
        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is null)
        {
            return InvalidCategory();
        }

        if (!category.IsActive && category.Id != currentCategoryId)
        {
            return InactiveCategory();
        }

        return null;
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

    private static BaseResponse<ExpenseSummary> InvalidSummaryRequest(string message)
    {
        return new BaseResponse<ExpenseSummary>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = "Bad Request",
            Message = message
        };
    }

    private static BaseResponse<bool> InvalidAmount()
    {
        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = "Bad Request",
            Message = "Expense amount must be greater than zero"
        };
    }
    
    private static BaseResponse<bool> InvalidCurrency()
    {
        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = "Bad Request",
            Message = "Currency must contain exactly three letters"
        };
    }
    
    private static BaseResponse<bool> InvalidCategory()
    {
        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = "Bad Request",
            Message = "The selected category does not exist"
        };
    }
    
    private static BaseResponse<bool> InactiveCategory()
    {
        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = "Bad Request",
            Message = "The selected category is inactive"
        };
    }
    
    private static BaseResponse<bool> InvalidDate()
    {
        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = "Bad Request",
            Message = "The selected date cannot be more than one day from current day"
        };
    }
}
