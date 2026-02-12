using Api.Types;

namespace Api.Services;

public interface IWeatherDataService
{
    IEnumerable<WeatherStation> GetStations(string? state = null);
    WeatherStation? GetStation(string id);
    CurrentConditions? GetCurrentConditions(string stationId);
    IEnumerable<DailyForecast> GetDailyForecast(string stationId, int days = 5);
    IEnumerable<HourlyForecast> GetHourlyForecast(string stationId, int hours = 24);
    IEnumerable<WeatherAlert> GetActiveAlerts(string? stationId = null, AlertSeverity? minSeverity = null);
    AirQuality? GetAirQuality(string stationId);
}
