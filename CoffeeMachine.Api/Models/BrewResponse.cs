using System.Text.Json.Serialization;

namespace CoffeeMachine.Api.Models;

/// <summary>
/// The JSON response body for a successful brew.
/// </summary>
public sealed record BrewResponse(
    [property: JsonPropertyName("message")]  string Message,
    [property: JsonPropertyName("prepared")] DateTimeOffset Prepared
);
