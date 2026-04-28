using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WeatherWidget.Models;
using WeatherWidget.Services;

namespace WeatherWidget
{
    public partial class WeatherWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService;
        private readonly WeatherSettings _settings;
        private DispatcherTimer? _refreshTimer;
        private WeatherData? _currentWeather;
        private List<WeatherForecast>? _forecast;

        public WeatherData? CurrentWeather
        {
            get => _currentWeather;
            set
            {
                _currentWeather = value;
                OnPropertyChanged();
                if (value != null) UpdateUI();
            }
        }

        public List<WeatherForecast>? Forecast
        {
            get => _forecast;
            set
            {
                _forecast = value;
                OnPropertyChanged();
                if (value != null) UpdateForecastUI();
            }
        }

        public bool ShowForecast => _settings.ShowForecast;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public WeatherWindow()
        {
            InitializeComponent();
            _settings = LoadSettings();
            _weatherService = new WeatherService(_settings.ApiKey);
            
            InitializeTimer();
            SubscribeToEvents();
            LoadWeatherData();
        }

        private void InitializeTimer()
        {
            _refreshTimer = new DispatcherTimer();
            var interval = GetRefreshInterval(_settings.RefreshInterval);
            _refreshTimer.Interval = interval;
            _refreshTimer.Tick += OnRefreshTimerTick;
            _refreshTimer.Start();
        }

        private void SubscribeToEvents()
        {
            _weatherService.WeatherDataUpdated += (sender, weather) =>
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentWeather = weather;
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                });
            };

            _weatherService.ForecastUpdated += (sender, forecast) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Forecast = forecast;
                });
            };

            _weatherService.ErrorOccurred += (sender, error) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ShowError(error);
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                });
            };
        }

        private async void LoadWeatherData()
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                ShowSettingsDialog();
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;

            try
            {
                await Task.WhenAll(
                    _weatherService.GetCurrentWeatherAsync(_settings.City, _settings.Country),
                    _weatherService.GetForecastAsync(_settings.City, _settings.Country, _settings.ForecastDays)
                );
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load weather data: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            if (CurrentWeather == null) return;

            LocationTextBlock.Text = $"{CurrentWeather.City}, {CurrentWeather.Country}";
            TemperatureTextBlock.Text = FormatTemperature(CurrentWeather.Temperature);
            DescriptionTextBlock.Text = CurrentWeather.Description;
            FeelsLikeTextBlock.Text = FormatTemperature(CurrentWeather.FeelsLike);
            HumidityTextBlock.Text = $"{CurrentWeather.Humidity:F0}%";
            WindTextBlock.Text = FormatWindSpeed(CurrentWeather.WindSpeed);
            PressureTextBlock.Text = $"{CurrentWeather.Pressure} hPa";
            WeatherIconTextBlock.Text = GetWeatherIcon(CurrentWeather.Icon);
        }

        private void UpdateForecastUI()
        {
            ForecastItemsControl.ItemsSource = Forecast?.Take(_settings.ForecastDays);
        }

        private string FormatTemperature(double temp)
        {
            return _settings.TemperatureUnit switch
            {
                TemperatureUnit.Fahrenheit => $"{temp * 9/5 + 32:F0}°F",
                _ => $"{temp:F0}°C"
            };
        }

        private string FormatWindSpeed(double speed)
        {
            return _settings.WindSpeedUnit switch
            {
                WindSpeedUnit.Mph => $"{speed * 2.237:F0} mph",
                WindSpeedUnit.Ms => $"{speed:F0} m/s",
                _ => $"{speed * 3.6:F0} km/h"
            };
        }

        private string GetWeatherIcon(string iconCode)
        {
            return iconCode switch
            {
                "01d" or "01n" => "☀️", // Clear
                "02d" or "02n" => "⛅", // Few clouds
                "03d" or "03n" => "☁️", // Scattered clouds
                "04d" or "04n" => "☁️", // Broken clouds
                "09d" or "09n" => "🌧️", // Shower rain
                "10d" or "10n" => "🌦️", // Rain
                "11d" or "11n" => "⛈️", // Thunderstorm
                "13d" or "13n" => "❄️", // Snow
                "50d" or "50n" => "🌫️", // Mist
                _ => "🌤️" // Default
            };
        }

        private TimeSpan GetRefreshInterval(RefreshInterval interval)
        {
            return interval switch
            {
                RefreshInterval.FiveMinutes => TimeSpan.FromMinutes(5),
                RefreshInterval.FifteenMinutes => TimeSpan.FromMinutes(15),
                RefreshInterval.ThirtyMinutes => TimeSpan.FromMinutes(30),
                RefreshInterval.OneHour => TimeSpan.FromHours(1),
                RefreshInterval.TwoHours => TimeSpan.FromHours(2),
                _ => TimeSpan.FromMinutes(30)
            };
        }

        private void OnRefreshTimerTick(object? sender, EventArgs e)
        {
            LoadWeatherData();
        }

        private void ShowError(string message)
        {
            System.Windows.MessageBox.Show(this, message, "Weather Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSettingsDialog()
        {
            var settingsWindow = new WeatherSettingsWindow(_settings);
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                SaveSettings(settingsWindow.Settings);
                LoadWeatherData();
            }
        }

        private WeatherSettings LoadSettings()
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WeatherWidget",
                "settings.json"
            );

            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                return JsonSerializer.Deserialize<WeatherSettings>(json) ?? new WeatherSettings();
            }

            return new WeatherSettings();
        }

        private void SaveSettings(WeatherSettings settings)
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WeatherWidget",
                "settings.json"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = new ContextMenu();
            
            var refreshItem = new MenuItem { Header = "🔄 Refresh" };
            refreshItem.Click += (s, e) => LoadWeatherData();
            
            var settingsItem = new MenuItem { Header = "⚙️ Settings" };
            settingsItem.Click += (s, e) => ShowSettingsDialog();
            
            var closeItem = new MenuItem { Header = "❌ Close" };
            closeItem.Click += (s, e) => Close();

            contextMenu.Items.Add(refreshItem);
            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(closeItem);

            contextMenu.IsOpen = true;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsDialog();
        }

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
            
            // Notify wrapper that widget is closed
            if (Tag is WeatherWrapper wrapper)
            {
                wrapper.NotifyClosed();
            }
            
            base.OnClosed(e);
        }
    }
}
