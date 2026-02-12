using System.ComponentModel;
using Api.Services;
using ModelContextProtocol.Server;

namespace Api.McpTools;

/// <summary>
/// Sample MCP tools for demonstration purposes.
/// These tools can be invoked by MCP clients to perform various operations.
/// </summary>
public class WeatherDataTools(IWeatherDataService weatherDataService)
{
    [McpServerTool]
    [Description("Gets a list of available weather stations with their IDs and names.")]
    public string GetAvailableStations()
    {
        var stations = weatherDataService.GetStations();
        return stations.Any()
            ? "Available Weather Stations:\n" + string.Join("\n", stations.Select(s => $"{s.Id}: {s.Name}"))
            : "No weather stations available.";
    }

    [McpServerTool]
    [Description("Gets the current weather conditions for the specified station ID.")]
    public string GetCurrentConditions(
    [Description("The unique identifier of the weather station")] string stationId)
    {
        // Call the weather data service to get current conditions for the station.
        var conditions = weatherDataService.GetCurrentConditions(stationId);
        return conditions != null
            ? $"Current conditions for station {stationId}: {conditions.TemperatureF}°F, {conditions.SkyCondition}, {conditions.Wind.SpeedMph} mph {conditions.Wind.Direction} wind."
            : $"No current conditions available for station {stationId}.";
    }

    [McpServerTool]
    [Description("Gets the 5-day daily forecast for the specified station ID.")]
    public string GetDailyForecast(
        [Description("The unique identifier of the weather station")] string stationId)
    {
        var forecast = weatherDataService.GetDailyForecast(stationId, 5);
        if (!forecast.Any())
            return $"No forecast data available for station {stationId}.";

        var forecastSummary = string.Join("\n", forecast.Select(f =>
            $"{f.Date:MM/dd}: High {f.HighTempF}°F, Low {f.LowTempF}°F, {f.SkyCondition}"));
        return $"5-day forecast for station {stationId}:\n{forecastSummary}";
    }

    [McpServerTool]
    [Description("Gets the current air quality index for the specified station ID.")]
    public string GetAirQuality(
    [Description("The unique identifier of the weather station")] string stationId)
    {
        var airQuality = weatherDataService.GetAirQuality(stationId);
        return airQuality != null
            ? $"Current AQI for station {stationId}: {airQuality.Aqi} ({airQuality.Category}), Primary Pollutant: {airQuality.PrimaryPollutant}"
            : $"No air quality data available for station {stationId}.";
    }

    [McpServerTool]
    [Description("Gets specified weather data for a station in a human-readable format.")]
    public string GetWeatherSummary(
        [Description("The unique identifier of the weather station")] string stationId,
        [Description("Whether to include current conditions in the summary")] bool includeCurrent = true,
        [Description("Whether to include the 5-day forecast in the summary")] bool includeForecast = true,
        [Description("Whether to include air quality information in the summary")] bool includeAirQuality = false)
    {
        var summaryParts = new List<string>();

        if (includeCurrent)
            summaryParts.Add(GetCurrentConditions(stationId));

        if (includeForecast)
            summaryParts.Add(GetDailyForecast(stationId));

        if (includeAirQuality)
            summaryParts.Add(GetAirQuality(stationId));

        return string.Join("\n\n", summaryParts);
    }
}