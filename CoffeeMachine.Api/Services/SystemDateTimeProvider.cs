using CoffeeMachine.Api.Interfaces;

namespace CoffeeMachine.Api.Services;

/// <summary>
/// DateTime provider
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
