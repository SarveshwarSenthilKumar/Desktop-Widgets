using System;

namespace WeatherWidget.Models
{
    public class WeatherData
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
        public int Pressure { get; set; }
        public DateTime LastUpdated { get; set; }
        public WeatherCondition Condition { get; set; }
    }

    public enum WeatherCondition
    {
        Clear,
        Cloudy,
        Rainy,
        Snowy,
        Stormy,
        Foggy,
        Unknown
    }
}
