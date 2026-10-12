using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Mapping;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Tests;

public class CategoryServiceTests
{
    [Theory]
    [InlineData("Food")]
    [InlineData("food")]
    [InlineData("  FOOD ")]
    public async Task Create_WithDuplicateName_IsRejected(string name)
    {
        await using var context = CreateContext();
        var service = new CategoryService(context, CreateMapper());
        var request = new CategoryRequest { Type = CategoryType.Food, Name = name, Description = "test" };

        var response = await service.Create(request);

        Assert.Equal(StatusCodes.Status409Conflict, response.Status);
        Assert.False(response.Details);
        Assert.Equal(1, await context.Categories.CountAsync(c => c.Name.Trim().ToLower() == "food"));
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
