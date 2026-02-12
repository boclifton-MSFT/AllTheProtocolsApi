namespace Api.Types;

public record HourlyForecast(
    DateTime DateTime,
    double TemperatureF,
    double FeelsLikeF,
    SkyCondition SkyCondition,
    WindInfo Wind,
    Precipitation Precipitation);
