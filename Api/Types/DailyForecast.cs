namespace Api.Types;

public record DailyForecast(
    DateOnly Date,
    double HighTempF,
    double LowTempF,
    SkyCondition SkyCondition,
    WindInfo Wind,
    Precipitation Precipitation,
    string Sunrise,
    string Sunset);
