using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using CoffeeMachine.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CoffeeMachine.Tests.Unit;

/// <summary>
/// CoffeeMachineService unit tests
/// </summary>
public sealed class CoffeeMachineServiceTests
{
    private static (CoffeeMachineService svc, FakeDateTimeProvider clock, FakeBrewCounterService counter)
        Build(int counterStartAt = 0, bool aprilFools = false)
    {
        var clock   = new FakeDateTimeProvider();
        var counter = new FakeBrewCounterService(counterStartAt);
        var svc     = new CoffeeMachineService(counter, clock);

        if (aprilFools) clock.SetAprilFools();
        else            clock.SetNormalDay();

        return (svc, clock, counter);
    }

    [Fact]
    public void Brew_NormalDay_FirstCall_Returns200WithMessage()
    {
        var (svc, clock, _) = Build();

        var result = svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Ready);
        result.Response.Should().NotBeNull();
        result.Response!.Message.Should().Be("Your piping hot coffee is ready");
        result.Response.Prepared.Should().Be(clock.UtcNow);
    }

    [Fact]
    public void Brew_NormalDay_PreparedTimestampMatchesClockValue()
    {
        var expectedTime = new DateTimeOffset(2024, 8, 20, 14, 30, 0, TimeSpan.Zero);
        var (svc, clock, _) = Build();
        clock.UtcNow = expectedTime;

        var result = svc.Brew();

        result.Response!.Prepared.Should().Be(expectedTime);
    }

    [Theory]
    [InlineData(4)]   // 5th call
    [InlineData(9)]   // 10th call
    [InlineData(14)]  // 15th call
    [InlineData(19)]  // 20th call
    public void Brew_EveryFifthCall_Returns503(int counterStart)
    {
        var (svc, _, _) = Build(counterStart);

        var result = svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.OutOfCoffee);
        result.Response.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]   // 1st call
    [InlineData(1)]   // 2nd call
    [InlineData(2)]   // 3rd call
    [InlineData(3)]   // 4th call
    public void Brew_NonFifthCall_Returns200(int counterStart)
    {
        var (svc, _, _) = Build(counterStart);

        var result = svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Ready);
    }

    [Fact]
    public void Brew_CounterAlwaysIncrements_EvenWhenOtherRulesApply()
    {
        // Counter should increment on every call — including 503 and 418 responses
        var (svc, clock, counter) = Build(0, aprilFools: true);

        svc.Brew();  // April 1st → 418
        counter.Current.Should().Be(1);

        clock.SetNormalDay();

        // Start a fresh counter at 4, so the 5th call (503) also increments
        var counter2 = new FakeBrewCounterService(4);
        var svc2     = new CoffeeMachineService(counter2, clock);
        svc2.Brew();  // 5th call → 503
        counter2.Current.Should().Be(5);
    }

    [Fact]
    public void Brew_April1st_Returns418()
    {
        var (svc, _, _) = Build(aprilFools: true);

        var result = svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Teapot);
        result.Response.Should().BeNull();
    }

    [Fact]
    public void Brew_April1st_TakesPrecedenceOverFifthCall()
    {
        // Even if this is the 5th call, April 1st wins
        var (svc, _, _) = Build(counterStartAt: 4, aprilFools: true);

        var result = svc.Brew();

        result.Outcome.Should().Be(BrewOutcome.Teapot);
    }

    [Theory]
    [InlineData(3, 31)]   // March 31
    [InlineData(4, 2)]    // April 2
    [InlineData(4, 30)]   // April 30
    [InlineData(12, 31)]  // NYE
    public void Brew_NotAprilFirst_DoesNotReturn418(int month, int day)
    {
        var (svc, clock, _) = Build();
        clock.UtcNow = new DateTimeOffset(2024, month, day, 12, 0, 0, TimeSpan.Zero);

        var result = svc.Brew();

        result.Outcome.Should().NotBe(BrewOutcome.Teapot);
    }

    [Fact]
    public void Brew_April1st_AllCallsReturn418_NotJustFirst()
    {
        var (svc, _, _) = Build(aprilFools: true);

        for (int i = 0; i < 10; i++)
        {
            var result = svc.Brew();
            result.Outcome.Should().Be(BrewOutcome.Teapot, because: $"call {i + 1} on April 1st should be 418");
        }
    }
}
