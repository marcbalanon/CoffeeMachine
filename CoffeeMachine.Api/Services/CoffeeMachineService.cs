using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Api.Models;
using Microsoft.Extensions.Options;

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
    private readonly IWeatherService _weather;
    private readonly WeatherOptions _weatherOpts;

    public CoffeeMachineService(
        IBrewCounterService counter,
        IDateTimeProvider clock,
        IWeatherService weather,
        IOptions<WeatherOptions> weatherOpts)
    {
        _counter = counter;
        _clock = clock;
        _weather = weather;
        _weatherOpts = weatherOpts.Value;
    }

    public async Task<BrewResult> Brew(CancellationToken ct = default)
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
        var message = await BrewedCoffeeReady(ct);
        return new BrewResult(BrewOutcome.Ready, new BrewResponse(message, now));
    }
    private async Task<string> BrewedCoffeeReady(CancellationToken ct)
    {
        var temp = await _weather.GetCurrentTemperatureCelsiusAsync(ct);

        var message = (temp.HasValue && temp.Value > _weatherOpts.HotThreshold) ? "Your refreshing iced coffee is ready" : "Your piping hot coffee is ready";
        return message;
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
