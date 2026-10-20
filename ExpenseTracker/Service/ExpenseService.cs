using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Dto.expense;
using ExpenseTracker.Exceptions;
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
            throw new NotFoundException($"Expense with id {id} not found");
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
        var errors = new Dictionary<string, string[]>();
        if (dateFrom is null)
        {
            errors["dateFrom"] = ["dateFrom is required."];
        }

        if (dateTo is null)
        {
            errors["dateTo"] = ["dateTo is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        if (dateFrom > dateTo)
        {
            throw new ValidationException("dateFrom", "dateFrom cannot be later than dateTo.");
        }

        var from = dateFrom!.Value.ToDateTime(TimeOnly.MinValue);
        var toExclusive = dateTo!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);

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
            throw new BadRequestException(
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
        ValidateFields(request);
        await ValidateCategory(request.CategoryId, currentCategoryId: null);

        var expense = mapper.Map<Expense>(request);
        expense.Name = request.Name ?? string.Empty;
        expense.UserId = userId;
        expense.Currency = request.Currency;
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
        ValidateFields(request);

        var existing = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (existing is null)
        {
            throw new NotFoundException($"Expense with id {id} not found");
        }

        await ValidateCategory(request.CategoryId, existing.CategoryId);

        existing.Name = request.Name ?? existing.Name;
        existing.Description = request.Description;
        existing.Amount = request.Amount;
        existing.Currency = request.Currency;
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

    public async Task Delete(int userId, int id)
    {
        var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (expense is null)
        {
            throw new NotFoundException($"Expense with id {id} not found");
        }

        context.Expenses.Remove(expense);
        await context.SaveChangesAsync();
    }

    // Collects every invalid field so the client gets all errors in one response
    private static void ValidateFields(ExpenseRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Amount <= 0)
        {
            errors["amount"] = ["Amount must be greater than zero."];
        }

        if (request.Currency.Length != 3 || request.Currency.Any(c => !char.IsAsciiLetterUpper(c)))
        {
            errors["currency"] = ["Currency must contain exactly three uppercase letters (e.g. AMD, USD, EUR)."];
        }

        if (request.ExpenseDate.Date > DateTime.UtcNow.Date.AddDays(1))
        {
            errors["expenseDate"] = ["The selected date cannot be more than one day from current day."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
    
    private async Task ValidateCategory(int categoryId, int? currentCategoryId)
    {
        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is null)
        {
            throw new ValidationException("categoryId", "The selected category does not exist.");
        }

        if (!category.IsActive && category.Id != currentCategoryId)
        {
            throw new ValidationException("categoryId", "The selected category is inactive.");
        }
    }
}
