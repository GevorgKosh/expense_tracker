using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ExpenseTracker.Tests;

public class HealthCheckApiTests : IClassFixture<ExpenseTrackerApiFactory>
{
    private readonly ExpenseTrackerApiFactory _factory;

    public HealthCheckApiTests(ExpenseTrackerApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_WithoutToken_ReturnsHealthyWithDatabaseCheck()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Healthy", body.GetProperty("status").GetString());
        var check = Assert.Single(body.GetProperty("checks").EnumerateArray());
        Assert.Equal("database", check.GetProperty("name").GetString());
        Assert.Equal("Healthy", check.GetProperty("status").GetString());
    }
}
