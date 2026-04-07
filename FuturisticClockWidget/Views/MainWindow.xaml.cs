using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;

namespace FuturisticClockWidget.Views
{
    public enum ClockType
    {
        Digital,
        Analog
    }
    
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DateTime _currentTime;
        private bool _is24HourFormat = true; // Default to 24-hour format
        private double _baseFontSize = 28.8;  // Reduced for better small size scaling
        private double _baseDateFontSize = 12; // Increased for better readability
        private double _baseSmallFontSize = 10; // Increased for better readability
        private ClockType _clockType = ClockType.Digital;
        
        public ClockType CurrentClockType
        {
            get => _clockType;
            set
            {
                _clockType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAnalogMode));
                OnPropertyChanged(nameof(IsDigitalMode));
            }
        }
        
        public bool IsAnalogMode => CurrentClockType == ClockType.Analog;
        public bool IsDigitalMode => CurrentClockType == ClockType.Digital;
        
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
                if (_is24HourFormat)
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
                var calendar = culture.Calendar;
                var weekRule = culture.DateTimeFormat.CalendarWeekRule;
                var weekOfYear = calendar.GetWeekOfYear(CurrentTime, weekRule, culture.DateTimeFormat.FirstDayOfWeek);
                return $"W{weekOfYear:D2}";
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
                return $"{daysRemaining} days left";
            }
        }
        
        public string WeeksElapsed
        {
            get
            {
                var startOfYear = new DateTime(CurrentTime.Year, 1, 1);
                var weeksPassed = (int)Math.Floor((CurrentTime - startOfYear).TotalDays / 7.0);
                return $"Week {weeksPassed + 1}";
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Initialize timer for real-time updates
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Update every 10ms for smooth milliseconds
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Set initial time
            CurrentTime = DateTime.Now;
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
            double scale = currentClockSize / 140.0; // Base size is now 140
            
            // Apply exponential scaling for better size range
            double exponentialScale = Math.Pow(scale, 0.8);
            
            const double baseHourLength = 35;
            const double baseMinuteLength = 45;
            const double baseSecondLength = 52;
            
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
        
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Show context menu on right click
            e.Handled = true;
        }
        
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Format12Hour_Click(object sender, RoutedEventArgs e)
        {
            _is24HourFormat = false;
            OnPropertyChanged(nameof(FormattedTime));
        }
        
        private void Format24Hour_Click(object sender, RoutedEventArgs e)
        {
            _is24HourFormat = true;
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
        
        private void SizeSmall_Click(object sender, RoutedEventArgs e)
        {
            Width = 200;
            Height = 100;
            // Keep current position, adjust if needed to stay on screen
            if (Left + 200 > SystemParameters.PrimaryScreenWidth)
                Left = SystemParameters.PrimaryScreenWidth - 200;
            if (Top + 100 > SystemParameters.PrimaryScreenHeight)
                Top = SystemParameters.PrimaryScreenHeight - 100;
        }
        
        private void SizeMedium_Click(object sender, RoutedEventArgs e)
        {
            Width = 280;
            Height = 140;
            // Keep current position, adjust if needed to stay on screen
            if (Left + 280 > SystemParameters.PrimaryScreenWidth)
                Left = SystemParameters.PrimaryScreenWidth - 280;
            if (Top + 140 > SystemParameters.PrimaryScreenHeight)
                Top = SystemParameters.PrimaryScreenHeight - 140;
        }
        
        private void SizeLarge_Click(object sender, RoutedEventArgs e)
        {
            Width = 400;
            Height = 200;
            // Keep current position, adjust if needed to stay on screen
            if (Left + 400 > SystemParameters.PrimaryScreenWidth)
                Left = SystemParameters.PrimaryScreenWidth - 400;
            if (Top + 200 > SystemParameters.PrimaryScreenHeight)
                Top = SystemParameters.PrimaryScreenHeight - 200;
        }
        
        private void SizeExtraLarge_Click(object sender, RoutedEventArgs e)
        {
            Width = 600;
            Height = 300;
            // Keep current position, adjust if needed to stay on screen
            if (Left + 600 > SystemParameters.PrimaryScreenWidth)
                Left = SystemParameters.PrimaryScreenWidth - 600;
            if (Top + 300 > SystemParameters.PrimaryScreenHeight)
                Top = SystemParameters.PrimaryScreenHeight - 300;
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFontSizes();
        }
        
        private void UpdateFontSizes()
        {
            // Calculate scale factors based on window size
            double widthScale = ActualWidth / 280.0; // Base width is 280
            double heightScale = ActualHeight / 140.0; // Base height is 140
            double scale = Math.Min(widthScale, heightScale);
            
            // Apply different scaling for different elements
            // Main time uses exponential scaling for dramatic size range
            double exponentialScale = Math.Pow(scale, 0.8);
            
            // Info elements use linear scaling for better readability at small sizes
            double infoScale = Math.Max(0.7, scale); // Minimum 70% of base size for readability
            
            // Apply scaling to font sizes for digital mode
            if (TimeTextBlock != null)
            {
                TimeTextBlock.FontSize = _baseFontSize * exponentialScale;
            }
            
            // Apply scaling to font sizes for analog mode digital display
            if (AnalogTimeTextBlock != null)
            {
                AnalogTimeTextBlock.FontSize = 22 * exponentialScale; // Slightly larger for analog mode
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
                            textBlock.FontSize = _baseDateFontSize * infoScale;
                        }
                        else // Likely analog mode
                        {
                            textBlock.FontSize = 11 * infoScale; // Better readability for analog mode
                        }
                    }
                    else if (textBlock.Style == Resources["SmallInfoStyle"] as Style)
                    {
                        // Check if this is in analog mode (smaller font) or digital mode
                        if (textBlock.FontSize > 10) // Likely digital mode
                        {
                            textBlock.FontSize = _baseSmallFontSize * infoScale;
                        }
                        else // Likely analog mode
                        {
                            textBlock.FontSize = 9 * infoScale; // Better readability for analog mode panels
                        }
                    }
                    else
                    {
                        // Handle analog panel text blocks that don't match the styles
                        if (textBlock.FontSize <= 11 && textBlock.FontSize >= 8) // Likely analog panel text
                        {
                            textBlock.FontSize = 9 * infoScale;
                        }
                    }
                }
            }
            
            // Scale analog clock if it exists
            if (AnalogClockCanvas != null)
            {
                double baseClockSize = 140;
                double newClockSize = baseClockSize * exponentialScale;
                AnalogClockCanvas.Width = newClockSize;
                AnalogClockCanvas.Height = newClockSize;
                
                // Update hand lengths proportionally
                const double baseHourLength = 35;
                const double baseMinuteLength = 45;
                const double baseSecondLength = 52;
                
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
                    Canvas.SetLeft(centerDot, centerOffset - 5);
                    Canvas.SetTop(centerDot, centerOffset - 5);
                }
                
                // Update clock face
                var clockFace = FindName("ClockFace") as Ellipse;
                if (clockFace != null)
                {
                    clockFace.Width = newClockSize;
                    clockFace.Height = newClockSize;
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
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
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
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
