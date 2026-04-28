using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WeatherWidget.Converters
{
    public class WeatherConditionToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WeatherWidget.Models.WeatherCondition condition)
            {
                return condition switch
                {
                    WeatherWidget.Models.WeatherCondition.Clear => "☀️",
                    WeatherWidget.Models.WeatherCondition.Cloudy => "☁️",
                    WeatherWidget.Models.WeatherCondition.Rainy => "🌧️",
                    WeatherWidget.Models.WeatherCondition.Snowy => "❄️",
                    WeatherWidget.Models.WeatherCondition.Stormy => "⛈️",
                    WeatherWidget.Models.WeatherCondition.Foggy => "🌫️",
                    _ => "🌤️"
                };
            }
            return "🌤️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
