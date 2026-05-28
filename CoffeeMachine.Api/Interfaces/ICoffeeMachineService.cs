using CoffeeMachine.Api.Models;

namespace CoffeeMachine.Api.Interfaces;

/// <summary>
/// Encapsulates the business rules for the coffee machine endpoint.
/// </summary>
public interface ICoffeeMachineService
{
    /// <summary>
    /// Evaluates all business rules and returns the appropriate result.
    /// </summary>
    Task<BrewResult> Brew(CancellationToken ct = default);
}
