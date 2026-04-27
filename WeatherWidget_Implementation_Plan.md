# Weather Widget Implementation Plan

## Executive Summary

Based on comprehensive analysis of the existing codebase, this document outlines the complete implementation strategy for a Weather Widget that follows the established patterns and architecture of the Desktop Widgets ecosystem.

## Architecture Analysis

### Current Widget Architecture

The codebase follows a **modular widget architecture** with these key components:

1. **WidgetDashboard** - Main orchestrator and UI hub
2. **Individual Widget Projects** - Self-contained widget implementations
3. **Widget Wrapper Pattern** - Integration layer between dashboard and widgets
4. **Persistence Layer** - Settings and state management

### Key Architectural Patterns

#### 1. Widget Interface Pattern
```csharp
public interface IWidget : INotifyPropertyChanged
{
    string Name { get; }
    string Description { get; }
    Window WidgetWindow { get; }
    bool IsRunning { get; }
    
    event EventHandler? WidgetClosed;
    
    void Start();
    void Stop();
    void Show();
    void Hide();
    void SetPosition(double x, double y);
    void SetSize(double width, double height);
}
```

#### 2. Widget Base Class Pattern
Each widget has its own `WidgetBase` implementation that handles common functionality:
- Window lifecycle management
- Property change notifications
- Widget closed event handling

#### 3. Wrapper Pattern
Dashboard uses wrapper classes to integrate external widgets:
- `StopwatchWidgetWrapper`
- `CalendarWidgetWrapper`
- `TimerWidgetWrapper`

#### 4. Factory Registration Pattern
Widgets are registered in `WidgetManager`:
```csharp
private void RegisterWidgetTypes()
{
    RegisterWidget<ClockWidgetWrapper>();
    RegisterWidget<Models.CalendarWidgetWrapper>();
    RegisterWidget<TimerWidgetWrapper>();
    RegisterWidget<StopwatchWidgetWrapper>();
}
```

## Weather Widget Implementation Plan

### Phase 1: Project Structure Setup

#### 1.1 Create WeatherWidget Project
```
WeatherWidget/
├── WeatherWidget.csproj
├── App.xaml
├── App.xaml.cs
├── IWidget.cs
├── WidgetBase.cs
├── WeatherWrapper.cs
├── WeatherWindow.xaml
├── WeatherWindow.xaml.cs
├── Models/
│   ├── WeatherData.cs
│   ├── WeatherForecast.cs
│   └── WeatherSettings.cs
├── Services/
│   ├── WeatherService.cs
│   └── SettingsManager.cs
├── Converters/
│   └── WeatherConverters.cs
└── Resources/
    └── WeatherIcons.xaml
```

#### 1.2 Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <OutputType>WinExe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.77" />
    <PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  </ItemGroup>

</Project>
```

### Phase 2: Core Implementation

#### 2.1 Weather Data Models

```csharp
// Models/WeatherData.cs
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

// Models/WeatherForecast.cs
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

// Models/WeatherSettings.cs
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
```

#### 2.2 Weather Service

```csharp
// Services/WeatherService.cs
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
        // Parse OpenWeatherMap JSON response
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
        // Parse OpenWeatherMap forecast JSON response
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
```

#### 2.3 Weather Window UI

```xml
<!-- WeatherWindow.xaml -->
<Window x:Class="WeatherWidget.WeatherWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:WeatherWidget"
        mc:Ignorable="d"
        Title="Weather Widget" 
        Height="300" Width="400"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        Topmost="True"
        ShowInTaskbar="False"
        WindowStartupLocation="CenterScreen"
        MouseLeftButtonDown="Window_MouseLeftButtonDown"
        MouseRightButtonDown="Window_MouseRightButtonDown">
    
    <Window.Resources>
        <!-- Glassmorphism Background Style -->
        <Style x:Key="GlassBorderStyle" TargetType="Border">
            <Setter Property="Background" Value="#0A000000"/>
            <Setter Property="BorderBrush" Value="#3000D4FF"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="CornerRadius" Value="20"/>
            <Setter Property="Effect">
                <Setter.Value>
                    <DropShadowEffect Color="#00D4FF" BlurRadius="20" ShadowDepth="0" Opacity="0.4"/>
                </Setter.Value>
            </Setter>
        </Style>

        <!-- Weather Icon Style -->
        <Style x:Key="WeatherIconStyle" TargetType="TextBlock">
            <Setter Property="FontFamily" Value="Segoe MDL2 Assets"/>
            <Setter Property="FontSize" Value="48"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="Foreground" Value="#00D4FF"/>
            <Setter Property="Effect">
                <Setter.Value>
                    <DropShadowEffect Color="#00D4FF" BlurRadius="8" ShadowDepth="0" Opacity="0.6"/>
                </Setter.Value>
            </Setter>
        </Style>

        <!-- Temperature Style -->
        <Style x:Key="TemperatureStyle" TargetType="TextBlock">
            <Setter Property="FontFamily" Value="Segoe UI Light"/>
            <Setter Property="FontSize" Value="36"/>
            <Setter Property="FontWeight" Value="UltraLight"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="Foreground" Value="#FFFFFF"/>
            <Setter Property="Effect">
                <Setter.Value>
                    <DropShadowEffect Color="#00D4FF" BlurRadius="6" ShadowDepth="0" Opacity="0.4"/>
                </Setter.Value>
            </Setter>
        </Style>

        <!-- Location Style -->
        <Style x:Key="LocationStyle" TargetType="TextBlock">
            <Setter Property="FontFamily" Value="Segoe UI Light"/>
            <Setter Property="FontSize" Value="18"/>
            <Setter Property="FontWeight" Value="Light"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="Foreground" Value="#CCFFFFFF"/>
        </Style>

        <!-- Description Style -->
        <Style x:Key="DescriptionStyle" TargetType="TextBlock">
            <Setter Property="FontFamily" Value="Segoe UI Light"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="Light"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="Foreground" Value="#99FFFFFF"/>
            <Setter Property="TextTransform" Value="Capitalize"/>
        </Style>

        <!-- Details Style -->
        <Style x:Key="DetailsStyle" TargetType="TextBlock">
            <Setter Property="FontFamily" Value="Segoe UI"/>
            <Setter Property="FontSize" Value="11"/>
            <Setter Property="FontWeight" Value="Light"/>
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="Foreground" Value="#80FFFFFF"/>
        </Style>

        <!-- Boolean to Visibility Converter -->
        <BooleanToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    </Window.Resources>

    <Border Style="{StaticResource GlassBorderStyle}">
        <Grid Margin="20">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Header with Location and Settings -->
            <Grid Grid.Row="0" Margin="0,0,0,10">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <!-- Location -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" HorizontalAlignment="Left">
                    <TextBlock Text="📍" FontSize="16" VerticalAlignment="Center" Margin="0,0,5,0"/>
                    <TextBlock x:Name="LocationTextBlock" 
                               Text="Loading..." 
                               Style="{StaticResource LocationStyle}"/>
                </StackPanel>

                <!-- Settings Button -->
                <Button Grid.Column="1" 
                        x:Name="SettingsButton"
                        Content="⚙️" 
                        Background="Transparent"
                        BorderThickness="0"
                        FontSize="16"
                        Cursor="Hand"
                        Click="SettingsButton_Click"
                        ToolTip="Weather Settings"/>
            </Grid>

            <!-- Main Weather Display -->
            <Grid Grid.Row="1">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Current Weather -->
                <StackPanel Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Center">
                    <!-- Weather Icon -->
                    <TextBlock x:Name="WeatherIconTextBlock" 
                               Text="🌤️" 
                               Style="{StaticResource WeatherIconStyle}"
                               Margin="0,0,0,10"/>
                    
                    <!-- Temperature -->
                    <TextBlock x:Name="TemperatureTextBlock" 
                               Text="--°" 
                               Style="{StaticResource TemperatureStyle}"/>
                    
                    <!-- Description -->
                    <TextBlock x:Name="DescriptionTextBlock" 
                               Text="Loading..." 
                               Style="{StaticResource DescriptionStyle}"
                               Margin="0,5,0,0"/>
                </StackPanel>

                <!-- Weather Details -->
                <StackPanel Grid.Column="1" HorizontalAlignment="Center" VerticalAlignment="Center">
                    <!-- Feels Like -->
                    <StackPanel Orientation="Horizontal" Margin="0,5">
                        <TextBlock Text="🌡️" FontSize="12" VerticalAlignment="Center" Margin="0,0,5,0"/>
                        <TextBlock Text="Feels like: " Style="{StaticResource DetailsStyle}"/>
                        <TextBlock x:Name="FeelsLikeTextBlock" Text="--°" Style="{StaticResource DetailsStyle}"/>
                    </StackPanel>

                    <!-- Humidity -->
                    <StackPanel Orientation="Horizontal" Margin="0,5">
                        <TextBlock Text="💧" FontSize="12" VerticalAlignment="Center" Margin="0,0,5,0"/>
                        <TextBlock Text="Humidity: " Style="{StaticResource DetailsStyle}"/>
                        <TextBlock x:Name="HumidityTextBlock" Text="--%" Style="{StaticResource DetailsStyle}"/>
                    </StackPanel>

                    <!-- Wind -->
                    <StackPanel Orientation="Horizontal" Margin="0,5">
                        <TextBlock Text="💨" FontSize="12" VerticalAlignment="Center" Margin="0,0,5,0"/>
                        <TextBlock Text="Wind: " Style="{StaticResource DetailsStyle}"/>
                        <TextBlock x:Name="WindTextBlock" Text="-- km/h" Style="{StaticResource DetailsStyle}"/>
                    </StackPanel>

                    <!-- Pressure -->
                    <StackPanel Orientation="Horizontal" Margin="0,5">
                        <TextBlock Text="🔵" FontSize="12" VerticalAlignment="Center" Margin="0,0,5,0"/>
                        <TextBlock Text="Pressure: " Style="{StaticResource DetailsStyle}"/>
                        <TextBlock x:Name="PressureTextBlock" Text="-- hPa" Style="{StaticResource DetailsStyle}"/>
                    </StackPanel>
                </StackPanel>
            </Grid>

            <!-- Forecast Section (Collapsible) -->
            <Expander Grid.Row="2" 
                      x:Name="ForecastExpander"
                      Header="5-Day Forecast"
                      Background="Transparent"
                      BorderBrush="#4000D4FF"
                      Foreground="#CCFFFFFF"
                      Margin="0,10,0,0"
                      Visibility="{Binding ShowForecast, Converter={StaticResource BoolToVisibilityConverter}}">
                <Expander.HeaderTemplate>
                    <DataTemplate>
                        <TextBlock Text="5-Day Forecast" FontFamily="Segoe UI Light" FontSize="14"/>
                    </DataTemplate>
                </Expander.HeaderTemplate>
                
                <ItemsControl x:Name="ForecastItemsControl" Margin="0,10,0,0">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Background="#0A000000" 
                                    CornerRadius="8" 
                                    Margin="5,2"
                                    Padding="10,5">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>

                                    <!-- Date -->
                                    <TextBlock Grid.Column="0" 
                                               Text="{Binding Date, StringFormat='{}{0:ddd}'}"
                                               Style="{StaticResource DetailsStyle}"
                                               MinWidth="40"/>

                                    <!-- Icon -->
                                    <TextBlock Grid.Column="1" 
                                               Text="{Binding Icon}"
                                               FontFamily="Segoe MDL2 Assets"
                                               FontSize="20"
                                               Foreground="#00D4FF"
                                               HorizontalAlignment="Center"
                                               Margin="10,0"/>

                                    <!-- High/Low -->
                                    <StackPanel Grid.Column="2" Orientation="Horizontal" HorizontalAlignment="Center">
                                        <TextBlock Text="{Binding MaxTemp, StringFormat='{}{0}°'}" 
                                                   Foreground="#FFFFFF" 
                                                   FontSize="12" 
                                                   VerticalAlignment="Center"/>
                                        <TextBlock Text="/" 
                                                   Foreground="#66FFFFFF" 
                                                   FontSize="12" 
                                                   Margin="2,0"
                                                   VerticalAlignment="Center"/>
                                        <TextBlock Text="{Binding MinTemp, StringFormat='{}{0}°'}" 
                                                   Foreground="#99FFFFFF" 
                                                   FontSize="12" 
                                                   VerticalAlignment="Center"/>
                                    </StackPanel>

                                    <!-- Precipitation Chance -->
                                    <StackPanel Grid.Column="3" Orientation="Horizontal" HorizontalAlignment="Right">
                                        <TextBlock Text="💧" FontSize="10" VerticalAlignment="Center" Margin="0,0,3,0"/>
                                        <TextBlock Text="{Binding PrecipitationChance, StringFormat='{}{0}%'}" 
                                                   Style="{StaticResource DetailsStyle}"/>
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </Expander>

            <!-- Loading Overlay -->
            <Grid Grid.Row="0" Grid.RowSpan="3" 
                  Background="#80000000" 
                  x:Name="LoadingOverlay"
                  Visibility="Collapsed">
                <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
                    <TextBlock Text="🌤️" FontSize="48" HorizontalAlignment="Center" Margin="0,0,0,10">
                        <TextBlock.Effect>
                            <DropShadowEffect Color="#00D4FF" BlurRadius="10" ShadowDepth="0" Opacity="0.8"/>
                        </TextBlock.Effect>
                    </TextBlock>
                    <TextBlock Text="Loading weather data..." 
                               FontFamily="Segoe UI Light" 
                               FontSize="16" 
                               Foreground="#FFFFFF" 
                               HorizontalAlignment="Center"/>
                </StackPanel>
            </Grid>
        </Grid>
    </Border>
</Window>
```

#### 2.4 Weather Window Code-Behind

```csharp
// WeatherWindow.xaml.cs
public partial class WeatherWindow : Window, INotifyPropertyChanged
{
    private readonly WeatherService _weatherService;
    private readonly WeatherSettings _settings;
    private DispatcherTimer _refreshTimer;
    private WeatherData _currentWeather;
    private List<WeatherForecast> _forecast;

    public WeatherData CurrentWeather
    {
        get => _currentWeather;
        set
        {
            _currentWeather = value;
            OnPropertyChanged();
            UpdateUI();
        }
    }

    public List<WeatherForecast> Forecast
    {
        get => _forecast;
        set
        {
            _forecast = value;
            OnPropertyChanged();
            UpdateForecastUI();
        }
    }

    public bool ShowForecast => _settings.ShowForecast;

    public event PropertyChangedEventHandler? PropertyChanged;

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
        MessageBox.Show(this, message, "Weather Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        // Load settings from file or return defaults
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
```

### Phase 3: Widget Integration

#### 3.1 Weather Widget Wrapper

```csharp
// WeatherWrapper.cs
public class WeatherWrapper : WidgetBase
{
    private static int _instanceCount = 0;
    private readonly int _instanceId;
    private readonly string _uniqueId;

    public override string Name => $"Weather Widget";
    public override string Description => "Real-time weather information with forecast";
    
    public string WidgetId => $"Weather_{_instanceId}_{_uniqueId}";

    public WeatherWrapper()
    {
        _instanceCount++;
        _instanceId = _instanceCount;
        _uniqueId = Guid.NewGuid().ToString("N")[..8];
    }

    protected override Window CreateWidgetWindow()
    {
        var weatherWindow = new WeatherWindow();
        weatherWindow.Title = $"Weather {_instanceId}-{_uniqueId}";
        weatherWindow.Tag = this; // Set reference back to wrapper
        return weatherWindow;
    }

    public override void SetSize(double width, double height)
    {
        base.SetSize(width, height);
        
        if (_widgetWindow is WeatherWindow weatherWindow)
        {
            weatherWindow.Width = width;
            weatherWindow.Height = height;
        }
    }
    
    public void NotifyClosed()
    {
        NotifyWidgetClosed();
    }
}
```

#### 3.2 Dashboard Integration

```csharp
// In WidgetDashboard/Models/WidgetManager.cs
private void RegisterWidgetTypes()
{
    RegisterWidget<ClockWidgetWrapper>();
    RegisterWidget<Models.CalendarWidgetWrapper>();
    RegisterWidget<TimerWidgetWrapper>();
    RegisterWidget<StopwatchWidgetWrapper>();
    RegisterWidget<WeatherWidgetWrapper>(); // Add this line
}
```

```xml
<!-- In WidgetDashboard/WidgetDashboard.csproj -->
<ItemGroup>
    <ProjectReference Include="..\FuturisticClockWidget\FuturisticClockWidget.csproj" />
    <ProjectReference Include="..\CalendarWidget\CalendarWidget.csproj" />
    <ProjectReference Include="..\StopwatchWidget\StopwatchWidget.csproj" />
    <ProjectReference Include="..\WeatherWidget\WeatherWidget.csproj" /> <!-- Add this line -->
</ItemGroup>
```

### Phase 4: Advanced Features

#### 4.1 Weather Settings Window

```csharp
// WeatherSettingsWindow.xaml.cs
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
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ValidateSettings())
        {
            DialogResult = true;
            Close();
        }
    }

    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(Settings.City))
        {
            MessageBox.Show("Please enter a city name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Settings.ApiKey))
        {
            MessageBox.Show("Please enter a valid API key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
```

#### 4.2 Notifications Service

```csharp
// Services/NotificationService.cs
public class NotificationService
{
    public void ShowWeatherNotification(string title, string message, string icon = "🌤️")
    {
        var notification = new NotifyIcon
        {
            Visible = true,
            Icon = SystemIcons.Application,
            BalloonTipTitle = title,
            BalloonTipText = message,
            BalloonTipIcon = ToolTipIcon.Info
        };

        notification.ShowBalloonTip(5000);
        
        // Auto-hide after 5 seconds
        Task.Delay(5000).ContinueWith(_ => 
        {
            notification.Visible = false;
            notification.Dispose();
        });
    }
}
```

### Phase 5: Deployment & Testing

#### 5.1 Build Configuration

```xml
<!-- WeatherWidget.csproj additions for deployment -->
<PropertyGroup>
    <AssemblyTitle>Weather Widget</AssemblyTitle>
    <AssemblyDescription>Real-time weather widget for desktop dashboard</AssemblyDescription>
    <AssemblyCompany>Desktop Widgets</AssemblyCompany>
    <AssemblyProduct>Weather Widget</AssemblyProduct>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
</PropertyGroup>
```

#### 5.2 Testing Strategy

1. **Unit Tests**: Test WeatherService parsing logic
2. **Integration Tests**: Test API connectivity and data flow
3. **UI Tests**: Test window interactions and settings
4. **Performance Tests**: Test refresh intervals and memory usage

#### 5.3 Error Handling

- Network connectivity issues
- Invalid API keys
- Rate limiting
- Invalid location data
- JSON parsing errors

## Implementation Timeline

### Week 1: Foundation
- [ ] Create project structure
- [ ] Implement data models
- [ ] Set up WeatherService
- [ ] Basic API integration

### Week 2: UI Development
- [ ] Design WeatherWindow XAML
- [ ] Implement code-behind logic
- [ ] Add weather icon mapping
- [ ] Create settings window

### Week 3: Integration & Features
- [ ] Implement widget wrapper
- [ ] Dashboard integration
- [ ] Add forecast functionality
- [ ] Implement notifications

### Week 4: Polish & Testing
- [ ] Error handling improvements
- [ ] Performance optimization
- [ ] UI polish and animations
- [ ] Comprehensive testing

## API Integration Notes

### OpenWeatherMap API
- **Free Tier**: 1,000 calls/day, 60 calls/minute
- **Current Weather**: `/weather` endpoint
- **5-Day Forecast**: `/forecast` endpoint
- **Required Parameters**: `q` (location), `appid` (API key)
- **Optional Parameters**: `units` (metric/imperial), `lang` (language)

### Alternative APIs
- **WeatherAPI**: Free tier 1M calls/month
- **AccuWeather**: Free tier 50 calls/day
- **Visual Crossing**: Free tier 1,000 calls/day

## Security Considerations

1. **API Key Protection**: Store in encrypted settings
2. **Network Security**: Use HTTPS for all API calls
3. **Input Validation**: Sanitize user input for location
4. **Rate Limiting**: Implement client-side rate limiting
5. **Error Handling**: Don't expose sensitive information in error messages

## Performance Optimizations

1. **Caching**: Cache weather data for 10-15 minutes
2. **Async Operations**: Use async/await for all network calls
3. **Memory Management**: Proper disposal of HttpClient and timers
4. **UI Threading**: Use Dispatcher.Invoke for UI updates
5. **Resource Management**: Efficient icon and image handling

## Conclusion

This implementation plan provides a comprehensive roadmap for creating a Weather Widget that seamlessly integrates with the existing Desktop Widgets ecosystem. The widget will follow established patterns, provide rich functionality, and deliver a polished user experience that matches the quality of existing widgets.

The modular design allows for easy extension and maintenance, while the comprehensive feature set ensures users have access to all essential weather information in an elegant, desktop-friendly format.
