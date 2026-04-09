using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WidgetDashboard.Views
{
    public partial class CalendarWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<CalendarDay> _calendarDays = new ObservableCollection<CalendarDay>();
        private bool _highlightToday = true;
        private bool _showWeekNumbers = false;

        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            set
            {
                _calendarDays = value;
                OnPropertyChanged();
            }
        }

        public string CurrentMonthYear => DateTime.Now.ToString("MMMM yyyy");
        
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
            RefreshCalendar();
        }

        private void RefreshCalendar()
        {
            var today = DateTime.Now;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var days = new ObservableCollection<CalendarDay>();
            
            // Add empty cells for days before month starts
            for (int i = 0; i < (int)firstDayOfMonth.DayOfWeek; i++)
            {
                days.Add(new CalendarDay
                {
                    DayNumber = "",
                    WeekRow = 0,
                    WeekCol = i,
                    IsVisible = false
                });
            }
            
            // Add all days of the month
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var currentDate = new DateTime(today.Year, today.Month, day);
                var weekRow = ((int)firstDayOfMonth.DayOfWeek + day - 1) / 7 + 1;
                var weekCol = ((int)firstDayOfMonth.DayOfWeek + day - 1) % 7;
                
                var isToday = currentDate.Date == today.Date;
                var isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
                
                days.Add(new CalendarDay
                {
                    DayNumber = day.ToString(),
                    WeekRow = weekRow,
                    WeekCol = weekCol,
                    IsVisible = true,
                    IsToday = isToday,
                    IsWeekend = isWeekend
                });
            }
            
            CalendarDays = days;
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

        public int WeekRow { get; set; }
        public int WeekCol { get; set; }

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
