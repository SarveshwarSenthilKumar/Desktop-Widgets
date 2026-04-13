using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfColor = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;
using WpfApplication = System.Windows.Application;
using WpfClipboard = System.Windows.Clipboard;

namespace CalendarWidget
{
    public partial class CalendarWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<CalendarDay> _calendarDays = new ObservableCollection<CalendarDay>();
        private ObservableCollection<CalendarDay> _weekNumbers = new ObservableCollection<CalendarDay>();
        private bool _showWeekNumbers = false;
        private DateTime _currentDisplayMonth = DateTime.Now;
        private Storyboard _pulseStoryboard;
        private WpfColor _backgroundColor = WpfColor.FromArgb(20, 0, 0, 0);
        private WpfColor _baseColor = WpfColor.FromRgb(0, 212, 255);

        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            set
            {
                _calendarDays = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CalendarDay> WeekNumbers
        {
            get => _weekNumbers;
            set
            {
                _weekNumbers = value;
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
        
        public string FiscalQuarter
        {
            get
            {
                var today = DateTime.Now;
                var month = today.Month;
                
                // Fiscal year typically starts in October (Q1) or January (Q1)
                // Using standard fiscal year starting in January
                var quarter = ((month - 1) / 3) + 1;
                return $"Q{quarter}";
            }
        }
        
        public string FiscalQuarterInfo
        {
            get
            {
                var today = DateTime.Now;
                var quarter = FiscalQuarter;
                var year = today.Year;
                
                // Calculate days left in current fiscal quarter
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                var quarterEndMonth = quarterStartMonth + 2;
                var quarterEnd = new DateTime(today.Year, quarterEndMonth, DateTime.DaysInMonth(today.Year, quarterEndMonth));
                var daysLeftInQuarter = (quarterEnd - today).Days + 1;
                
                return $"{quarter} ({daysLeftInQuarter}d left)";
            }
        }
        
        public string SeasonInfo
        {
            get
            {
                var today = DateTime.Now;
                var season = GetSeason(today.Month);
                var seasonStartMonth = season switch
                {
                    "Spring" => 3,
                    "Summer" => 6,
                    "Fall" => 9,
                    "Winter" => 12,
                    _ => 1
                };
                
                // Calculate season end
                var seasonEndMonth = seasonStartMonth + 2;
                if (seasonEndMonth > 12) seasonEndMonth -= 12;
                var seasonEndYear = seasonEndMonth < seasonStartMonth ? today.Year + 1 : today.Year;
                var seasonEnd = new DateTime(seasonEndYear, seasonEndMonth, DateTime.DaysInMonth(seasonEndYear, seasonEndMonth));
                var daysLeftInSeason = (seasonEnd - today).Days + 1;
                
                return $"{season} ({daysLeftInSeason}d left)";
            }
        }
        
        public string MoonPhaseInfo
        {
            get
            {
                var today = DateTime.Now;
                var phase = CalculateMoonPhase(today);
                var phaseEmoji = phase switch
                {
                    "New Moon" => "🌑",
                    "Waxing Crescent" => "🌒",
                    "First Quarter" => "🌓",
                    "Waxing Gibbous" => "🌔",
                    "Full Moon" => "🌕",
                    "Waning Gibbous" => "🌖",
                    "Last Quarter" => "🌗",
                    "Waning Crescent" => "🌘",
                    _ => "🌑"
                };
                
                // Calculate days until next full moon
                var daysUntilNextFullMoon = CalculateDaysUntilNextFullMoon(today);
                var fullMoonInfo = daysUntilNextFullMoon <= 7 ? $" (Full in {daysUntilNextFullMoon}d)" : "";
                
                return $"{phaseEmoji} {phase}{fullMoonInfo}";
            }
        }

        public bool ShowWeekNumbers
        {
            get => _showWeekNumbers;
            set
            {
                if (_showWeekNumbers != value)
                {
                    _showWeekNumbers = value;
                    OnPropertyChanged();
                    RefreshCalendar();
                }
            }
        }
        
        public WpfColor BackgroundColor
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
        
        public WpfColor BaseColor
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
            var weekNumbers = new ObservableCollection<CalendarDay>();
            
            // Calculate the starting day (adjust for Sunday as first day)
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            
            // Start from the Sunday of the first week
            var calendarStart = firstDayOfMonth.AddDays(-startDayOfWeek);
            
            // Generate 6 weeks of calendar data
            for (int week = 0; week < 6; week++)
            {
                var weekStart = calendarStart.AddDays(week * 7);
                var weekNumber = GetWeekNumber(weekStart);
                
                // Add week number
                weekNumbers.Add(new CalendarDay
                {
                    DayNumber = "",
                    IsVisible = _showWeekNumbers,
                    WeekNumber = weekNumber,
                    Date = weekStart
                });
                
                // Add days for this week (Sunday to Saturday)
                for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
                {
                    var currentDate = weekStart.AddDays(dayOfWeek);
                    var isCurrentMonth = currentDate.Month == _currentDisplayMonth.Month && 
                                     currentDate.Year == _currentDisplayMonth.Year;
                    var isToday = currentDate.Date == today.Date;
                    var isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || 
                                   currentDate.DayOfWeek == DayOfWeek.Sunday;
                    
                    days.Add(new CalendarDay
                    {
                        DayNumber = isCurrentMonth ? currentDate.Day.ToString() : "",
                        IsVisible = isCurrentMonth,
                        IsToday = isToday,
                        IsWeekend = isWeekend,
                        WeekNumber = weekNumber,
                        Date = currentDate
                    });
                }
            }
            
            CalendarDays = days;
            WeekNumbers = weekNumbers;
            
            // Update month year display
            OnPropertyChanged(nameof(CurrentMonthYear));
        }

        private int GetWeekNumber(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
        }
        
        private int CalculateDaysUntilNextFullMoon(DateTime date)
        {
            // Approximate lunar cycle is 29.53 days
            // This is a simplified calculation
            var knownNewMoon = new DateTime(2000, 1, 6); // Known new moon date
            var daysSinceNewMoon = (date - knownNewMoon).Days;
            var lunarCycle = daysSinceNewMoon % 29;
            var daysUntilFullMoon = (15 - lunarCycle + 29) % 29;
            return daysUntilFullMoon;
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


        private void ShowWeekNumbers_Click(object sender, RoutedEventArgs e)
        {
            // Toggle handled by binding
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
                    gradient.GradientStops.Add(new GradientStop(WpfColor.FromArgb(
                        (byte)Math.Min(255, baseColor.A + 20), 
                        baseColor.R, baseColor.G, baseColor.B), 0.5));
                    gradient.GradientStops.Add(new GradientStop(WpfColor.FromArgb(
                        (byte)Math.Min(255, baseColor.A + 10), 
                        baseColor.R, baseColor.G, baseColor.B), 1));
                    
                    mainBackground.Background = gradient;
                    
                    // Update border brush with base color
                    var borderGradient = new LinearGradientBrush();
                    borderGradient.StartPoint = new System.Windows.Point(0, 0);
                    borderGradient.EndPoint = new System.Windows.Point(1, 1);
                    
                    var borderColor = _baseColor;
                    borderGradient.GradientStops.Add(new GradientStop(WpfColor.FromArgb(48, borderColor.R, borderColor.G, borderColor.B), 0));
                    borderGradient.GradientStops.Add(new GradientStop(WpfColor.FromArgb(21, borderColor.R, borderColor.G, borderColor.B), 0.5));
                    borderGradient.GradientStops.Add(new GradientStop(WpfColor.FromArgb(37, borderColor.R, borderColor.G, borderColor.B), 1));
                    
                    mainBackground.BorderBrush = borderGradient;
                }
                
                // Update text colors to match base color
                UpdateTextColors();
                
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
        
        private void UpdateTextColors()
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
                        if (textBlock.Foreground is SolidColorBrush textBrush)
                        {
                            // Use the base color directly for text
                            // Ensure full opacity for good visibility
                            var textColor = WpfColor.FromArgb(
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
                System.Diagnostics.Debug.WriteLine($"Error in UpdateTextColors: {ex.Message}");
            }
        }
        
        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.ColorDialog();
                dialog.Color = DrawingColor.FromArgb(_backgroundColor.A, _backgroundColor.R, _backgroundColor.G, _backgroundColor.B);
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = dialog.Color;
                    BackgroundColor = WpfColor.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing background color: {ex.Message}");
                System.Windows.MessageBox.Show($"Error changing background color: {ex.Message}", "Color Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ChangeBaseColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.ColorDialog();
                dialog.Color = DrawingColor.FromArgb(_baseColor.A, _baseColor.R, _baseColor.G, _baseColor.B);
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = dialog.Color;
                    BaseColor = WpfColor.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing base color: {ex.Message}");
                System.Windows.MessageBox.Show($"Error changing base color: {ex.Message}", "Color Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ResetColors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BackgroundColor = WpfColor.FromArgb(20, 0, 0, 0);
                BaseColor = WpfColor.FromRgb(0, 212, 255);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting colors: {ex.Message}");
                System.Windows.MessageBox.Show($"Error resetting colors: {ex.Message}", "Color Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            System.Diagnostics.Debug.WriteLine("CalendarWindow.OnClosed called");
            
            // Notify the wrapper that the widget was closed
            // This will trigger the WidgetClosed event
            if (Tag is CalendarWidget.CalendarWidgetWrapper wrapper)
            {
                System.Diagnostics.Debug.WriteLine("Found wrapper in Tag, calling NotifyClosed");
                wrapper.NotifyClosed();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Tag is {Tag?.GetType().Name ?? "null"}, expected CalendarWidgetWrapper");
            }
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
        
        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _currentDisplayMonth = DateTime.Now;
            RefreshCalendar();
        }
        
        private void RefreshCalendar_Click(object sender, RoutedEventArgs e)
        {
            RefreshCalendar();
        }
        
        private void CopyDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentDate = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
                WpfClipboard.SetText(currentDate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying date: {ex.Message}");
            }
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
        
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
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
    }

    public class WeekNumberToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Find the parent window to check ShowWeekNumbers property
            if (value is int weekNumber && weekNumber > 0)
            {
                // Try to find the parent CalendarWindow
                var window = WpfApplication.Current.Windows.OfType<CalendarWindow>().FirstOrDefault();
                if (window != null && window.ShowWeekNumbers)
                {
                    return new SolidColorBrush(WpfColor.FromArgb(40, 255, 107, 107)); // Semi-transparent red
                }
            }
            return System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CalendarDay : INotifyPropertyChanged
    {
        private string _dayNumber = string.Empty;
        private bool _isVisible = true;
        private bool _isToday = false;
        private bool _isWeekend = false;
        private int _weekNumber = 0;
        private DateTime _date;

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

        public int WeekNumber
        {
            get => _weekNumber;
            set
            {
                _weekNumber = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public Style DayStyle
        {
            get
            {
                // Return null - styling will be handled in XAML
                return null;
            }
        }

        public Style TextStyle
        {
            get
            {
                // Return null - styling will be handled in XAML
                return null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Additional event handlers for menu items
        private void DigitalClock_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for digital clock functionality
        }

        private void AnalogClock_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for analog clock functionality
        }

        private void CardinalMarkers_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for cardinal markers functionality
        }

        private void FullMarkers_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for full markers functionality
        }

        private void Format12Hour_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for 12-hour format functionality
        }

        private void Format24Hour_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for 24-hour format functionality
        }

        private void ShowDate_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for show date functionality
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for settings functionality
        }
    }
}
