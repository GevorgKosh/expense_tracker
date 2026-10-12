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