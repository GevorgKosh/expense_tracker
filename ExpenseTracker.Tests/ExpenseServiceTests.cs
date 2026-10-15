using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Mapping;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        var service = new ExpenseService(context, CreateMapper());
        var request = new ExpenseRequest
        {
            Name = "test_name",
            Description = "test_description",
            Amount = 1000.0,
            Currency = "USD",
            CategoryId = categoryId
        };

        var response = await service.Create(request);

        Assert.Equal(StatusCodes.Status400BadRequest, response.Status);
        Assert.Equal("The selected category is inactive", response.Message);
        Assert.False(response.Details);
        Assert.False(await context.Expenses.AnyAsync());
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task Create_WithZeroOrNegativeAmount_IsRejected(double amount)
    {
        await using var context = CreateContext();
        var service = new ExpenseService(context, CreateMapper());
        var request = new ExpenseRequest
        {
            Name = "test_name",
            Description = "test_description",
            Amount = amount,
            CategoryId = 1
        };

        var response = await service.Create(request);

        Assert.Equal(StatusCodes.Status400BadRequest, response.Status);
        Assert.False(response.Details);
        Assert.False(await context.Expenses.AnyAsync());
    }

    [Theory]
    [InlineData("2026-03-01", "2026-03-31", new[] { 2, 3 })]
    [InlineData("2026-03-15", null, new[] { 3, 4 })]
    [InlineData(null, "2026-03-01", new[] { 1, 2 })]
    [InlineData("2026-05-01", "2026-05-31", new int[0])]
    public async Task GetFiltered_ByDate_ReturnsExpensesWithinRange(string? dateFrom, string? dateTo, int[] expectedIds)
    {
        await using var context = CreateContext();
        context.Expenses.AddRange(
            CreateExpense(1, new DateTime(2026, 2, 28)),
            CreateExpense(2, new DateTime(2026, 3, 1)),
            CreateExpense(3, new DateTime(2026, 3, 15)),
            CreateExpense(4, new DateTime(2026, 4, 10)));
        await context.SaveChangesAsync();

        var service = new ExpenseService(context, CreateMapper());
        var filter = new ExpenseFilter
        {
            DateFrom = dateFrom is null ? null : DateTime.Parse(dateFrom),
            DateTo = dateTo is null ? null : DateTime.Parse(dateTo)
        };

        var response = await service.GetFiltered(filter);

        Assert.Equal(StatusCodes.Status200OK, response.Status);
        Assert.Equal(expectedIds, response.Details.Select(e => e.Id).Order());
    }

    private static Expense CreateExpense(int id, DateTime expenseDate)
    {
        return new Expense
        {
            Id = id,
            Name = $"expense_{id}",
            Description = "test_description",
            Amount = 100.0,
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