using Microsoft.Extensions.Logging;
using SunriseCalculator;

// Setup logging
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

ILogger logger = loggerFactory.CreateLogger<Sunrise>();
var sunriseCalculator = new Sunrise(logger);

// Parameters from the original main function
double latitude = 43.268399;
double longitude = -79.774549;
double elevation = 74;

// Get Toronto timezone
TimeZoneInfo torontoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

// Calculate and print results
var result = sunriseCalculator.Calculate(
    DateTimeOffset.UtcNow,
    latitude,
    longitude,
    elevation,
    torontoTimeZone);

Console.WriteLine(result);
