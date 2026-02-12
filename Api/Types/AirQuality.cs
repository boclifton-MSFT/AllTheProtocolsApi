namespace Api.Types;

public record AirQuality(
    int Aqi,
    AirQualityCategory Category,
    string PrimaryPollutant,
    Dictionary<string, double> Pollutants);
