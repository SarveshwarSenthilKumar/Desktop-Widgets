using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WeatherWidget.Models;

namespace WeatherWidget.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5";
        
        public event EventHandler<WeatherData>? WeatherDataUpdated;
        public event EventHandler<List<WeatherForecast>>? ForecastUpdated;
        public event EventHandler<string>? ErrorOccurred;

        public WeatherService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<WeatherData> GetCurrentWeatherAsync(string city, string country)
        {
            try
            {
                var location = $"{city},{country}";
                var url = $"{_baseUrl}/weather?q={location}&appid={_apiKey}&units=metric";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var weatherData = ParseCurrentWeather(json);
                
                WeatherDataUpdated?.Invoke(this, weatherData);
                return weatherData;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to get weather data: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WeatherForecast>> GetForecastAsync(string city, string country, int days = 5)
        {
            try
            {
                var location = $"{city},{country}";
                var url = $"{_baseUrl}/forecast?q={location}&appid={_apiKey}&units=metric&cnt={days * 8}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var forecast = ParseForecast(json);
                
                ForecastUpdated?.Invoke(this, forecast);
                return forecast;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to get forecast: {ex.Message}");
                throw;
            }
        }

        private WeatherData ParseCurrentWeather(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            return new WeatherData
            {
                City = root.GetProperty("name").GetString() ?? string.Empty,
                Country = root.GetProperty("sys").GetProperty("country").GetString() ?? string.Empty,
                Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                FeelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
                Description = root.GetProperty("weather")[0].GetProperty("description").GetString() ?? string.Empty,
                Icon = root.GetProperty("weather")[0].GetProperty("icon").GetString() ?? string.Empty,
                Humidity = root.GetProperty("main").GetProperty("humidity").GetDouble(),
                WindSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble(),
                WindDirection = root.GetProperty("wind").TryGetProperty("deg", out var deg) ? deg.GetDouble() : 0,
                Pressure = root.GetProperty("main").GetProperty("pressure").GetInt32(),
                LastUpdated = DateTime.UtcNow,
                Condition = ParseWeatherCondition(root.GetProperty("weather")[0].GetProperty("main").GetString() ?? string.Empty)
            };
        }

        private List<WeatherForecast> ParseForecast(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var forecasts = new List<WeatherForecast>();
            
            var dailyForecasts = new Dictionary<DateTime, List<JsonElement>>();
            
            foreach (var item in root.GetProperty("list").EnumerateArray())
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).DateTime;
                var date = dateTime.Date;
                
                if (!dailyForecasts.ContainsKey(date))
                {
                    dailyForecasts[date] = new List<JsonElement>();
                }
                dailyForecasts[date].Add(item);
            }

            foreach (var (date, items) in dailyForecasts.Take(5))
            {
                var maxTemp = items.Max(i => i.GetProperty("main").GetProperty("temp_max").GetDouble());
                var minTemp = items.Min(i => i.GetProperty("main").GetProperty("temp_min").GetDouble());
                var mainWeather = items.First(i => i.GetProperty("main").GetProperty("temp").GetDouble() >= (maxTemp + minTemp) / 2);
                
                forecasts.Add(new WeatherForecast
                {
                    Date = date,
                    MaxTemp = maxTemp,
                    MinTemp = minTemp,
                    Description = mainWeather.GetProperty("weather")[0].GetProperty("description").GetString() ?? string.Empty,
                    Icon = mainWeather.GetProperty("weather")[0].GetProperty("icon").GetString() ?? string.Empty,
                    Condition = ParseWeatherCondition(mainWeather.GetProperty("weather")[0].GetProperty("main").GetString() ?? string.Empty),
                    PrecipitationChance = mainWeather.TryGetProperty("pop", out var pop) ? pop.GetDouble() * 100 : 0
                });
            }

            return forecasts;
        }

        private WeatherCondition ParseWeatherCondition(string condition)
        {
            return condition.ToLower() switch
            {
                "clear" => WeatherCondition.Clear,
                "clouds" => WeatherCondition.Cloudy,
                "rain" => WeatherCondition.Rainy,
                "drizzle" => WeatherCondition.Rainy,
                "snow" => WeatherCondition.Snowy,
                "thunderstorm" => WeatherCondition.Stormy,
                "mist" or "fog" or "haze" => WeatherCondition.Foggy,
                _ => WeatherCondition.Unknown
            };
        }
    }
}
