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

namespace FuturisticClockWidget.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DateTime _currentTime;
        private bool _is24HourFormat = true; // Default to 24-hour format
        private bool _isResizing = false;
        private Point _resizeStartPoint;
        private Size _resizeStartSize;
        private double _baseFontSize = 28;
        private double _baseDateFontSize = 10;
        private double _baseSmallFontSize = 8;
        
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
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFontSizes();
        }
        
        private void ResizeGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isResizing = true;
                _resizeStartPoint = e.GetPosition(this);
                _resizeStartSize = new Size(ActualWidth, ActualHeight);
                CaptureMouse();
                e.Handled = true;
            }
        }
        
        private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - _resizeStartPoint.X;
                double deltaY = currentPoint.Y - _resizeStartPoint.Y;
                
                double newWidth = Math.Max(200, _resizeStartSize.Width + deltaX);
                double newHeight = Math.Max(100, _resizeStartSize.Height + deltaY);
                
                Width = newWidth;
                Height = newHeight;
                e.Handled = true;
            }
        }
        
        private void ResizeGrip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }
        
        private void UpdateFontSizes()
        {
            // Calculate scale factors based on window size
            double widthScale = ActualWidth / 280.0; // Base width is 280
            double heightScale = ActualHeight / 140.0; // Base height is 140
            double scale = Math.Min(widthScale, heightScale);
            
            // Apply scaling to font sizes
            if (TimeTextBlock != null)
            {
                TimeTextBlock.FontSize = _baseFontSize * scale;
            }
            
            // Find and update date text blocks
            var textBlocks = FindVisualChildren<TextBlock>(this);
            foreach (var textBlock in textBlocks)
            {
                if (textBlock.Name != "TimeTextBlock")
                {
                    if (textBlock.Style == Resources["DateTextStyle"] as Style)
                    {
                        textBlock.FontSize = _baseDateFontSize * scale;
                    }
                    else if (textBlock.Style == Resources["SmallInfoStyle"] as Style)
                    {
                        textBlock.FontSize = _baseSmallFontSize * scale;
                    }
                }
            }
        }
        
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
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
                DragMove();
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
