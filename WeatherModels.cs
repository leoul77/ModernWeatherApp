using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModernWeatherApp;

public class GeoResponse
{
    [JsonPropertyName("results")]
    public List<GeoLocation>? Results { get; set; }
}

public class GeoLocation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("admin1")]
    public string Admin1 { get; set; } = string.Empty;
}

public class WeatherApiResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("current")]
    public CurrentWeatherData? Current { get; set; }

    [JsonPropertyName("daily")]
    public DailyWeatherData? Daily { get; set; }
}

public class CurrentWeatherData
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public int Humidity { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }
}

public class DailyWeatherData
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = new();

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TemperatureMax { get; set; } = new();

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TemperatureMin { get; set; } = new();
}

public record WeatherInfo(
    string City,
    string Country,
    double CurrentTemp,
    double FeelsLike,
    int Humidity,
    double WindSpeed,
    double Precipitation,
    string Condition,
    string Icon,
    bool IsDay,
    double TodayMax,
    double TodayMin,
    List<DailyForecastItem> Forecast
);

public record DailyForecastItem(
    string DayName,
    string DateText,
    double MaxTemp,
    double MinTemp,
    string Condition,
    string Icon
);
