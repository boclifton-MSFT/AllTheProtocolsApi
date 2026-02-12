using Api.Services;
using Api.Types;

namespace Api.Endpoints;

public static class MapWeatherRestEndpoints
{
    public static WebApplication MapWeatherApi(this WebApplication app)
    {
        var weather = app.MapGroup("/api/weather")
            .WithTags("Weather");

        weather.MapGet("/stations", (IWeatherDataService svc, string? state) =>
            Results.Ok(svc.GetStations(state)))
            .WithName("GetStations")
            .WithSummary("Get all weather stations, optionally filtered by state");

        weather.MapGet("/stations/{id}", (IWeatherDataService svc, string id) =>
            svc.GetStation(id) is { } station
                ? Results.Ok(station)
                : Results.NotFound())
            .WithName("GetStation")
            .WithSummary("Get a weather station by ID");

        weather.MapGet("/stations/{stationId}/conditions", (IWeatherDataService svc, string stationId) =>
            svc.GetCurrentConditions(stationId) is { } conditions
                ? Results.Ok(conditions)
                : Results.NotFound())
            .WithName("GetCurrentConditions")
            .WithSummary("Get current weather conditions for a station");

        weather.MapGet("/stations/{stationId}/forecast/daily", (IWeatherDataService svc, string stationId, int? days) =>
            Results.Ok(svc.GetDailyForecast(stationId, Math.Clamp(days ?? 5, 1, 7))))
            .WithName("GetDailyForecast")
            .WithSummary("Get the daily forecast for a station (1-7 days)");

        weather.MapGet("/stations/{stationId}/forecast/hourly", (IWeatherDataService svc, string stationId, int? hours) =>
            Results.Ok(svc.GetHourlyForecast(stationId, Math.Clamp(hours ?? 24, 1, 24))))
            .WithName("GetHourlyForecast")
            .WithSummary("Get the hourly forecast for a station (1-24 hours)");

        weather.MapGet("/stations/{stationId}/air-quality", (IWeatherDataService svc, string stationId) =>
            svc.GetAirQuality(stationId) is { } aq
                ? Results.Ok(aq)
                : Results.NotFound())
            .WithName("GetAirQuality")
            .WithSummary("Get air quality data for a station");

        weather.MapGet("/alerts", (IWeatherDataService svc, string? stationId, AlertSeverity? minSeverity) =>
            Results.Ok(svc.GetActiveAlerts(stationId, minSeverity)))
            .WithName("GetActiveAlerts")
            .WithSummary("Get active weather alerts, optionally filtered by station and severity");

        return app;
    }
}
