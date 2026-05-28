using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressMapClientErrors = true;
    });

builder.Services.AddControllers();

// Singleton counter, one instance per process.
builder.Services.AddSingleton<IBrewCounterService, BrewCounterService>();

// Singleton clock.
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

// Weather options bound from appsettings.json 
builder.Services.Configure<WeatherOptions>(
    builder.Configuration.GetSection(WeatherOptions.Section));

// Weather Service
builder.Services.AddHttpClient<IWeatherService, OpenWeatherMapService>();

// Coffee machine service 
builder.Services.AddTransient<ICoffeeMachineService, CoffeeMachineService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Coffee Machine API", Version = "v1" });
});

// ── Pipeline ──────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Expose the type for WebApplicationFactory<Program> in integration tests
public partial class Program { }
