using CoffeeMachine.Api.Interfaces;

namespace CoffeeMachine.Tests.Helpers;

/// <summary>
/// Fake date time provider.
/// </summary>
public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new DateTimeOffset(2024, 6, 15, 10, 0, 0, TimeSpan.Zero);

    /// <summary>Set to April 1st</summary>
    public void SetAprilFools()
        => UtcNow = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Set to any non-April-1st date.</summary>
    public void SetNormalDay()
        => UtcNow = new DateTimeOffset(2026, 5, 28, 10, 0, 0, TimeSpan.Zero);
}


