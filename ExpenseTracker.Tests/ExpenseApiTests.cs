using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseTracker.Dto.expense;
using ExpenseTracker.Dto.user;
using ExpenseTracker.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.Tests;

public class ExpenseApiTests : IClassFixture<ExpenseTrackerApiFactory>
{
    private readonly ExpenseTrackerApiFactory _factory;

    public ExpenseApiTests(ExpenseTrackerApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateExpense_ThenGetExpenses_ReturnsCreatedExpense()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/expenses", CreateRequest(amount: 2500));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/expenses");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var body = await getResponse.Content.ReadFromJsonAsync<BaseResponse<List<ExpenseResponse>>>();
        var expense = Assert.Single(body!.Details!);
        Assert.Equal(2500, expense.Amount);
        Assert.Equal("USD", expense.Currency);
        Assert.Equal("Food", expense.CategoryName);
        Assert.Equal(new DateOnly(2026, 7, 10), expense.ExpenseDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateExpense_WithZeroOrNegativeAmount_ReturnsValidationError(decimal amount)
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/expenses", CreateRequest(amount));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse<Dictionary<string, string[]>>>();
        Assert.Equal(StatusCodes.Status400BadRequest, body!.Status);
        Assert.Equal(ErrorCodes.ValidationError, body.Error);
        Assert.Equal(["Amount must be greater than zero."], body.Details!["amount"]);
    }

    [Fact]
    public async Task GetExpenses_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/expenses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
        Assert.Equal(ErrorCodes.Unauthorized, body!.Error);
    }
    
    private async Task<HttpClient> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        var userName = $"user_{Guid.NewGuid():N}";
        const string password = "test_password";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new UserRegisterRequest
        {
            UserName = userName,
            Password = password,
            Email = $"{userName}@test.com"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new UserLoginRequest(userName, password));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<BaseResponse<LoginResponse>>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.Details!.Token.AccessToken);

        return client;
    }

    private static ExpenseRequest CreateRequest(decimal amount)
    {
        return new ExpenseRequest
        {
            Name = "lunch",
            Description = "test_description",
            Amount = amount,
            Currency = "USD",
            ExpenseDate = new DateTime(2026, 7, 10),
            CategoryId = 1
        };
    }
}
