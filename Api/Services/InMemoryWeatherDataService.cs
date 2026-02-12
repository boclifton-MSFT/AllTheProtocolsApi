using Api.Types;

namespace Api.Services;

public class InMemoryWeatherDataService : IWeatherDataService
{
    private readonly Dictionary<string, WeatherStation> _stations;
    private readonly Dictionary<string, CurrentConditions> _conditions;
    private readonly Dictionary<string, List<DailyForecast>> _dailyForecasts;
    private readonly Dictionary<string, List<HourlyForecast>> _hourlyForecasts;
    private readonly Dictionary<string, List<WeatherAlert>> _alerts;
    private readonly Dictionary<string, AirQuality> _airQuality;

    public InMemoryWeatherDataService()
    {
        _stations = SeedStations();
        _conditions = SeedCurrentConditions();
        _dailyForecasts = SeedDailyForecasts();
        _hourlyForecasts = SeedHourlyForecasts();
        _alerts = SeedAlerts();
        _airQuality = SeedAirQuality();
    }

    public IEnumerable<WeatherStation> GetStations(string? state = null)
    {
        var stations = _stations.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(state))
            stations = stations.Where(s => s.Location.State.Equals(state, StringComparison.OrdinalIgnoreCase));
        return stations;
    }

    public WeatherStation? GetStation(string id)
        => _stations.GetValueOrDefault(id);

    public CurrentConditions? GetCurrentConditions(string stationId)
        => _conditions.GetValueOrDefault(stationId);

    public IEnumerable<DailyForecast> GetDailyForecast(string stationId, int days = 5)
        => _dailyForecasts.TryGetValue(stationId, out var forecasts)
            ? forecasts.Take(days)
            : [];

    public IEnumerable<HourlyForecast> GetHourlyForecast(string stationId, int hours = 24)
        => _hourlyForecasts.TryGetValue(stationId, out var forecasts)
            ? forecasts.Take(hours)
            : [];

    public IEnumerable<WeatherAlert> GetActiveAlerts(string? stationId = null, AlertSeverity? minSeverity = null)
    {
        var alerts = stationId is not null && _alerts.TryGetValue(stationId, out var stationAlerts)
            ? stationAlerts.AsEnumerable()
            : _alerts.Values.SelectMany(a => a);

        if (minSeverity.HasValue)
            alerts = alerts.Where(a => a.Severity >= minSeverity.Value);

        return alerts;
    }

    public AirQuality? GetAirQuality(string stationId)
        => _airQuality.GetValueOrDefault(stationId);

    // ───────────────────────────── Seed Data ─────────────────────────────

    private static Dictionary<string, WeatherStation> SeedStations() => new()
    {
        ["den01"] = new WeatherStation("den01", "Denver International", new Api.Types.Location(39.8561, -104.6737, 5431, "Denver", "CO", "US"), StationStatus.Online),
        ["mia01"] = new WeatherStation("mia01", "Miami Beach Oceanside", new Api.Types.Location(25.7907, -80.1300, 7, "Miami", "FL", "US"), StationStatus.Online),
        ["sea01"] = new WeatherStation("sea01", "Seattle-Tacoma Metro", new Api.Types.Location(47.4502, -122.3088, 433, "Seattle", "WA", "US"), StationStatus.Online),
        ["phx01"] = new WeatherStation("phx01", "Phoenix Sky Harbor", new Api.Types.Location(33.4373, -112.0078, 1135, "Phoenix", "AZ", "US"), StationStatus.Online),
        ["chi01"] = new WeatherStation("chi01", "Chicago O'Hare", new Api.Types.Location(41.9742, -87.9073, 672, "Chicago", "IL", "US"), StationStatus.Maintenance),
    };

    private static Dictionary<string, CurrentConditions> SeedCurrentConditions()
    {
        var now = DateTime.UtcNow;
        return new()
        {
            ["den01"] = new CurrentConditions(
                ObservedAt: now,
                TemperatureF: 42.0,
                FeelsLikeF: 36.0,
                HumidityPercent: 28,
                DewPointF: 12.0,
                PressureMb: 1024.5,
                VisibilityMiles: 10.0,
                UvIndex: 3,
                SkyCondition: SkyCondition.PartlyCloudy,
                Wind: new WindInfo(12.5, WindDirection.NW, 22.0),
                Precipitation: new Precipitation(PrecipitationType.None, 0.0, 5)),

            ["mia01"] = new CurrentConditions(
                ObservedAt: now,
                TemperatureF: 84.0,
                FeelsLikeF: 91.0,
                HumidityPercent: 78,
                DewPointF: 76.0,
                PressureMb: 1013.2,
                VisibilityMiles: 8.0,
                UvIndex: 9,
                SkyCondition: SkyCondition.MostlyCloudy,
                Wind: new WindInfo(8.0, WindDirection.SE, null),
                Precipitation: new Precipitation(PrecipitationType.Rain, 0.15, 60)),

            ["sea01"] = new CurrentConditions(
                ObservedAt: now,
                TemperatureF: 52.0,
                FeelsLikeF: 49.0,
                HumidityPercent: 88,
                DewPointF: 48.0,
                PressureMb: 1008.7,
                VisibilityMiles: 5.0,
                UvIndex: 1,
                SkyCondition: SkyCondition.Overcast,
                Wind: new WindInfo(6.0, WindDirection.S, 11.0),
                Precipitation: new Precipitation(PrecipitationType.Rain, 0.35, 85)),

            ["phx01"] = new CurrentConditions(
                ObservedAt: now,
                TemperatureF: 105.0,
                FeelsLikeF: 103.0,
                HumidityPercent: 8,
                DewPointF: 22.0,
                PressureMb: 1010.0,
                VisibilityMiles: 10.0,
                UvIndex: 11,
                SkyCondition: SkyCondition.Clear,
                Wind: new WindInfo(4.0, WindDirection.WSW, null),
                Precipitation: new Precipitation(PrecipitationType.None, 0.0, 0)),

            ["chi01"] = new CurrentConditions(
                ObservedAt: now,
                TemperatureF: 18.0,
                FeelsLikeF: 2.0,
                HumidityPercent: 65,
                DewPointF: 7.0,
                PressureMb: 1030.1,
                VisibilityMiles: 7.0,
                UvIndex: 1,
                SkyCondition: SkyCondition.MostlyCloudy,
                Wind: new WindInfo(22.0, WindDirection.NNW, 38.0),
                Precipitation: new Precipitation(PrecipitationType.Snow, 0.8, 70)),
        };
    }

    private static Dictionary<string, List<DailyForecast>> SeedDailyForecasts()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new()
        {
            ["den01"] = GenerateDenverDaily(today),
            ["mia01"] = GenerateMiamiDaily(today),
            ["sea01"] = GenerateSeattleDaily(today),
            ["phx01"] = GeneratePhoenixDaily(today),
            ["chi01"] = GenerateChicagoDaily(today),
        };
    }

    private static Dictionary<string, List<HourlyForecast>> SeedHourlyForecasts()
    {
        var now = DateTime.UtcNow;
        return new()
        {
            ["den01"] = GenerateHourly(now, baseTemp: 42, range: 8, SkyCondition.PartlyCloudy, WindDirection.NW, 12.5, PrecipitationType.None, 5),
            ["mia01"] = GenerateHourly(now, baseTemp: 84, range: 4, SkyCondition.MostlyCloudy, WindDirection.SE, 8.0, PrecipitationType.Rain, 60),
            ["sea01"] = GenerateHourly(now, baseTemp: 52, range: 3, SkyCondition.Overcast, WindDirection.S, 6.0, PrecipitationType.Rain, 85),
            ["phx01"] = GenerateHourly(now, baseTemp: 105, range: 6, SkyCondition.Clear, WindDirection.WSW, 4.0, PrecipitationType.None, 0),
            ["chi01"] = GenerateHourly(now, baseTemp: 18, range: 5, SkyCondition.MostlyCloudy, WindDirection.NNW, 22.0, PrecipitationType.Snow, 70),
        };
    }

    private static Dictionary<string, List<WeatherAlert>> SeedAlerts()
    {
        var now = DateTime.UtcNow;
        return new()
        {
            ["den01"] = [],
            ["mia01"] =
            [
                new WeatherAlert(
                    "alert-mia-001",
                    "Tropical Storm Watch",
                    "A tropical storm watch is in effect for coastal Miami-Dade County. Sustained winds of 40-60 mph possible with higher gusts. Prepare emergency supplies and monitor updates.",
                    AlertSeverity.Watch,
                    AlertCategory.Hurricane,
                    now.AddHours(-6),
                    now.AddHours(48),
                    "Coastal Miami-Dade County"),
                new WeatherAlert(
                    "alert-mia-002",
                    "Flood Advisory",
                    "Heavy rainfall expected over the next 24 hours. Urban and low-lying areas may experience localized flooding. Avoid driving through standing water.",
                    AlertSeverity.Advisory,
                    AlertCategory.Flood,
                    now.AddHours(-2),
                    now.AddHours(24),
                    "Greater Miami Metro Area"),
            ],
            ["sea01"] = [],
            ["phx01"] =
            [
                new WeatherAlert(
                    "alert-phx-001",
                    "Excessive Heat Warning",
                    "Dangerously hot conditions with temperatures exceeding 110°F expected. Limit outdoor activity, stay hydrated, and never leave children or pets in vehicles.",
                    AlertSeverity.Warning,
                    AlertCategory.ExtremeHeat,
                    now.AddHours(-12),
                    now.AddHours(36),
                    "Maricopa County including Phoenix Metro"),
            ],
            ["chi01"] =
            [
                new WeatherAlert(
                    "alert-chi-001",
                    "Winter Storm Warning",
                    "Heavy snow expected with accumulations of 8-12 inches. Wind gusts up to 45 mph will cause blowing and drifting snow with near-zero visibility at times.",
                    AlertSeverity.Warning,
                    AlertCategory.WinterStorm,
                    now.AddHours(-3),
                    now.AddHours(18),
                    "Cook County and surrounding suburbs"),
                new WeatherAlert(
                    "alert-chi-002",
                    "High Wind Advisory",
                    "Northwest winds 25-35 mph with gusts up to 50 mph. Secure outdoor objects. Use caution while driving, especially high-profile vehicles.",
                    AlertSeverity.Advisory,
                    AlertCategory.HighWind,
                    now.AddHours(-1),
                    now.AddHours(12),
                    "Greater Chicago Metropolitan Area"),
            ],
        };
    }

    private static Dictionary<string, AirQuality> SeedAirQuality() => new()
    {
        ["den01"] = new AirQuality(45, AirQualityCategory.Good, "PM2.5",
            new Dictionary<string, double> { ["PM2.5"] = 8.2, ["PM10"] = 18.0, ["O3"] = 32.0, ["NO2"] = 12.0, ["SO2"] = 1.5, ["CO"] = 0.4 }),

        ["mia01"] = new AirQuality(62, AirQualityCategory.Moderate, "O3",
            new Dictionary<string, double> { ["PM2.5"] = 11.0, ["PM10"] = 22.0, ["O3"] = 55.0, ["NO2"] = 18.0, ["SO2"] = 3.0, ["CO"] = 0.6 }),

        ["sea01"] = new AirQuality(38, AirQualityCategory.Good, "PM2.5",
            new Dictionary<string, double> { ["PM2.5"] = 6.5, ["PM10"] = 14.0, ["O3"] = 28.0, ["NO2"] = 10.0, ["SO2"] = 1.0, ["CO"] = 0.3 }),

        ["phx01"] = new AirQuality(112, AirQualityCategory.UnhealthyForSensitiveGroups, "PM10",
            new Dictionary<string, double> { ["PM2.5"] = 22.0, ["PM10"] = 85.0, ["O3"] = 68.0, ["NO2"] = 25.0, ["SO2"] = 5.0, ["CO"] = 1.2 }),

        ["chi01"] = new AirQuality(55, AirQualityCategory.Moderate, "PM2.5",
            new Dictionary<string, double> { ["PM2.5"] = 14.0, ["PM10"] = 30.0, ["O3"] = 35.0, ["NO2"] = 22.0, ["SO2"] = 4.0, ["CO"] = 0.8 }),
    };

    // ───────────────────────── Daily Forecast Generators ─────────────────────────

    private static List<DailyForecast> GenerateDenverDaily(DateOnly start) =>
    [
        new(start,           45, 22, SkyCondition.PartlyCloudy, new(10, WindDirection.NW, 18.0),  new(PrecipitationType.None, 0.0, 5),   "06:42", "17:28"),
        new(start.AddDays(1), 38, 18, SkyCondition.MostlyCloudy, new(15, WindDirection.N, 25.0),   new(PrecipitationType.Snow, 0.2, 40),  "06:41", "17:29"),
        new(start.AddDays(2), 32, 12, SkyCondition.Overcast,     new(18, WindDirection.NNW, 30.0), new(PrecipitationType.Snow, 1.5, 80),  "06:40", "17:30"),
        new(start.AddDays(3), 35, 15, SkyCondition.MostlyCloudy, new(12, WindDirection.W, 20.0),   new(PrecipitationType.Snow, 0.3, 35),  "06:39", "17:31"),
        new(start.AddDays(4), 48, 25, SkyCondition.Clear,        new(8, WindDirection.SW, null),    new(PrecipitationType.None, 0.0, 0),   "06:38", "17:33"),
        new(start.AddDays(5), 55, 30, SkyCondition.Clear,        new(6, WindDirection.S, null),     new(PrecipitationType.None, 0.0, 0),   "06:37", "17:34"),
        new(start.AddDays(6), 50, 28, SkyCondition.PartlyCloudy, new(10, WindDirection.NW, 16.0),  new(PrecipitationType.None, 0.0, 10),  "06:36", "17:35"),
    ];

    private static List<DailyForecast> GenerateMiamiDaily(DateOnly start) =>
    [
        new(start,           86, 76, SkyCondition.MostlyCloudy, new(8, WindDirection.SE, null),    new(PrecipitationType.Rain, 0.3, 65),  "06:55", "18:12"),
        new(start.AddDays(1), 88, 78, SkyCondition.PartlyCloudy, new(10, WindDirection.E, 15.0),   new(PrecipitationType.Rain, 0.1, 40),  "06:55", "18:13"),
        new(start.AddDays(2), 85, 77, SkyCondition.Overcast,     new(14, WindDirection.ESE, 22.0), new(PrecipitationType.Rain, 0.8, 80),  "06:54", "18:13"),
        new(start.AddDays(3), 83, 75, SkyCondition.Overcast,     new(18, WindDirection.E, 28.0),   new(PrecipitationType.Rain, 1.2, 90),  "06:54", "18:14"),
        new(start.AddDays(4), 82, 74, SkyCondition.MostlyCloudy, new(20, WindDirection.ENE, 32.0), new(PrecipitationType.Rain, 0.6, 70),  "06:53", "18:14"),
        new(start.AddDays(5), 84, 76, SkyCondition.PartlyCloudy, new(12, WindDirection.SE, null),  new(PrecipitationType.Rain, 0.2, 45),  "06:53", "18:15"),
        new(start.AddDays(6), 87, 77, SkyCondition.PartlyCloudy, new(8, WindDirection.S, null),    new(PrecipitationType.None, 0.0, 20),  "06:52", "18:15"),
    ];

    private static List<DailyForecast> GenerateSeattleDaily(DateOnly start) =>
    [
        new(start,           53, 44, SkyCondition.Overcast,      new(6, WindDirection.S, 11.0),    new(PrecipitationType.Rain, 0.4, 85),  "07:22", "17:15"),
        new(start.AddDays(1), 51, 43, SkyCondition.Overcast,      new(8, WindDirection.SSW, 14.0),  new(PrecipitationType.Rain, 0.6, 90),  "07:21", "17:16"),
        new(start.AddDays(2), 50, 42, SkyCondition.MostlyCloudy, new(10, WindDirection.SW, 16.0),  new(PrecipitationType.Rain, 0.3, 75),  "07:20", "17:18"),
        new(start.AddDays(3), 52, 44, SkyCondition.Overcast,      new(7, WindDirection.S, 12.0),    new(PrecipitationType.Rain, 0.5, 80),  "07:19", "17:19"),
        new(start.AddDays(4), 55, 45, SkyCondition.MostlyCloudy, new(5, WindDirection.W, null),    new(PrecipitationType.Rain, 0.1, 50),  "07:18", "17:20"),
        new(start.AddDays(5), 57, 46, SkyCondition.PartlyCloudy, new(4, WindDirection.NW, null),   new(PrecipitationType.None, 0.0, 25),  "07:17", "17:22"),
        new(start.AddDays(6), 54, 44, SkyCondition.Overcast,      new(8, WindDirection.SSW, 13.0),  new(PrecipitationType.Rain, 0.4, 80),  "07:16", "17:23"),
    ];

    private static List<DailyForecast> GeneratePhoenixDaily(DateOnly start) =>
    [
        new(start,           108, 82, SkyCondition.Clear,        new(4, WindDirection.WSW, null),   new(PrecipitationType.None, 0.0, 0),   "06:15", "18:35"),
        new(start.AddDays(1), 110, 84, SkyCondition.Clear,        new(5, WindDirection.W, null),     new(PrecipitationType.None, 0.0, 0),   "06:15", "18:36"),
        new(start.AddDays(2), 112, 85, SkyCondition.Clear,        new(3, WindDirection.SW, null),    new(PrecipitationType.None, 0.0, 0),   "06:14", "18:36"),
        new(start.AddDays(3), 109, 83, SkyCondition.Hazy,         new(6, WindDirection.S, null),     new(PrecipitationType.None, 0.0, 5),   "06:14", "18:37"),
        new(start.AddDays(4), 106, 80, SkyCondition.PartlyCloudy, new(8, WindDirection.SE, 14.0),   new(PrecipitationType.None, 0.0, 10),  "06:13", "18:37"),
        new(start.AddDays(5), 104, 79, SkyCondition.PartlyCloudy, new(10, WindDirection.E, 16.0),   new(PrecipitationType.Rain, 0.05, 15), "06:13", "18:38"),
        new(start.AddDays(6), 107, 81, SkyCondition.Clear,        new(5, WindDirection.WSW, null),   new(PrecipitationType.None, 0.0, 0),   "06:12", "18:38"),
    ];

    private static List<DailyForecast> GenerateChicagoDaily(DateOnly start) =>
    [
        new(start,           20, 8,  SkyCondition.MostlyCloudy, new(22, WindDirection.NNW, 38.0), new(PrecipitationType.Snow, 1.2, 75),  "06:58", "17:10"),
        new(start.AddDays(1), 15, 2,  SkyCondition.Overcast,     new(25, WindDirection.N, 42.0),   new(PrecipitationType.Snow, 3.5, 95),  "06:57", "17:11"),
        new(start.AddDays(2), 12, -2, SkyCondition.Overcast,     new(20, WindDirection.NNW, 35.0), new(PrecipitationType.Snow, 2.0, 85),  "06:56", "17:12"),
        new(start.AddDays(3), 18, 5,  SkyCondition.MostlyCloudy, new(15, WindDirection.NW, 28.0),  new(PrecipitationType.Snow, 0.5, 45),  "06:55", "17:14"),
        new(start.AddDays(4), 25, 12, SkyCondition.PartlyCloudy, new(10, WindDirection.W, 18.0),   new(PrecipitationType.None, 0.0, 15),  "06:54", "17:15"),
        new(start.AddDays(5), 30, 18, SkyCondition.Clear,        new(8, WindDirection.SW, null),    new(PrecipitationType.None, 0.0, 5),   "06:53", "17:16"),
        new(start.AddDays(6), 28, 15, SkyCondition.MostlyCloudy, new(12, WindDirection.NW, 20.0),  new(PrecipitationType.Snow, 0.3, 30),  "06:52", "17:18"),
    ];

    // ───────────────────────── Hourly Forecast Generator ─────────────────────────

    private static List<HourlyForecast> GenerateHourly(
        DateTime start, double baseTemp, double range,
        SkyCondition sky, WindDirection windDir, double windSpeed,
        PrecipitationType precipType, int precipChance)
    {
        var forecasts = new List<HourlyForecast>();

        // Simulate a gentle sinusoidal temperature curve over 24 hours
        for (int h = 0; h < 24; h++)
        {
            var hour = start.AddHours(h);
            // Temperature peaks around hour 14 (2 PM), lowest around hour 5 (5 AM)
            var tempOffset = range * Math.Sin((h - 5) * Math.PI / 19.0);
            var temp = Math.Round(baseTemp + tempOffset, 1);
            var feelsLike = Math.Round(temp - (windSpeed > 15 ? 6 : windSpeed > 8 ? 3 : 1), 1);

            // Vary wind speed slightly
            var hourlyWind = Math.Round(windSpeed + (h % 3 - 1) * 1.5, 1);

            // Vary precipitation chance through the day
            var hourlyPrecipChance = Math.Clamp(precipChance + (h % 4 - 2) * 5, 0, 100);

            forecasts.Add(new HourlyForecast(
                hour,
                temp,
                feelsLike,
                sky,
                new WindInfo(hourlyWind, windDir, hourlyWind > 15 ? Math.Round(hourlyWind * 1.5, 1) : null),
                new Precipitation(precipType, precipType != PrecipitationType.None ? Math.Round(0.02 * hourlyPrecipChance / 100.0, 3) : 0.0, hourlyPrecipChance)));
        }

        return forecasts;
    }
}
