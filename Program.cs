using System;
using Microsoft.Extensions.Logging;
using SunriseCalculator;

// Setup logging
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

ILogger logger = loggerFactory.CreateLogger<Sunrise>();
var sunriseCalculator = new Sunrise(logger);

// Parameters from the original main function
double latitude = 43.268399;
double longitude = -79.774549;
double elevation = 0;

// Get current timestamp (Unix epoch time)
DateTimeOffset now = DateTimeOffset.UtcNow;
double currentTime = now.ToUnixTimeSeconds();

// Get Toronto timezone
TimeZoneInfo torontoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

// Calculate and print results
var result = sunriseCalculator.Calc(
    currentTime,
    latitude,
    longitude,
    elevation,
    torontoTimeZone);

Console.WriteLine(result);
