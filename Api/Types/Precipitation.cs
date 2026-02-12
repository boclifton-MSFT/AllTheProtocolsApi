namespace Api.Types;

public record Precipitation(
    PrecipitationType Type,
    double AmountInches,
    int ProbabilityPercent);
