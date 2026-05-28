namespace CoffeeMachine.Api.Interfaces;

/// <summary>
/// Weather Service API.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Returns the current temperature in Celsius at the configured location.
    /// </summary>
    Task<double?> GetCurrentTemperatureCelsiusAsync(CancellationToken ct = default);
}
