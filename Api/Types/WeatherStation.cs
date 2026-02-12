namespace Api.Types;

public record WeatherStation(
    string Id,
    string Name,
    Location Location,
    StationStatus Status);
