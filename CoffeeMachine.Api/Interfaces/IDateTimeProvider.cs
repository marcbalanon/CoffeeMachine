namespace CoffeeMachine.Api.Interfaces;

/// <summary>
/// Current date time provider in UTC, with offset.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Returns the current date and time, with UTC offset.</summary>
    DateTimeOffset UtcNow { get; }
}
