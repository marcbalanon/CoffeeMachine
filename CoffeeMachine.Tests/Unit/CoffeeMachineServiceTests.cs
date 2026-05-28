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
/// CoffeeMachineService unit tests
/// </summary>
public sealed class CoffeeMachineServiceTests
{
    private static (CoffeeMachineService svc, FakeDateTimeProvider clock, FakeBrewCounterService counter, Mock<IWeatherService> weatherMock)
        Build(int counterStartAt = 0, bool aprilFools = false, double? temperature = 20.0)
    {
        var clock = new FakeDateTimeProvider();
        var counter = new FakeBrewCounterService(counterStartAt);
        var weatherMock = new Mock<IWeatherService>();
        var opts = Options.Create(new WeatherOptions { HotThreshold = 30.0 });

        weatherMock
            .Setup(w => w.GetCurrentTemperatureCelsiusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperature);

        var svc = new CoffeeMachineService(counter, clock, weatherMock.Object, opts);

        if (aprilFools) clock.SetAprilFools();
        else clock.SetNormalDay();

        return (svc, clock, counter, weatherMock);
    }

    [Fact]
    public async Task Brew_NormalDay_FirstCall_Returns200WithHotMessage()
    {
        var (svc, clock, _, _) = Build(temperature: 20.0);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Ready);
        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
        result.Response.Prepared.Should().Be(clock.UtcNow);
    }

    [Fact]
    public async Task Brew_NormalDay_PreparedTimestampMatchesClockValue()
    {
        var expectedTime = new DateTimeOffset(2024, 8, 20, 14, 30, 0, TimeSpan.Zero);
        var (svc, clock, _, _) = Build(temperature: 20.0);
        clock.UtcNow = expectedTime;

        var result = await svc.Brew();

        result.Response!.Prepared.Should().Be(expectedTime);
    }

    [Theory]
    [InlineData(4)]   // 5th call
    [InlineData(9)]   // 10th call
    [InlineData(14)]  // 15th call
    [InlineData(19)]  // 20th call
    public async Task Brew_EveryFifthCall_Returns503(int counterStart)
    {
        var (svc, _, _, _) = Build(counterStart);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.OutOfCoffee);
        result.Response.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]   // 1st call
    [InlineData(1)]   // 2nd call
    [InlineData(2)]   // 3rd call
    [InlineData(3)]   // 4th call
    public async Task Brew_NonFifthCall_Returns200(int counterStart)
    {
        var (svc, _, _, _) = Build(counterStart);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Ready);
    }

    [Fact]
    public async Task Brew_CounterAlwaysIncrements_EvenWhenOtherRulesApply()
    {
        var (svc, clock, counter, _) = Build(0, aprilFools: true);

        await svc.Brew();
        counter.Current.Should().Be(1);

        clock.SetNormalDay();

        var counter2 = new FakeBrewCounterService(4);
        var weatherMock = new Mock<IWeatherService>();
        weatherMock
            .Setup(w => w.GetCurrentTemperatureCelsiusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(20.0);
        var svc2 = new CoffeeMachineService(counter2, clock, weatherMock.Object,
                       Options.Create(new WeatherOptions()));

        await svc2.Brew();  // 5th call → 503
        counter2.Current.Should().Be(5);
    }

    [Fact]
    public async Task Brew_April1st_Returns418()
    {
        var (svc, _, _, _) = Build(aprilFools: true);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Teapot);
        result.Response.Should().BeNull();
    }

    [Fact]
    public async Task Brew_April1st_TakesPrecedenceOverFifthCall()
    {
        // Even if this is the 5th call, April 1st wins
        var (svc, _, _, _) = Build(counterStartAt: 4, aprilFools: true);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Teapot);
    }

    [Theory]
    [InlineData(3, 31)]   // March 31
    [InlineData(4, 2)]    // April 2
    [InlineData(4, 30)]   // April 30
    [InlineData(12, 31)]  // NYE
    public async Task Brew_NotAprilFirst_DoesNotReturn418(int month, int day)
    {
        var (svc, clock, _, _) = Build();
        clock.UtcNow = new DateTimeOffset(2024, month, day, 12, 0, 0, TimeSpan.Zero);

        var result = await svc.Brew();

        result.Outcome.Should().NotBe(BrewOutcome.Teapot);
    }

    [Fact]
    public async Task Brew_April1st_AllCallsReturn418_NotJustFirst()
    {
        var (svc, _, _, _) = Build(aprilFools: true);

        for (int i = 0; i < 10; i++)
        {
            var result = await svc.Brew();
            result.Outcome.Should().Be(BrewOutcome.Teapot, because: $"call {i + 1} on April 1st should be 418");
        }
    }

    [Fact]
    public async Task Brew_April1st_WeatherIsNeverChecked()
    {
        // On April 1st we short-circuit before reaching the weather call
        var (svc, _, _, weatherMock) = Build(aprilFools: true);

        await svc.Brew();

        weatherMock.Verify(
            w => w.GetCurrentTemperatureCelsiusAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Brew_TemperatureAboveThreshold_ReturnsIcedMessage()
    {
        var (svc, _, _, _) = Build(temperature: 35.0);

        var result = await svc.Brew();

        result.Response!.Message.Should().Be("Your refreshing iced coffee is ready");
    }

    [Fact]
    public async Task Brew_TemperatureExactlyAtThreshold_ReturnsHotMessage()
    {
        // Boundary: strictly greater than 30, not greater-than-or-equal
        var (svc, _, _, _) = Build(temperature: 30.0);

        var result = await svc.Brew();

        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Brew_WeatherServiceUnavailable_FallsBackToHotMessage()
    {
        // null = weather service failed; must not propagate the failure
        var (svc, _, _, _) = Build(temperature: null);

        var result = await svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Ready);
        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
    }
}
