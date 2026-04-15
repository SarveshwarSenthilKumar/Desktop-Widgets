using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shapes;
using FuturisticClockWidget.Models;
using FuturisticClockWidget.Services;

namespace FuturisticClockWidget.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DateTime _currentTime;
        private double _baseFontSize = 28.8;  // Reduced for better small size scaling
        private double _baseDateFontSize = 12; // Increased for better readability
        private double _baseSmallFontSize = 10; // Increased for better readability
        private bool _isLoadingSettings = false;
        private bool _showDate = true;
        private System.Windows.Media.Color _backgroundColor = System.Windows.Media.Color.FromArgb(20, 0, 0, 0);
        private System.Windows.Media.Color _baseColor = System.Windows.Media.Color.FromRgb(0, 212, 255);
        
        public ClockType CurrentClockType
        {
            get => SettingsManager.Current.Clock.ClockType;
            set
            {
                if (SettingsManager.Current.Clock.ClockType != value)
                {
                    SettingsManager.Current.Clock.ClockType = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAnalogMode));
                    OnPropertyChanged(nameof(IsDigitalMode));
                }
            }
        }
        
        private HourMarkerMode HourMarkerMode => SettingsManager.Current.Clock.HourMarkerMode;
        
        public bool IsAnalogMode => CurrentClockType == ClockType.Analog;
        public bool IsDigitalMode => CurrentClockType == ClockType.Digital;
        
        public bool ShowDate
        {
            get => _showDate;
            set
            {
                if (_showDate != value)
                {
                    _showDate = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public System.Windows.Media.Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    OnPropertyChanged();
                    UpdateColors();
                }
            }
        }
        
        public System.Windows.Media.Color BaseColor
        {
            get => _baseColor;
            set
            {
                if (_baseColor != value)
                {
                    _baseColor = value;
                    OnPropertyChanged();
                    UpdateColors();
                }
            }
        }
        
        public SolidColorBrush BackgroundBrush => new SolidColorBrush(_backgroundColor);
        public SolidColorBrush BaseColorBrush => new SolidColorBrush(_baseColor);
        
        public DateTime CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentDate));
                OnPropertyChanged(nameof(FormattedTime));
                OnPropertyChanged(nameof(WeekNumber));
                OnPropertyChanged(nameof(DayOfYear));
                OnPropertyChanged(nameof(TimeZoneDisplay));
                OnPropertyChanged(nameof(DaysUntilEndOfYear));
                OnPropertyChanged(nameof(WeeksElapsed));
            }
        }
        
        public DateTime CurrentDate => CurrentTime;
        
        public string FormattedTime
        {
            get
            {
                if (SettingsManager.Current.Clock.Is24HourFormat)
                {
                    return CurrentTime.ToString("HH:mm:ss");
                }
                else
                {
                    return CurrentTime.ToString("hh:mm:ss tt");
                }
            }
        }
        
        public string WeekNumber
        {
            get
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                var weekOfYear = culture.Calendar.GetWeekOfYear(CurrentTime, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
                
                // Check if window is in small mode
                if (ActualWidth < 250)
                {
                    return $"W{weekOfYear:D2}"; // Compact format for small modes
                }
                else
                {
                    return $"Week {weekOfYear}"; // Full text for larger modes
                }
            }
        }
        
        public string DayOfYear
        {
            get
            {
                return $"Day {CurrentTime.DayOfYear:D3}";
            }
        }
        
        public string TimeZoneDisplay
        {
            get
            {
                var timeZone = TimeZoneInfo.Local;
                var offset = timeZone.BaseUtcOffset;
                var offsetString = offset.Hours >= 0 ? $"+{offset.Hours:D2}" : $"{offset.Hours:D2}";
                var timeZoneName = timeZone.DisplayName.Split(' ')[0];
                return $"UTC{offsetString} {timeZoneName}";
            }
        }
        
        public string DaysUntilEndOfYear
        {
            get
            {
                var endOfYear = new DateTime(CurrentTime.Year, 12, 31);
                var daysRemaining = (endOfYear - CurrentTime.Date).Days;
                
                // Check if window is in small mode (less than 250px width)
                if (ActualWidth < 250)
                {
                    return daysRemaining.ToString(); // Just the number for small modes
                }
                else
                {
                    return $"{daysRemaining} days left"; // Full text for larger modes
                }
            }
        }
        
        public string WeeksElapsed
        {
            get
            {
                // Use the same calculation as WeekNumber for consistency
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                var weekOfYear = culture.Calendar.GetWeekOfYear(CurrentTime, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
                
                // Check if window is in small mode
                if (ActualWidth < 250)
                {
                    return $"W{weekOfYear:D2}"; // Compact format for small modes
                }
                else
                {
                    return $"Week {weekOfYear}"; // Full text for larger modes
                }
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Load settings and apply them
            LoadSettings();
            
            // Initialize timer for real-time updates
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Update every 10ms for smooth milliseconds
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Set initial time
            CurrentTime = DateTime.Now;
            
            // Initialize hour markers
            UpdateHourMarkers();
        }
        
        private void LoadSettings()
        {
            _isLoadingSettings = true;
            try
            {
                var settings = SettingsManager.Current;
                
                // Apply window settings
                Left = settings.Window.Left;
                Top = settings.Window.Top;
                Width = settings.Window.Width;
                Height = settings.Window.Height;
                Topmost = settings.Window.Topmost;
                ShowInTaskbar = settings.Window.ShowInTaskbar;
                
                // Ensure window is visible on screen
                EnsureWindowVisible();
                
                // Apply appearance settings
                ApplyAppearanceSettings(settings.Appearance);
            }
            finally
            {
                _isLoadingSettings = false;
            }
        }
        
        private void EnsureWindowVisible()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            if (Left < 0) Left = 0;
            if (Top < 0) Top = 0;
            if (Left + Width > screenWidth) Left = screenWidth - Width;
            if (Top + Height > screenHeight) Top = screenHeight - Height;
        }
        
        private void ApplyAppearanceSettings(AppearanceSettings appearance)
        {
            // Apply opacity
            Opacity = appearance.Opacity;
            
            // Apply font scale with safety checks to prevent 0 values
            double safeFontScale = Math.Max(0.1, appearance.FontScale); // Minimum 0.1 to prevent 0
            _baseFontSize = Math.Max(8.0, 28.8 * safeFontScale); // Minimum 8pt
            _baseDateFontSize = Math.Max(6.0, 12 * safeFontScale); // Minimum 6pt
            _baseSmallFontSize = Math.Max(5.0, 10 * safeFontScale); // Minimum 5pt
            
            // Update font sizes
            UpdateFontSizes();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentTime = DateTime.Now;
            UpdateAnalogClockHands();
        }
        
        private void UpdateAnalogClockHands()
        {
            if (HourHand == null || MinuteHand == null || SecondHand == null || AnalogClockCanvas == null)
                return;
                
            var now = CurrentTime;
            
            // Calculate angles (0 degrees = 12 o'clock, 90 degrees = 3 o'clock)
            double secondAngle = (now.Second * 6) - 90; // 6 degrees per second
            double minuteAngle = (now.Minute * 6) + (now.Second * 0.1) - 90; // 6 degrees per minute + smooth seconds
            double hourAngle = ((now.Hour % 12) * 30) + (now.Minute * 0.5) - 90; // 30 degrees per hour + smooth minutes
            
            // Calculate dynamic center and hand lengths based on current canvas size
            double currentClockSize = AnalogClockCanvas.Width;
            double centerX = currentClockSize / 2;
            double centerY = currentClockSize / 2;
            double scale = currentClockSize / 100.0; // Base size is now 100
            
            // Apply exponential scaling for better size range
            double exponentialScale = Math.Pow(scale, 0.8);
            
            const double baseHourLength = 25;
            const double baseMinuteLength = 32;
            const double baseSecondLength = 38;
            
            double hourHandLength = baseHourLength * exponentialScale;
            double minuteHandLength = baseMinuteLength * exponentialScale;
            double secondHandLength = baseSecondLength * exponentialScale;
            
            // Hour hand
            double hourEndX = centerX + Math.Cos(hourAngle * Math.PI / 180) * hourHandLength;
            double hourEndY = centerY + Math.Sin(hourAngle * Math.PI / 180) * hourHandLength;
            HourHand.X2 = hourEndX;
            HourHand.Y2 = hourEndY;
            
            // Minute hand
            double minuteEndX = centerX + Math.Cos(minuteAngle * Math.PI / 180) * minuteHandLength;
            double minuteEndY = centerY + Math.Sin(minuteAngle * Math.PI / 180) * minuteHandLength;
            MinuteHand.X2 = minuteEndX;
            MinuteHand.Y2 = minuteEndY;
            
            // Second hand
            double secondEndX = centerX + Math.Cos(secondAngle * Math.PI / 180) * secondHandLength;
            double secondEndY = centerY + Math.Sin(secondAngle * Math.PI / 180) * secondHandLength;
            SecondHand.X2 = secondEndX;
            SecondHand.Y2 = secondEndY;
        }
        
        private void UpdateHourMarkers()
        {
            if (HourMarkersCanvas == null)
                return;
                
            // Clear existing markers
            HourMarkersCanvas.Children.Clear();
            
            double currentClockSize = HourMarkersCanvas.Width;
            double centerX = currentClockSize / 2;
            double centerY = currentClockSize / 2;
            double scale = currentClockSize / 100.0;
            
            // Calculate marker dimensions
            double markerLength = 7 * scale; // Length of each marker
            double strokeWidth = 1.5 * scale; // Thickness of markers
            double margin = 5 * scale; // Distance from edge
            
            // Determine marker positions based on mode
            var markerPositions = HourMarkerMode == HourMarkerMode.Cardinal 
                ? new[]
                {
                    new { Angle = 0, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },    // 12 o'clock
                    new { Angle = 90, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },   // 3 o'clock
                    new { Angle = 180, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },  // 6 o'clock
                    new { Angle = 270, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true }  // 9 o'clock
                }
                : new[]
                {
                    new { Angle = 0, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },     // 12 o'clock
                    new { Angle = 30, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false },  // 1 o'clock
                    new { Angle = 60, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false },  // 2 o'clock
                    new { Angle = 90, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },     // 3 o'clock
                    new { Angle = 120, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false }, // 4 o'clock
                    new { Angle = 150, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false }, // 5 o'clock
                    new { Angle = 180, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },    // 6 o'clock
                    new { Angle = 210, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false }, // 7 o'clock
                    new { Angle = 240, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false }, // 8 o'clock
                    new { Angle = 270, StartDist = margin, EndDist = margin + markerLength, IsCardinal = true },    // 9 o'clock
                    new { Angle = 300, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false }, // 10 o'clock
                    new { Angle = 330, StartDist = margin, EndDist = margin + markerLength * 0.6, IsCardinal = false }  // 11 o'clock
                };
            
            foreach (var marker in markerPositions)
            {
                double angleRad = marker.Angle * Math.PI / 180;
                
                double startX = centerX + Math.Cos(angleRad) * (currentClockSize / 2 - marker.StartDist);
                double startY = centerY + Math.Sin(angleRad) * (currentClockSize / 2 - marker.StartDist);
                double endX = centerX + Math.Cos(angleRad) * (currentClockSize / 2 - marker.EndDist);
                double endY = centerY + Math.Sin(angleRad) * (currentClockSize / 2 - marker.EndDist);
                
                // Different styling for cardinal vs non-cardinal markers
                var line = new Line
                {
                    X1 = startX,
                    Y1 = startY,
                    X2 = endX,
                    Y2 = endY,
                    Stroke = new SolidColorBrush(Color.FromRgb(0, 212, 255)), // #00D4FF
                    StrokeThickness = marker.IsCardinal ? strokeWidth : strokeWidth * 0.7,
                    Opacity = marker.IsCardinal ? 0.9 : 0.6
                };
                
                HourMarkersCanvas.Children.Add(line);
            }
        }
        
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Show context menu on right click
            e.Handled = true;
        }
        
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                
                if (settingsWindow.ShowDialog() == true)
                {
                    // Settings were saved, reload them
                    LoadSettings();
                    
                    // Update UI elements that depend on settings
                    OnPropertyChanged(nameof(FormattedTime));
                    UpdateHourMarkers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Format12Hour_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Current.Clock.Is24HourFormat = false;
            SettingsManager.SaveSettings();
            OnPropertyChanged(nameof(FormattedTime));
        }
        
        private void Format24Hour_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Current.Clock.Is24HourFormat = true;
            SettingsManager.SaveSettings();
            OnPropertyChanged(nameof(FormattedTime));
        }
        
        private void DigitalClock_Click(object sender, RoutedEventArgs e)
        {
            CurrentClockType = ClockType.Digital;
        }
        
        private void AnalogClock_Click(object sender, RoutedEventArgs e)
        {
            CurrentClockType = ClockType.Analog;
        }
        
        private void CardinalMarkers_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Current.Clock.HourMarkerMode = HourMarkerMode.Cardinal;
            SettingsManager.SaveSettings();
            UpdateHourMarkers();
        }
        
        private void FullMarkers_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Current.Clock.HourMarkerMode = HourMarkerMode.Full;
            SettingsManager.SaveSettings();
            UpdateHourMarkers();
        }
        
        private void SizeSmall_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(200, 100, WindowSize.Small);
        }
        
        private void SizeMedium_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(280, 140, WindowSize.Medium);
        }
        
        private void SizeLarge_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(400, 200, WindowSize.Large);
        }
        
        private void SizeExtraLarge_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(600, 300, WindowSize.ExtraLarge);
        }
        
        private void ShowDate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                ShowDate = menuItem.IsChecked;
            }
        }
        
        private void SetWindowSize(double width, double height, WindowSize presetSize)
        {
            Width = width;
            Height = height;
            
            // Keep current position, adjust if needed to stay on screen
            if (Left + width > SystemParameters.PrimaryScreenWidth)
                Left = SystemParameters.PrimaryScreenWidth - width;
            if (Top + height > SystemParameters.PrimaryScreenHeight)
                Top = SystemParameters.PrimaryScreenHeight - height;
            
            // Save to settings
            SettingsManager.Current.Window.Width = width;
            SettingsManager.Current.Window.Height = height;
            SettingsManager.Current.Window.PresetSize = presetSize;
            SettingsManager.Current.Window.Left = Left;
            SettingsManager.Current.Window.Top = Top;
            SettingsManager.SaveSettings();
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isLoadingSettings)
            {
                // Save new size to settings
                SettingsManager.Current.Window.Width = Width;
                SettingsManager.Current.Window.Height = Height;
                SettingsManager.Current.Window.PresetSize = WindowSize.Custom;
                SettingsManager.SaveSettings();
            }
            UpdateFontSizes();
        }
        
        private void UpdateFontSizes()
        {
            // Safety check: ensure window dimensions are valid
            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;
                
            // Calculate scale factors based on window size
            double widthScale = ActualWidth / 280.0; // Base width is 280
            double heightScale = ActualHeight / 140.0; // Base height is 140
            double scale = Math.Min(widthScale, heightScale);
            
            // Ensure scale is never 0 or negative
            scale = Math.Max(0.1, scale); // Minimum scale to prevent 0 font sizes
            
            // Apply different scaling for different elements
            // Main time uses exponential scaling for dramatic size range
            double exponentialScale = Math.Pow(scale, 0.8);
            
            // Info elements use linear scaling for better readability at small sizes
            double infoScale = Math.Max(0.7, scale); // Minimum 70% of base size for readability
            
            // Apply scaling to font sizes for digital mode with minimum constraints
            if (TimeTextBlock != null)
            {
                double calculatedSize = _baseFontSize * exponentialScale;
                TimeTextBlock.FontSize = Math.Max(8.0, calculatedSize); // Minimum 8pt font
            }
            
            // Apply scaling to font sizes for analog mode digital display with minimum constraints
            if (AnalogTimeTextBlock != null)
            {
                double calculatedSize = 22 * exponentialScale; // Slightly larger for analog mode
                AnalogTimeTextBlock.FontSize = Math.Max(6.0, calculatedSize); // Minimum 6pt font
            }
            
            // Find and update date text blocks
            var textBlocks = FindVisualChildren<TextBlock>(this);
            foreach (var textBlock in textBlocks)
            {
                if (textBlock.Name != "TimeTextBlock" && textBlock.Name != "AnalogTimeTextBlock")
                {
                    if (textBlock.Style == Resources["DateTextStyle"] as Style)
                    {
                        // Check if this is in analog mode (smaller font) or digital mode
                        if (textBlock.FontSize > 15) // Likely digital mode
                        {
                            double calculatedSize = _baseDateFontSize * infoScale;
                            textBlock.FontSize = Math.Max(7.0, calculatedSize); // Minimum 7pt font
                        }
                        else // Likely analog mode
                        {
                            double calculatedSize = 11 * infoScale; // Better readability for analog mode
                            textBlock.FontSize = Math.Max(6.0, calculatedSize); // Minimum 6pt font
                        }
                    }
                    else if (textBlock.Style == Resources["SmallInfoStyle"] as Style)
                    {
                        // Check if this is in analog mode (smaller font) or digital mode
                        if (textBlock.FontSize > 10) // Likely digital mode
                        {
                            double calculatedSize = _baseSmallFontSize * infoScale;
                            textBlock.FontSize = Math.Max(6.0, calculatedSize); // Minimum 6pt font
                        }
                        else // Likely analog mode
                        {
                            double calculatedSize = 9 * infoScale; // Better readability for analog mode panels
                            textBlock.FontSize = Math.Max(5.0, calculatedSize); // Minimum 5pt font
                        }
                    }
                    else
                    {
                        // Handle analog panel text blocks that don't match the styles
                        if (textBlock.FontSize <= 11 && textBlock.FontSize >= 6) // Likely analog panel text
                        {
                            double calculatedSize = 7 * infoScale;
                            textBlock.FontSize = Math.Max(4.0, calculatedSize); // Minimum 4pt font
                        }
                    }
                }
            }
            
            // Scale analog clock if it exists
            if (AnalogClockCanvas != null)
            {
                double baseClockSize = 100;
                double newClockSize = baseClockSize * exponentialScale;
                AnalogClockCanvas.Width = newClockSize;
                AnalogClockCanvas.Height = newClockSize;
                
                // Update hand lengths proportionally
                const double baseHourLength = 25;
                const double baseMinuteLength = 32;
                const double baseSecondLength = 38;
                
                double centerOffset = newClockSize / 2;
                double hourLength = baseHourLength * exponentialScale;
                double minuteLength = baseMinuteLength * exponentialScale;
                double secondLength = baseSecondLength * exponentialScale;
                
                // Update hand positions
                if (HourHand != null)
                {
                    HourHand.X1 = centerOffset;
                    HourHand.Y1 = centerOffset;
                }
                if (MinuteHand != null)
                {
                    MinuteHand.X1 = centerOffset;
                    MinuteHand.Y1 = centerOffset;
                }
                if (SecondHand != null)
                {
                    SecondHand.X1 = centerOffset;
                    SecondHand.Y1 = centerOffset;
                }
                
                // Update center dot position
                var centerDot = FindName("CenterDot") as Ellipse;
                if (centerDot != null)
                {
                    Canvas.SetLeft(centerDot, centerOffset - 3);
                    Canvas.SetTop(centerDot, centerOffset - 3);
                }
                
                // Update clock face
                var clockFace = FindName("ClockFace") as Ellipse;
                if (clockFace != null)
                {
                    clockFace.Width = newClockSize;
                    clockFace.Height = newClockSize;
                }
                
                // Update hour markers canvas size and recreate markers
                if (HourMarkersCanvas != null)
                {
                    HourMarkersCanvas.Width = newClockSize;
                    HourMarkersCanvas.Height = newClockSize;
                    UpdateHourMarkers();
                }
                
                // Trigger hand recalculation with new dimensions
                UpdateAnalogClockHands();
            }
        }
        
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        
        private void UpdateColors()
        {
            try
            {
                // Update the main background
                Border mainBackground = FindName("MainBackground") as Border;
                if (mainBackground != null)
                {
                    // Create a gradient based on the background color
                    var gradient = new LinearGradientBrush();
                    gradient.StartPoint = new System.Windows.Point(0, 0);
                    gradient.EndPoint = new System.Windows.Point(1, 1);
                    
                    // Create gradient stops based on the background color
                    var baseColor = _backgroundColor;
                    gradient.GradientStops.Add(new GradientStop(baseColor, 0));
                    gradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(
                        (byte)Math.Min(255, baseColor.A + 20), 
                        baseColor.R, baseColor.G, baseColor.B), 0.5));
                    gradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(
                        (byte)Math.Min(255, baseColor.A + 10), 
                        baseColor.R, baseColor.G, baseColor.B), 1));
                    
                    mainBackground.Background = gradient;
                    
                    // Update border brush with base color
                    var borderGradient = new LinearGradientBrush();
                    borderGradient.StartPoint = new System.Windows.Point(0, 0);
                    borderGradient.EndPoint = new System.Windows.Point(1, 1);
                    
                    var borderColor = _baseColor;
                    borderGradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(48, borderColor.R, borderColor.G, borderColor.B), 0));
                    borderGradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(21, borderColor.R, borderColor.G, borderColor.B), 0.5));
                    borderGradient.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(37, borderColor.R, borderColor.G, borderColor.B), 1));
                    
                    mainBackground.BorderBrush = borderGradient;
                    
                    // Update the main border drop shadow
                    var dropShadow = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = _baseColor,
                        BlurRadius = 25,
                        ShadowDepth = 0,
                        Opacity = 0.4
                    };
                    mainBackground.Effect = dropShadow;
                }
                
                // Update text colors safely
                UpdateTextColorsSafely();
                
                // Update dynamic resources that use the base color
                OnPropertyChanged(nameof(BackgroundBrush));
                OnPropertyChanged(nameof(BaseColorBrush));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating colors: {ex.Message}");
                // Don't crash the application, just log the error
            }
        }
        
        private void UpdateTextColorsSafely()
        {
            try
            {
                // Update text shadow effects and colors by finding all TextBlock elements
                var textBlocks = FindVisualChildren<System.Windows.Controls.TextBlock>(this);
                foreach (var textBlock in textBlocks)
                {
                    try
                    {
                        // Update shadow color
                        if (textBlock.Effect is System.Windows.Media.Effects.DropShadowEffect shadowEffect)
                        {
                            shadowEffect.Color = _baseColor;
                        }
                        
                        // Update text foreground color to match base color
                        // Only update if it's not a special element that should keep its original color
                        if (textBlock.Name != "SpecialElement" && textBlock.Foreground is SolidColorBrush textBrush)
                        {
                            // Use the base color directly for text
                            // Ensure full opacity for good visibility
                            var textColor = System.Windows.Media.Color.FromArgb(
                                255, // Full opacity for text
                                _baseColor.R,
                                _baseColor.G,
                                _baseColor.B
                            );
                            textBrush.Color = textColor;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating text block: {ex.Message}");
                        // Continue with other text blocks
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateTextColorsSafely: {ex.Message}");
            }
        }
        
        
        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.ColorDialog();
                dialog.Color = System.Drawing.Color.FromArgb(_backgroundColor.A, _backgroundColor.R, _backgroundColor.G, _backgroundColor.B);
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = dialog.Color;
                    BackgroundColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing background color: {ex.Message}");
                MessageBox.Show($"Error changing background color: {ex.Message}", "Color Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ChangeBaseColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.ColorDialog();
                dialog.Color = System.Drawing.Color.FromArgb(_baseColor.A, _baseColor.R, _baseColor.G, _baseColor.B);
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = dialog.Color;
                    BaseColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing base color: {ex.Message}");
                MessageBox.Show($"Error changing base color: {ex.Message}", "Color Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Enable window dragging
            try
            {
                DragMove();
            }
            catch
            {
                // If drag fails, just ignore it
            }
        }
        
        private void ResetColors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset to default colors
                BackgroundColor = System.Windows.Media.Color.FromArgb(20, 0, 0, 0);
                BaseColor = System.Windows.Media.Color.FromRgb(0, 212, 255);
                
                // Save to settings - use the correct property names
                SettingsManager.Current.Appearance.PrimaryColor = "#00D4FF";
                SettingsManager.SaveSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting colors: {ex.Message}");
            }
        }
        
        private void OpenTimerWidget_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var timerWindow = new TimerWindow();
                
                // Position the timer widget relative to the clock widget
                timerWindow.Left = Left + Width + 20;
                timerWindow.Top = Top;
                
                // Ensure it stays on screen
                if (timerWindow.Left + timerWindow.Width > SystemParameters.PrimaryScreenWidth)
                {
                    timerWindow.Left = Left - timerWindow.Width - 20;
                }
                if (timerWindow.Left < 0)
                {
                    timerWindow.Left = 20;
                }
                
                timerWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open timer widget: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            
            if (!_isLoadingSettings)
            {
                // Save new position to settings
                SettingsManager.Current.Window.Left = Left;
                SettingsManager.Current.Window.Top = Top;
                SettingsManager.SaveSettings();
            }
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Set window to be click-through when holding Ctrl
            // This allows interaction with desktop while keeping widget visible
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Escape key to close
            if (e.Key == Key.Escape)
            {
                Close();
            }
            
            // Space to toggle topmost
            if (e.Key == Key.Space)
            {
                Topmost = !Topmost;
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected override void OnClosed(EventArgs e)
        {
            // Save final settings
            if (!_isLoadingSettings)
            {
                SettingsManager.Current.Window.Left = Left;
                SettingsManager.Current.Window.Top = Top;
                SettingsManager.Current.Window.Width = Width;
                SettingsManager.Current.Window.Height = Height;
                SettingsManager.SaveSettings();
            }
            
            // Notify the wrapper that the widget was closed
            // This will trigger the WidgetClosed event
            if (Tag != null)
            {
                // Use reflection to call the NotifyClosed method if it exists
                var wrapperType = Tag.GetType();
                var notifyMethod = wrapperType.GetMethod("NotifyClosed");
                notifyMethod?.Invoke(Tag, null);
            }
            
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
