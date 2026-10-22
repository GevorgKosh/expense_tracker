using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto.expense;
using ExpenseTracker.Entity;
using ExpenseTracker.Exceptions;
using ExpenseTracker.Mapping;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExpenseTracker.Tests;

public class ExpenseServiceTests
{
    [Theory]
    [InlineData(1)]
    public async Task Create_WithInactiveCategory_IsRejected(int categoryId)
    {
        await using var context = CreateContext();
        var category = await context.Categories.SingleAsync(c => c.Id == categoryId);
        category.IsActive = false;
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var request = new ExpenseRequest
        {
            Name = "test_name",
            Description = "test_description",
            Amount = 1000,
            Currency = "USD",
            CategoryId = categoryId
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.Create(1, request));

        Assert.Equal(["The selected category is inactive."], exception.Errors["categoryId"]);
        Assert.False(await context.Expenses.AnyAsync());
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task Create_WithZeroOrNegativeAmount_IsRejected(decimal amount)
    {
        await using var context = CreateContext();
        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var request = new ExpenseRequest
        {
            Name = "test_name",
            Description = "test_description",
            Amount = amount,
            Currency = "USD",
            CategoryId = 1
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.Create(1, request));

        Assert.Equal(["Amount must be greater than zero."], exception.Errors["amount"]);
        Assert.Single(exception.Errors);
        Assert.False(await context.Expenses.AnyAsync());
    }

    [Theory]
    [InlineData("2026-03-01", "2026-03-31", new[] { 2, 3 })]
    [InlineData("2026-03-15", null, new[] { 3, 4 })]
    [InlineData(null, "2026-03-01", new[] { 1, 2 })]
    [InlineData("2026-05-01", "2026-05-31", new int[0])]
    public async Task GetExpenses_ByDate_ReturnsExpensesWithinRange(string? dateFrom, string? dateTo, int[] expectedIds)
    {
        await using var context = CreateContext();
        context.Expenses.AddRange(
            CreateExpense(1, new DateTime(2026, 2, 28)),
            CreateExpense(2, new DateTime(2026, 3, 1)),
            CreateExpense(3, new DateTime(2026, 3, 15)),
            CreateExpense(4, new DateTime(2026, 4, 10)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var filter = new ExpenseFilter
        {
            DateFrom = dateFrom is null ? null : DateTime.Parse(dateFrom),
            DateTo = dateTo is null ? null : DateTime.Parse(dateTo)
        };

        var response = await service.GetExpenses(1, filter);

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        Assert.Equal(expectedIds, response.Details.Select(e => e.Id).Order());
    }

    [Fact]
    public async Task GetExpenses_WithMultipleFilters_AppliesAllOfThem()
    {
        await using var context = CreateContext();
        var matching = CreateExpense(1, new DateTime(2026, 7, 10));
        matching.Currency = "AMD";
        matching.Amount = 8500;
        var wrongCategory = CreateExpense(2, new DateTime(2026, 7, 10));
        wrongCategory.Currency = "AMD";
        wrongCategory.Amount = 8500;
        wrongCategory.CategoryId = 2;
        var wrongCurrency = CreateExpense(3, new DateTime(2026, 7, 10));
        wrongCurrency.Amount = 8500;
        var tooSmall = CreateExpense(4, new DateTime(2026, 7, 10));
        tooSmall.Currency = "AMD";
        tooSmall.Amount = 500;
        var outsideDates = CreateExpense(5, new DateTime(2026, 8, 1));
        outsideDates.Currency = "AMD";
        outsideDates.Amount = 8500;
        context.Expenses.AddRange(matching, wrongCategory, wrongCurrency, tooSmall, outsideDates);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var filter = new ExpenseFilter
        {
            CategoryId = 1,
            Currency = "amd",
            MinimumAmount = 1000,
            MaximumAmount = 10000,
            DateFrom = new DateTime(2026, 7, 1),
            DateTo = new DateTime(2026, 7, 31)
        };

        var response = await service.GetExpenses(1, filter);

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        var expense = Assert.Single(response.Details);
        Assert.Equal(1, expense.Id);
        Assert.Equal("Food", expense.CategoryName);
    }

    [Fact]
    public async Task GetExpenses_ReturnsOnlyCurrentUsersExpenses()
    {
        await using var context = CreateContext();
        var otherUsersExpense = CreateExpense(2, new DateTime(2026, 7, 10));
        otherUsersExpense.UserId = 2;
        context.Expenses.AddRange(CreateExpense(1, new DateTime(2026, 7, 10)), otherUsersExpense);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var response = await service.GetExpenses(1, new ExpenseFilter());

        Assert.Equal(1, Assert.Single(response.Details).Id);
    }

    [Fact]
    public async Task GetExpenses_ByDateTo_IncludesExpensesLaterOnTheLastDay()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(1, new DateTime(2026, 7, 31, 18, 0, 0)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var response = await service.GetExpenses(1, new ExpenseFilter { DateTo = new DateTime(2026, 7, 31) });

        Assert.Single(response.Details);
    }

    [Fact]
    public async Task GetById_ReturnsCategoryNameAndDates()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var response = await service.GetById(1, 10);

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        Assert.Equal("Food", response.Details.CategoryName);
        Assert.Equal(new DateOnly(2026, 7, 17), response.Details.ExpenseDate);
        Assert.Equal(new DateTime(2026, 3, 15), response.Details.CreatedAt);
    }

    [Fact]
    public async Task GetById_ForAnotherUsersExpense_ReturnsNotFound()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetById(2, 10));
    }

    [Fact]
    public async Task Update_UpdatesAllFields()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var request = CreateUpdateRequest(categoryId: 2);

        var response = await service.Update(1, 10, request);

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        var expense = await context.Expenses.AsNoTracking().SingleAsync(e => e.Id == 10);
        Assert.Equal("updated_description", expense.Description);
        Assert.Equal(8500, expense.Amount);
        Assert.Equal("AMD", expense.Currency);
        Assert.Equal(new DateTime(2026, 7, 18), expense.ExpenseDate);
        Assert.Equal(2, expense.CategoryId);
        Assert.Equal("expense_10", expense.Name);
    }

    [Fact]
    public async Task Update_ForAnotherUsersExpense_ReturnsNotFound()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.Update(2, 10, CreateUpdateRequest(categoryId: 1)));
        var expense = await context.Expenses.AsNoTracking().SingleAsync(e => e.Id == 10);
        Assert.Equal("USD", expense.Currency);
    }

    [Theory]
    [InlineData("XX1", 1, "currency")]
    [InlineData("AMD", 999, "categoryId")]
    public async Task Update_WithInvalidCurrencyOrCategory_IsRejected(string currency, int categoryId, string invalidField)
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var request = CreateUpdateRequest(categoryId);
        request.Currency = currency;

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.Update(1, 10, request));

        Assert.Equal([invalidField], exception.Errors.Keys);
    }

    [Fact]
    public async Task Update_WithFutureDate_IsRejected()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);
        var request = CreateUpdateRequest(categoryId: 1);
        request.ExpenseDate = DateTime.UtcNow.Date.AddDays(2);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.Update(1, 10, request));

        Assert.True(exception.Errors.ContainsKey("expenseDate"));
    }

    [Fact]
    public async Task Update_WithInactiveCategory_IsAllowedForCurrentCategory()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await DeactivateCategory(context, 1);

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var response = await service.Update(1, 10, CreateUpdateRequest(categoryId: 1));

        Assert.Equal(StatusCodes.Status200OK, response.Status);
    }

    [Fact]
    public async Task Update_WithInactiveCategory_IsRejectedForAnotherCategory()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await DeactivateCategory(context, 2);

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.Update(1, 10, CreateUpdateRequest(categoryId: 2)));

        Assert.True(exception.Errors.ContainsKey("categoryId"));
    }

    private static async Task DeactivateCategory(ExpenseTrackerDbContext context, int categoryId)
    {
        var category = await context.Categories.SingleAsync(c => c.Id == categoryId);
        category.IsActive = false;
        await context.SaveChangesAsync();
    }

    private static ExpenseRequest CreateUpdateRequest(int categoryId)
    {
        return new ExpenseRequest
        {
            Description = "updated_description",
            Amount = 8500,
            Currency = "AMD",
            ExpenseDate = new DateTime(2026, 7, 18),
            CategoryId = categoryId
        };
    }

    [Fact]
    public async Task Delete_RemovesExpense()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        await service.Delete(1, 10);

        Assert.False(await context.Expenses.AnyAsync(e => e.Id == 10));
    }

    [Fact]
    public async Task Delete_ForAnotherUsersExpense_ThrowsNotFound()
    {
        await using var context = CreateContext();
        context.Expenses.Add(CreateExpense(10, new DateTime(2026, 7, 17)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => service.Delete(2, 10));

        Assert.True(await context.Expenses.AnyAsync(e => e.Id == 10));
    }

    [Fact]
    public async Task GetSummary_ReturnsTotalsGroupedByCategory()
    {
        await using var context = CreateContext();
        var lunch = CreateExpense(1, new DateTime(2026, 7, 1));
        lunch.Amount = 15000;
        var dinner = CreateExpense(2, new DateTime(2026, 7, 31, 20, 0, 0));
        dinner.Amount = 20000;
        var taxi = CreateExpense(3, new DateTime(2026, 7, 15));
        taxi.Amount = 40000;
        taxi.CategoryId = 2;
        var outsidePeriod = CreateExpense(4, new DateTime(2026, 8, 1));
        var otherUser = CreateExpense(5, new DateTime(2026, 7, 15));
        otherUser.UserId = 2;
        otherUser.Currency = "EUR";
        context.Expenses.AddRange(lunch, dinner, taxi, outsidePeriod, otherUser);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var response = await service.GetSummary(1, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31));

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        var summary = response.Details;
        Assert.Equal(new DateOnly(2026, 7, 1), summary.DateFrom);
        Assert.Equal(new DateOnly(2026, 7, 31), summary.DateTo);
        Assert.Equal(3, summary.ExpenseCount);
        Assert.Equal(75000, summary.TotalAmount);
        Assert.Equal("USD", summary.Currency);
        Assert.Collection(summary.Categories,
            food =>
            {
                Assert.Equal(1, food.CategoryId);
                Assert.Equal("Food", food.CategoryName);
                Assert.Equal(35000, food.TotalAmount);
            },
            transportation =>
            {
                Assert.Equal(2, transportation.CategoryId);
                Assert.Equal("Transportation", transportation.CategoryName);
                Assert.Equal(40000, transportation.TotalAmount);
            });
    }

    [Fact]
    public async Task GetSummary_WithMixedCurrencies_IsRejected()
    {
        await using var context = CreateContext();
        var amd = CreateExpense(1, new DateTime(2026, 7, 10));
        amd.Currency = "AMD";
        context.Expenses.AddRange(amd, CreateExpense(2, new DateTime(2026, 7, 11)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetSummary(1, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31)));

        Assert.Contains("AMD, USD", exception.Message);
    }

    [Theory]
    [InlineData(null, "2026-07-31", "dateFrom")]
    [InlineData("2026-07-01", null, "dateTo")]
    [InlineData("2026-07-31", "2026-07-01", "dateFrom")]
    public async Task GetSummary_WithMissingOrReversedDates_IsRejected(string? dateFrom, string? dateTo, string invalidField)
    {
        await using var context = CreateContext();
        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.GetSummary(
            1,
            dateFrom is null ? null : DateOnly.Parse(dateFrom),
            dateTo is null ? null : DateOnly.Parse(dateTo)));

        Assert.Equal([invalidField], exception.Errors.Keys);
    }

    [Fact]
    public async Task GetSummary_WithNoExpenses_ReturnsEmptySummary()
    {
        await using var context = CreateContext();
        var service = new ExpenseService(context, CreateMapper(), NullLogger<ExpenseService>.Instance);

        var response = await service.GetSummary(1, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31));

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        Assert.Equal(0, response.Details.ExpenseCount);
        Assert.Equal(0, response.Details.TotalAmount);
        Assert.Null(response.Details.Currency);
        Assert.Empty(response.Details.Categories);
    }

    private static Expense CreateExpense(int id, DateTime expenseDate)
    {
        return new Expense
        {
            Id = id,
            Name = $"expense_{id}",
            Description = "test_description",
            Amount = 100,
            Currency = "USD",
            CategoryId = 1,
            UserId = 1,
            ExpenseDate = expenseDate,
            // Same for every expense, so only ExpenseDate can decide the result
            CreatedDateTime = new DateTime(2026, 3, 15)
        };
    }

    private static ExpenseTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ExpenseTrackerDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    private static IMapper CreateMapper()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();
    }
}