using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FuturisticClockWidget.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DateTime _currentTime;
        private bool _is24HourFormat = true; // Default to 24-hour format
        
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
