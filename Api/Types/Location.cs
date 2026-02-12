namespace Api.Types;

public record Location(
    double Latitude,
    double Longitude,
    double ElevationFt,
    string City,
    string State,
    string Country);
