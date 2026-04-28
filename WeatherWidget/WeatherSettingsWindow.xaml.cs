using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeatherWidget.Models;

namespace WeatherWidget
{
    public partial class WeatherSettingsWindow : Window
    {
        public WeatherSettings Settings { get; private set; }

        public WeatherSettingsWindow(WeatherSettings currentSettings)
        {
            InitializeComponent();
            Settings = new WeatherSettings
            {
                City = currentSettings.City,
                Country = currentSettings.Country,
                ApiKey = currentSettings.ApiKey,
                TemperatureUnit = currentSettings.TemperatureUnit,
                WindSpeedUnit = currentSettings.WindSpeedUnit,
                ShowForecast = currentSettings.ShowForecast,
                ForecastDays = currentSettings.ForecastDays,
                RefreshInterval = currentSettings.RefreshInterval,
                EnableNotifications = currentSettings.EnableNotifications,
                NotificationThreshold = currentSettings.NotificationThreshold
            };
            
            DataContext = this;
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Initialize Temperature Unit ComboBox
            TemperatureUnitComboBox.SelectedIndex = Settings.TemperatureUnit == TemperatureUnit.Fahrenheit ? 1 : 0;
            
            // Initialize Wind Speed Unit ComboBox
            WindSpeedUnitComboBox.SelectedIndex = Settings.WindSpeedUnit switch
            {
                WindSpeedUnit.Mph => 1,
                WindSpeedUnit.Ms => 2,
                _ => 0
            };
            
            // Initialize Forecast Days ComboBox
            var forecastDaysIndex = Settings.ForecastDays switch
            {
                3 => 0,
                5 => 1,
                7 => 2,
                _ => 1
            };
            ForecastDaysComboBox.SelectedIndex = forecastDaysIndex;
            
            // Initialize Refresh Interval ComboBox
            RefreshIntervalComboBox.SelectedIndex = Settings.RefreshInterval switch
            {
                RefreshInterval.FiveMinutes => 0,
                RefreshInterval.FifteenMinutes => 1,
                RefreshInterval.ThirtyMinutes => 2,
                RefreshInterval.OneHour => 3,
                RefreshInterval.TwoHours => 4,
                _ => 2
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSettings())
            {
                // Update settings from UI
                Settings.City = CityTextBox.Text.Trim();
                Settings.Country = CountryTextBox.Text.Trim();
                Settings.ApiKey = ApiKeyTextBox.Text.Trim();
                
                Settings.TemperatureUnit = TemperatureUnitComboBox.SelectedIndex == 1 ? TemperatureUnit.Fahrenheit : TemperatureUnit.Celsius;
                Settings.WindSpeedUnit = WindSpeedUnitComboBox.SelectedIndex switch
                {
                    1 => WindSpeedUnit.Mph,
                    2 => WindSpeedUnit.Ms,
                    _ => WindSpeedUnit.Kph
                };
                
                Settings.ShowForecast = EnableNotificationsCheckBox.IsChecked == true;
                Settings.ForecastDays = int.Parse(((ComboBoxItem)ForecastDaysComboBox.SelectedItem).Content.ToString()!);
                
                Settings.RefreshInterval = RefreshIntervalComboBox.SelectedIndex switch
                {
                    0 => RefreshInterval.FiveMinutes,
                    1 => RefreshInterval.FifteenMinutes,
                    2 => RefreshInterval.ThirtyMinutes,
                    3 => RefreshInterval.OneHour,
                    4 => RefreshInterval.TwoHours,
                    _ => RefreshInterval.ThirtyMinutes
                };
                
                Settings.EnableNotifications = EnableNotificationsCheckBox.IsChecked == true;
                
                if (double.TryParse(NotificationThresholdTextBox.Text, out var threshold))
                {
                    Settings.NotificationThreshold = threshold;
                }
                
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(Settings.City))
            {
                System.Windows.MessageBox.Show("Please enter a city name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CityTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Settings.Country))
            {
                System.Windows.MessageBox.Show("Please enter a country code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CountryTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                System.Windows.MessageBox.Show("Please enter a valid OpenWeatherMap API key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ApiKeyTextBox.Focus();
                return false;
            }

            if (Settings.ApiKey.Length < 10)
            {
                System.Windows.MessageBox.Show("API key appears to be invalid. Please check your OpenWeatherMap API key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ApiKeyTextBox.Focus();
                return false;
            }

            if (!double.TryParse(NotificationThresholdTextBox.Text, out var threshold) || threshold < -50 || threshold > 50)
            {
                System.Windows.MessageBox.Show("Please enter a valid notification threshold between -50 and 50.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NotificationThresholdTextBox.Focus();
                return false;
            }

            return true;
        }
    }
}
