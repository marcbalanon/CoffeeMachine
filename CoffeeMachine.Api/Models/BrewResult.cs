using CoffeeMachine.Api.Interfaces;

namespace CoffeeMachine.Api.Models;

/// <summary>
/// Brewing outcome enum.
/// </summary>
public enum BrewOutcome
{
    /// <summary>200 OK — coffee is ready.</summary>
    Ready,

    /// <summary>503 Service Unavailable — every 5th call, machine is out of coffee.</summary>
    OutOfCoffee,

    /// <summary>418 I'm a Teapot — called on April 1st.</summary>
    Teapot
}

/// <summary>
/// Carries the outcome and the optional response payload.
/// </summary>
public sealed record BrewResult(
    BrewOutcome Outcome,
    BrewResponse? Response = null
);
