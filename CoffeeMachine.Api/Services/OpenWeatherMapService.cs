using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace CoffeeMachine.Api.Services;

/// <summary>
/// Calls the OpenWeatherMap "Current Weather" API.
/// </summary>
public sealed class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly WeatherOptions _opts;
    private readonly ILogger<OpenWeatherMapService> _logger;

    public OpenWeatherMapService(
        HttpClient http,
        IOptions<WeatherOptions> opts,
        ILogger<OpenWeatherMapService> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task<double?> GetCurrentTemperatureCelsiusAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{_opts.BaseUrl}"
                    + $"?lat={_opts.Latitude}&lon={_opts.Longitude}"
                    + $"&units=metric&appid={_opts.ApiKey}";

            var dto = await _http.GetFromJsonAsync<OWMResponse>(url, ct);
            return dto?.Main?.Temp;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Weather service unavailable; brewing without temperature check.");
            return null;
        }
    }

    private record OWMResponse(
        [property: JsonPropertyName("main")] OWMMain? Main);

    private record OWMMain(
        [property: JsonPropertyName("temp")] double Temp);
}
