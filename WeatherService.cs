using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModernWeatherApp;

public class WeatherService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public async Task<WeatherInfo?> GetWeatherByCityAsync(string city)
    {
        try
        {
            string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
            string geoJson = await _http.GetStringAsync(geoUrl);
            var geoData = JsonSerializer.Deserialize<GeoResponse>(geoJson);

            if (geoData?.Results == null || geoData.Results.Count == 0)
            {
                return null;
            }

            var loc = geoData.Results[0];
            return await GetWeatherByCoordsAsync(loc.Latitude, loc.Longitude, loc.Name, loc.Country);
        }
        catch
        {
            return null;
        }
    }

    public async Task<WeatherInfo?> GetWeatherByCoordsAsync(double lat, double lon, string cityName, string countryName)
    {
        try
        {
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(CultureInfo.InvariantCulture)}&longitude={lon.ToString(CultureInfo.InvariantCulture)}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,wind_speed_10m&daily=weather_code,temperature_2m_max,temperature_2m_min&timezone=auto";
            string json = await _http.GetStringAsync(url);
            var res = JsonSerializer.Deserialize<WeatherApiResponse>(json);

            if (res?.Current == null || res.Daily == null)
            {
                return null;
            }

            var cur = res.Current;
            var daily = res.Daily;

            bool isDay = cur.IsDay == 1;
            var (condText, icon) = InterpretWmoCode(cur.WeatherCode, isDay);

            double todayMax = daily.TemperatureMax.Count > 0 ? daily.TemperatureMax[0] : cur.Temperature;
            double todayMin = daily.TemperatureMin.Count > 0 ? daily.TemperatureMin[0] : cur.Temperature;

            var forecast = new List<DailyForecastItem>();
            int count = Math.Min(daily.Time.Count, 7);

            for (int i = 0; i < count; i++)
            {
                string rawDate = daily.Time[i];
                string dayName = "Day";
                string dateFormatted = rawDate;

                if (DateTime.TryParse(rawDate, out DateTime dt))
                {
                    dayName = i == 0 ? "Today" : dt.ToString("ddd");
                    dateFormatted = dt.ToString("MMM d");
                }

                int code = i < daily.WeatherCode.Count ? daily.WeatherCode[i] : 0;
                double max = i < daily.TemperatureMax.Count ? daily.TemperatureMax[i] : 0;
                double min = i < daily.TemperatureMin.Count ? daily.TemperatureMin[i] : 0;

                var (fCond, fIcon) = InterpretWmoCode(code, true);

                forecast.Add(new DailyForecastItem(dayName, dateFormatted, max, min, fCond, fIcon));
            }

            return new WeatherInfo(
                City: cityName,
                Country: countryName,
                CurrentTemp: cur.Temperature,
                FeelsLike: cur.ApparentTemperature,
                Humidity: cur.Humidity,
                WindSpeed: cur.WindSpeed,
                Precipitation: cur.Precipitation,
                Condition: condText,
                Icon: icon,
                IsDay: isDay,
                TodayMax: todayMax,
                TodayMin: todayMin,
                Forecast: forecast
            );
        }
        catch
        {
            return null;
        }
    }

    public static (string Condition, string Icon) InterpretWmoCode(int code, bool isDay)
    {
        return code switch
        {
            0 => (isDay ? "Sunny" : "Clear Night", isDay ? "☀️" : "🌙"),
            1 => (isDay ? "Mainly Sunny" : "Mainly Clear", isDay ? "🌤️" : "✨"),
            2 => ("Partly Cloudy", "⛅"),
            3 => ("Overcast", "☁️"),
            45 or 48 => ("Foggy", "🌫️"),
            51 or 53 or 55 => ("Drizzle", "🌦️"),
            56 or 57 => ("Freezing Drizzle", "🌨️"),
            61 or 63 => ("Rain", "🌧️"),
            65 => ("Heavy Rain", "⛈️"),
            66 or 67 => ("Freezing Rain", "🌧️❄️"),
            71 or 73 => ("Snow", "🌨️"),
            75 => ("Heavy Snow", "❄️"),
            77 => ("Snow Grains", "❄️"),
            80 or 81 or 82 => ("Rain Showers", "🌦️"),
            85 or 86 => ("Snow Showers", "🌨️"),
            95 => ("Thunderstorm", "⛈️"),
            96 or 99 => ("Severe Thunderstorm", "🌩️"),
            _ => ("Cloudy", "⛅")
        };
    }
}
