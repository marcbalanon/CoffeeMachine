using CoffeeMachine.Api.Interfaces;

namespace CoffeeMachine.Api.Services;

/// <summary>
/// Uses Interlocked.Increment so concurrent requests don't skew the count.
/// </summary>
public sealed class BrewCounterService : IBrewCounterService
{
    private int _count;

    public int Increment() => Interlocked.Increment(ref _count);

    public int Current => Volatile.Read(ref _count);
}
