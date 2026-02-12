namespace Api.Types;

public record WeatherAlert(
    string Id,
    string Title,
    string Description,
    AlertSeverity Severity,
    AlertCategory Category,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    string AffectedArea);
