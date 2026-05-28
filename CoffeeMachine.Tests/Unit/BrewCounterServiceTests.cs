using CoffeeMachine.Api.Services;
using FluentAssertions;
using Xunit;

namespace CoffeeMachine.Tests.Unit;

public class BrewCounterServiceTests
{
    [Fact]
    public void Increment_StartsAt1_OnFirstCall()
    {
        var counter = new BrewCounterService();
        counter.Increment().Should().Be(1);
    }

    [Fact]
    public void Increment_ReturnsMonotonicallyIncreasingValues()
    {
        var counter = new BrewCounterService();

        for (int expected = 1; expected <= 10; expected++)
            counter.Increment().Should().Be(expected);
    }

    [Fact]
    public void Current_ReflectsLatestCount()
    {
        var counter = new BrewCounterService();
        counter.Increment();
        counter.Increment();

        counter.Current.Should().Be(2);
    }
}
