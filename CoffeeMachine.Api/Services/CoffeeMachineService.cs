using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Api.Models;
using System.Reflection.Metadata.Ecma335;

namespace CoffeeMachine.Api.Services;

/// <summary>
/// Implements the coffee machine rules in priority order:
///
///   1. April 1st - 418 Teapot
///   2. Every 5th call - 503 Out of coffee
///   3. Otherwise - 200 with JSON body
///
/// </summary>
public sealed class CoffeeMachineService : ICoffeeMachineService
{
    private readonly IBrewCounterService _counter;
    private readonly IDateTimeProvider _clock;

    public CoffeeMachineService(
        IBrewCounterService counter,
        IDateTimeProvider clock)
    {
        _counter = counter;
        _clock   = clock;
    }

    public BrewResult Brew()
    {
        var now   = _clock.UtcNow;
        var count = _counter.Increment();   // always increment regardless of outcome

        // Rule 3 
        if (IsAprilFools(now))
            return new BrewResult(BrewOutcome.Teapot);

        // Rule 2 every 5th call
        if (IsEveryFifthCall(count))
            return new BrewResult(BrewOutcome.OutOfCoffee);

        // Rule 1
        var response = new BrewResponse(
            Message:  "Your piping hot coffee is ready",
            Prepared: now
        );
        return new BrewResult(BrewOutcome.Ready, response);
    }

    private static bool IsAprilFools(DateTimeOffset dt)
    {
        return (dt.Month == 4 && dt.Day == 1);
    }
        

    private static bool IsEveryFifthCall(int count)
    {
        return (count >= 5);
    }
        
}
