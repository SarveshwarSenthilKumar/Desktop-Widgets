namespace WeatherWidget.Models
{
    public class WeatherSettings
    {
        public string City { get; set; } = "New York";
        public string Country { get; set; } = "US";
        public string ApiKey { get; set; } = string.Empty;
        public TemperatureUnit TemperatureUnit { get; set; } = TemperatureUnit.Celsius;
        public WindSpeedUnit WindSpeedUnit { get; set; } = WindSpeedUnit.Kph;
        public bool ShowForecast { get; set; } = true;
        public int ForecastDays { get; set; } = 5;
        public RefreshInterval RefreshInterval { get; set; } = RefreshInterval.ThirtyMinutes;
        public bool EnableNotifications { get; set; } = false;
        public double NotificationThreshold { get; set; } = 5.0;
    }

    public enum TemperatureUnit
    {
        Celsius,
        Fahrenheit
    }

    public enum WindSpeedUnit
    {
        Kph,
        Mph,
        Ms
    }

    public enum RefreshInterval
    {
        FiveMinutes,
        FifteenMinutes,
        ThirtyMinutes,
        OneHour,
        TwoHours
    }
}
