using Api.Services;

namespace Api.Types;

[QueryType]
public static class Query
{
    /// <summary>
    /// Get all weather stations, optionally filtered by US state abbreviation.
    /// </summary>
    public static IEnumerable<WeatherStation> GetStations(
        IWeatherDataService service,
        string? state = null)
        => service.GetStations(state);

    /// <summary>
    /// Get a single weather station by its unique identifier.
    /// </summary>
    public static WeatherStation? GetStation(
        IWeatherDataService service,
        string id)
        => service.GetStation(id);

    /// <summary>
    /// Get the current weather conditions at a station.
    /// Optionally specify a temperature unit for conversion (default: Fahrenheit).
    /// </summary>
    public static CurrentConditions? GetCurrentConditions(
        IWeatherDataService service,
        string stationId,
        TemperatureUnit unit = TemperatureUnit.Fahrenheit)
    {
        var conditions = service.GetCurrentConditions(stationId);
        if (conditions is null || unit == TemperatureUnit.Fahrenheit)
            return conditions;

        // Convert Fahrenheit to Celsius
        return conditions with
        {
            TemperatureF = ToCelsius(conditions.TemperatureF),
            FeelsLikeF = ToCelsius(conditions.FeelsLikeF),
            DewPointF = ToCelsius(conditions.DewPointF),
        };
    }

    /// <summary>
    /// Get the daily forecast for a station (up to 7 days).
    /// </summary>
    public static IEnumerable<DailyForecast> GetDailyForecast(
        IWeatherDataService service,
        string stationId,
        int days = 5)
        => service.GetDailyForecast(stationId, Math.Clamp(days, 1, 7));

    /// <summary>
    /// Get the hourly forecast for a station (up to 24 hours).
    /// </summary>
    public static IEnumerable<HourlyForecast> GetHourlyForecast(
        IWeatherDataService service,
        string stationId,
        int hours = 24)
        => service.GetHourlyForecast(stationId, Math.Clamp(hours, 1, 24));

    /// <summary>
    /// Get active weather alerts, optionally filtered by station and minimum severity.
    /// </summary>
    public static IEnumerable<WeatherAlert> GetActiveAlerts(
        IWeatherDataService service,
        string? stationId = null,
        AlertSeverity? minSeverity = null)
        => service.GetActiveAlerts(stationId, minSeverity);

    /// <summary>
    /// Get air quality data for a station.
    /// </summary>
    public static AirQuality? GetAirQuality(
        IWeatherDataService service,
        string stationId)
        => service.GetAirQuality(stationId);

    private static double ToCelsius(double fahrenheit)
        => Math.Round((fahrenheit - 32) * 5.0 / 9.0, 1);
}
