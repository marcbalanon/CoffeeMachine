namespace CoffeeMachine.Api.Interfaces;

/// <summary>
/// Tracks how many times /brew-coffee has been called.
/// Registered as a singleton so the counter persists for the lifetime of the application process.
/// </summary>
public interface IBrewCounterService
{
    /// <summary>
    /// Increments the call counter and returns the new value.
    /// </summary>
    int Increment();

    /// <summary>
    /// Returns the current call count without incrementing.
    /// </summary>
    int Current { get; }
}
