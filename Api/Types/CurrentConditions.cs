namespace Api.Types;

public record CurrentConditions(
    DateTime ObservedAt,
    double TemperatureF,
    double FeelsLikeF,
    int HumidityPercent,
    double DewPointF,
    double PressureMb,
    double VisibilityMiles,
    int UvIndex,
    SkyCondition SkyCondition,
    WindInfo Wind,
    Precipitation Precipitation);
