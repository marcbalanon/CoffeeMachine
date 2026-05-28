namespace CoffeeMachine.Api.Models;

public class WeatherOptions
{
    public const string Section = "Weather";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>Temperature above which iced coffee</summary>
    public double HotThreshold { get; set; } = 30.0;

}
