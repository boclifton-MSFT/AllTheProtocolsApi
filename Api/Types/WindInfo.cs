namespace Api.Types;

public record WindInfo(
    double SpeedMph,
    WindDirection Direction,
    double? GustsMph);
