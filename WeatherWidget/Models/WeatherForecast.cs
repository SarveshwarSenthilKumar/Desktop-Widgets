using System;

namespace WeatherWidget.Models
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public double MaxTemp { get; set; }
        public double MinTemp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public WeatherCondition Condition { get; set; }
        public double PrecipitationChance { get; set; }
    }
}
