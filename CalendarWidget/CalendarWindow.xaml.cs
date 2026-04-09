using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CalendarWidget
{
    public partial class CalendarWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<CalendarDay> _calendarDays = new ObservableCollection<CalendarDay>();
        private bool _highlightToday = true;
        private bool _showWeekNumbers = false;
        private DateTime _currentDisplayMonth = DateTime.Now;
        private Storyboard _pulseStoryboard;

        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            set
            {
                _calendarDays = value;
                OnPropertyChanged();
            }
        }

        public string CurrentMonthYear => _currentDisplayMonth.ToString("MMMM yyyy");
        
        public string CurrentDate => DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        
        public string WeekInfo
        {
            get
            {
                var today = DateTime.Now;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var weekEnd = weekStart.AddDays(6);
                return $"Week {GetWeekNumber(today)}: {weekStart:MMM dd} - {weekEnd:MMM dd}";
            }
        }
        
        public string WeekNumber => $"{GetWeekNumber(DateTime.Now)}";
        
        public string DayOfYear => $"{DateTime.Now.DayOfYear}";
        
        public string DaysUntilEndOfYear
        {
            get
            {
                var today = DateTime.Now;
                var endOfYear = new DateTime(today.Year, 12, 31);
                var daysLeft = (endOfYear - today).Days + 1;
                return $"{daysLeft}";
            }
        }
        
        public string MoonPhase
        {
            get
            {
                var today = DateTime.Now;
                var phase = CalculateMoonPhase(today);
                return phase;
            }
        }
        
        public string Season
        {
            get
            {
                var today = DateTime.Now;
                return GetSeason(today.Month);
            }
        }
        
        public string SeasonIcon
        {
            get
            {
                var month = DateTime.Now.Month;
                return month switch
                {
                    >= 3 and <= 5 => "🌸", // Spring
                    >= 6 and <= 8 => "☀️", // Summer
                    >= 9 and <= 11 => "🍂", // Fall
                    _ => "❄️" // Winter
                };
            }
        }

        public bool HighlightToday
        {
            get => _highlightToday;
            set
            {
                _highlightToday = value;
                OnPropertyChanged();
                RefreshCalendar();
            }
        }

        public bool ShowWeekNumbers
        {
            get => _showWeekNumbers;
            set
            {
                _showWeekNumbers = value;
                OnPropertyChanged();
                RefreshCalendar();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CalendarWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Initialize the pulse animation for today
            _pulseStoryboard = (Storyboard)FindResource("PulseAnimation");
            
            // Set up a timer to update the time-dependent properties
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Update every minute
            };
            timer.Tick += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentDate));
                OnPropertyChanged(nameof(WeekInfo));
                OnPropertyChanged(nameof(WeekNumber));
                OnPropertyChanged(nameof(DayOfYear));
                OnPropertyChanged(nameof(DaysUntilEndOfYear));
                OnPropertyChanged(nameof(MoonPhase));
                OnPropertyChanged(nameof(Season));
                OnPropertyChanged(nameof(SeasonIcon));
            };
            timer.Start();
            
            RefreshCalendar();
        }

        private void RefreshCalendar()
        {
            var today = DateTime.Now;
            var firstDayOfMonth = new DateTime(_currentDisplayMonth.Year, _currentDisplayMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var days = new ObservableCollection<CalendarDay>();
            
            // Add empty cells for days before month starts (to align first day correctly)
            for (int i = 0; i < (int)firstDayOfMonth.DayOfWeek; i++)
            {
                days.Add(new CalendarDay
                {
                    DayNumber = "",
                    IsVisible = false
                });
            }
            
            // Add all days of the month
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var currentDate = new DateTime(_currentDisplayMonth.Year, _currentDisplayMonth.Month, day);
                var isToday = currentDate.Date == today.Date;
                var isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
                
                days.Add(new CalendarDay
                {
                    DayNumber = day.ToString(),
                    IsVisible = true,
                    IsToday = isToday,
                    IsWeekend = isWeekend
                });
            }
            
            // Add empty cells to fill the grid (42 cells total = 6 rows × 7 columns)
            var totalCells = days.Count;
            var cellsNeeded = 42 - totalCells;
            for (int i = 0; i < cellsNeeded; i++)
            {
                days.Add(new CalendarDay
                {
                    DayNumber = "",
                    IsVisible = false
                });
            }
            
            CalendarDays = days;
            
            // Update month year display
            OnPropertyChanged(nameof(CurrentMonthYear));
        }

        private int GetWeekNumber(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving window: {ex.Message}");
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Context menu is already defined in XAML
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Handle size changes if needed
        }

        private void SizeSmall_Click(object sender, RoutedEventArgs e)
        {
            Width = 200;
            Height = 250;
        }

        private void SizeMedium_Click(object sender, RoutedEventArgs e)
        {
            Width = 280;
            Height = 320;
        }

        private void SizeLarge_Click(object sender, RoutedEventArgs e)
        {
            Width = 400;
            Height = 450;
        }

        private void HighlightToday_Click(object sender, RoutedEventArgs e)
        {
            // Toggle handled by binding
        }

        private void ShowWeekNumbers_Click(object sender, RoutedEventArgs e)
        {
            // Toggle handled by binding
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDisplayMonth = _currentDisplayMonth.AddMonths(-1);
            RefreshCalendar();
        }
        
        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDisplayMonth = _currentDisplayMonth.AddMonths(1);
            RefreshCalendar();
        }
        
        private string CalculateMoonPhase(DateTime date)
        {
            // Simple moon phase calculation (approximate)
            var knownNewMoon = new DateTime(2000, 1, 6, 18, 14, 0); // Known new moon
            var lunarCycle = 29.530588861; // Lunar cycle in days
            
            var daysSinceNewMoon = (date - knownNewMoon).TotalDays;
            var currentPhase = daysSinceNewMoon % lunarCycle;
            
            return currentPhase switch
            {
                < 1.84 => "New Moon",
                < 5.53 => "Waxing Crescent",
                < 9.22 => "First Quarter",
                < 12.91 => "Waxing Gibbous",
                < 16.61 => "Full Moon",
                < 20.30 => "Waning Gibbous",
                < 23.99 => "Last Quarter",
                < 27.68 => "Waning Crescent",
                _ => "New Moon"
            };
        }
        
        private string GetSeason(int month)
        {
            return month switch
            {
                >= 3 and <= 5 => "Spring",
                >= 6 and <= 8 => "Summer",
                >= 9 and <= 11 => "Fall",
                _ => "Winter"
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CalendarDay : INotifyPropertyChanged
    {
        private string _dayNumber = string.Empty;
        private bool _isVisible = true;
        private bool _isToday = false;
        private bool _isWeekend = false;

        public string DayNumber
        {
            get => _dayNumber;
            set
            {
                _dayNumber = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsToday
        {
            get => _isToday;
            set
            {
                _isToday = value;
                OnPropertyChanged();
            }
        }

        public bool IsWeekend
        {
            get => _isWeekend;
            set
            {
                _isWeekend = value;
                OnPropertyChanged();
            }
        }

        public Style DayStyle
        {
            get
            {
                if (IsToday && Application.Current?.Resources["TodayDayStyle"] is Style todayStyle)
                    return todayStyle;
                return Application.Current?.Resources["DayNumberStyle"] as Style;
            }
        }

        public Style TextStyle
        {
            get
            {
                if (IsWeekend && Application.Current?.Resources["WeekendDayStyle"] is Style weekendStyle)
                    return weekendStyle;
                return Application.Current?.Resources["DayNumberStyle"] as Style;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
