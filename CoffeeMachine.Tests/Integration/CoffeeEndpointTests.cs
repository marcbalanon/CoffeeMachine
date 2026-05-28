using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoffeeMachine.Tests.Integration;

/// <summary>
/// Integration tests that spin up the real ASP.NET Core pipeline.
/// </summary>
public sealed class CoffeeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CoffeeEndpointTests(WebApplicationFactory<Program> factory)
        => _factory = factory;

    /// <summary>
    /// Creates an HttpClient with the clock and counter replaced.
    /// </summary>
    private HttpClient CreateClient(
        bool aprilFools = false,
        int counterStartAt = 0)
    {
        var clock = new FakeDateTimeProvider();
        var counter = new FakeBrewCounterService(counterStartAt);

        if (aprilFools) clock.SetAprilFools();
        else clock.SetNormalDay();

        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDateTimeProvider>(clock);
                services.AddSingleton<IBrewCounterService>(counter);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GET_BrewCoffee_NormalDay_Returns200()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_BrewCoffee_NormalDay_BodyHasExpectedShape()
    {
        var client = CreateClient();

        var body = await client.GetFromJsonAsync<BrewResponse>("/brew-coffee");

        body.Should().NotBeNull();
        body!.Message.Should().Be("Your piping hot coffee is ready");
        body.Prepared.Should().NotBe(default);
    }

    [Fact]
    public async Task GET_BrewCoffee_ContentType_IsApplicationJson()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/brew-coffee");

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }


    [Fact]
    public async Task GET_BrewCoffee_FifthCall_Returns503()
    {
        // Counter starts at 4
        var client = CreateClient(counterStartAt: 4);

        // This is 5th call
        var response = await client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GET_BrewCoffee_FifthCall_ReturnsEmptyBody()
    {
        var client = CreateClient(counterStartAt: 4);

        var response = await client.GetAsync("/brew-coffee");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_BrewCoffee_TenthCall_Returns503()
    {
        var client = CreateClient(counterStartAt: 9);

        var response = await client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GET_BrewCoffee_April1st_Returns418()
    {
        var client = CreateClient(aprilFools: true);

        var response = await client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be((HttpStatusCode)418);
    }

    [Fact]
    public async Task GET_BrewCoffee_April1st_ReturnsEmptyBody()
    {
        var client = CreateClient(aprilFools: true);

        var response = await client.GetAsync("/brew-coffee");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_BrewCoffee_April1st_5thCall_Returns418NotTeapot()
    {
        // April 1st check first.
        var client = CreateClient(aprilFools: true, counterStartAt: 4);

        var response = await client.GetAsync("/brew-coffee");

        response.StatusCode.Should().Be((HttpStatusCode)418);
    }

    [Fact]
    public async Task GET_UnknownRoute_Returns404()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/make-tea");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
