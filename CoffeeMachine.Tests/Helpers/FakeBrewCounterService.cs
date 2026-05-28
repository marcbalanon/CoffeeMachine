using CoffeeMachine.Api.Interfaces;

namespace CoffeeMachine.Tests.Helpers;

/// <summary>
/// Fake brew counter.
/// </summary>
public class FakeBrewCounterService : IBrewCounterService
{
    private int _count;

    public FakeBrewCounterService(int startAt = 0) => _count = startAt;

    public int Increment() => ++_count;
    public int Current => _count;
}