using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using CoffeeMachine.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoffeeMachine.Tests.Unit;

/// <summary>
/// The weather service tests.
/// </summary>
public sealed class WeatherAwareCoffeeMachineServiceTests
{
    private static CoffeeMachineService Build(
        double? temperature,
        bool aprilFools = false,
        int counterStartAt = 0,
        double hotThreshold = 30.0)
    {
        var clock = new FakeDateTimeProvider();
        var counter = new FakeBrewCounterService(counterStartAt);

        if (aprilFools) clock.SetAprilFools();
        else clock.SetNormalDay();

        var weatherMock = new Mock<IWeatherService>();
        weatherMock
            .Setup(w => w.GetCurrentTemperatureCelsiusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperature);

        var opts = Options.Create(new WeatherOptions { HotThreshold = hotThreshold });

        return new CoffeeMachineService(counter, clock, weatherMock.Object, opts);
    }

    [Fact]
    public async Task Brew_TemperatureAbove30_ReturnsIcedMessage()
    {
        var svc = Build(temperature: 35.0);

        var result = await svc.Brew();

        result.Response!.Message.Should().Be("Your refreshing iced coffee is ready");
    }

    [Fact]
    public async Task Brew_TemperatureExactly30_ReturnsHotMessage()
    {
        // Boundary: > 30, not ≥ 30
        var svc = Build(temperature: 30.0);

        var result = await svc.Brew();

        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Brew_TemperatureBelow30_ReturnsHotMessage()
    {
        var svc = Build(temperature: 22.5);

        var result = await svc.Brew();

        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Brew_WeatherServiceUnavailable_FallsBackToHotMessage()
    {
        // null means the weather service failed; we should not propagate the failure
        var svc = Build(temperature: null);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Ready);
        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Brew_April1st_Returns418_WeatherIsNotChecked()
    {
        var weatherMock = new Mock<IWeatherService>();
        var clock = new FakeDateTimeProvider();
        clock.SetAprilFools();
        var counter = new FakeBrewCounterService();
        var opts = Options.Create(new WeatherOptions());

        var svc = new CoffeeMachineService(counter, clock, weatherMock.Object, opts);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Teapot);

        // Weather should NOT be called on April 1st — no point checking it
        weatherMock.Verify(
            w => w.GetCurrentTemperatureCelsiusAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
