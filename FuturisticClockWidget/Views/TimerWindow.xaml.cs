using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FuturisticClockWidget.Views
{
    public partial class TimerWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private TimeSpan _remainingTime;
        private TimeSpan _initialTime;
        private bool _isRunning;
        private TimerState _timerState;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedTime));
                    OnPropertyChanged(nameof(TimerStatus));
                }
            }
        }

        public string FormattedTime
        {
            get
            {
                if (RemainingTime.TotalSeconds <= 0)
                    return "00:00:00";
                
                var hours = (int)Math.Floor(RemainingTime.TotalHours);
                var minutes = RemainingTime.Minutes;
                var seconds = RemainingTime.Seconds;
                
                if (hours > 0)
                    return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                else
                    return $"{minutes:D2}:{seconds:D2}";
            }
        }

        public string TimerStatus
        {
            get
            {
                return _timerState switch
                {
                    TimerState.Stopped => "Ready",
                    TimerState.Running => "Running",
                    TimerState.Paused => "Paused",
                    TimerState.Completed => "Time's Up!",
                    _ => "Ready"
                };
            }
        }

        public TimerWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Initialize with 5 minutes
            _initialTime = TimeSpan.FromMinutes(5);
            RemainingTime = _initialTime;
            _timerState = TimerState.Stopped;
            
            // Initialize timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Update every 100ms for smooth display
            };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_isRunning && RemainingTime.TotalSeconds > 0)
            {
                RemainingTime = RemainingTime.Subtract(TimeSpan.FromMilliseconds(100));
                
                if (RemainingTime.TotalSeconds <= 0)
                {
                    RemainingTime = TimeSpan.Zero;
                    _isRunning = false;
                    _timerState = TimerState.Completed;
                    _timer.Stop();
                    
                    // Update UI
                    OnPropertyChanged(nameof(TimerStatus));
                    UpdateButtonStates();
                    
                    // Show notification
                    ShowTimerCompleteNotification();
                }
            }
        }

        private void StartPause_Click(object sender, RoutedEventArgs e)
        {
            switch (_timerState)
            {
                case TimerState.Stopped:
                case TimerState.Paused:
                    StartTimer();
                    break;
                case TimerState.Running:
                    PauseTimer();
                    break;
                case TimerState.Completed:
                    ResetTimer();
                    StartTimer();
                    break;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            ShowTimerDialog();
        }

        private void StartTimer()
        {
            _isRunning = true;
            _timerState = TimerState.Running;
            _timer.Start();
            
            OnPropertyChanged(nameof(TimerStatus));
            UpdateButtonStates();
        }

        private void PauseTimer()
        {
            _isRunning = false;
            _timerState = TimerState.Paused;
            _timer.Stop();
            
            OnPropertyChanged(nameof(TimerStatus));
            UpdateButtonStates();
        }

        private void ResetTimer()
        {
            _isRunning = false;
            _timerState = TimerState.Stopped;
            _timer.Stop();
            RemainingTime = _initialTime;
            
            OnPropertyChanged(nameof(TimerStatus));
            UpdateButtonStates();
        }

        private void ShowTimerDialog()
        {
            var dialog = new TimerInputDialog(_initialTime);
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                _initialTime = dialog.SelectedTime;
                ResetTimer();
            }
        }

        private void UpdateButtonStates()
        {
            if (StartPauseButton != null && ResetButton != null && SetButton != null)
            {
                switch (_timerState)
                {
                    case TimerState.Stopped:
                        StartPauseButton.Content = "▶️ Start";
                        ResetButton.IsEnabled = false;
                        SetButton.IsEnabled = true;
                        break;
                    case TimerState.Running:
                        StartPauseButton.Content = "⏸️ Pause";
                        ResetButton.IsEnabled = true;
                        SetButton.IsEnabled = false;
                        break;
                    case TimerState.Paused:
                        StartPauseButton.Content = "▶️ Resume";
                        ResetButton.IsEnabled = true;
                        SetButton.IsEnabled = false;
                        break;
                    case TimerState.Completed:
                        StartPauseButton.Content = "🔄 Restart";
                        ResetButton.IsEnabled = true;
                        SetButton.IsEnabled = true;
                        break;
                }
            }
        }

        private void ShowTimerCompleteNotification()
        {
            try
            {
                // Flash the window with cyan color
                var originalBackground = MainBackground.Background;
                var flashBrush = new SolidColorBrush(Color.FromRgb(0, 212, 255));
                MainBackground.Background = flashBrush;
                
                var flashTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(200)
                };
                flashTimer.Tick += (s, e) =>
                {
                    MainBackground.Background = originalBackground;
                    flashTimer.Stop();
                };
                flashTimer.Start();
                
                // Show message box (you could replace this with a custom notification)
                MessageBox.Show("Timer completed!", "Timer", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing timer notification: {ex.Message}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFontSizes();
        }

        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void SetTimer_Click(object sender, RoutedEventArgs e)
        {
            ShowTimerDialog();
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SizeSmall_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(200, 100);
        }

        private void SizeMedium_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(280, 120);
        }

        private void SizeLarge_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(400, 180);
        }

        private void SizeExtraLarge_Click(object sender, RoutedEventArgs e)
        {
            SetWindowSize(600, 240);
        }

        private void SetWindowSize(double width, double height)
        {
            Width = width;
            Height = height;
            
            // Keep current position, adjust if needed to stay on screen
            if (Left + width > SystemParameters.PrimaryScreenWidth)
                Left = SystemParameters.PrimaryScreenWidth - width;
            if (Top + height > SystemParameters.PrimaryScreenHeight)
                Top = SystemParameters.PrimaryScreenHeight - height;
            
            // Update font sizes based on new window size
            UpdateFontSizes();
        }

        private void UpdateFontSizes()
        {
            // Safety check: ensure window dimensions are valid
            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;
                
            // Calculate scale factors based on window size
            double widthScale = ActualWidth / 280.0; // Base width is 280
            double heightScale = ActualHeight / 120.0; // Base height is 120
            double scale = Math.Min(widthScale, heightScale);
            
            // Ensure scale is never 0 or negative
            scale = Math.Max(0.1, scale); // Minimum scale to prevent 0 font sizes
            
            // Apply exponential scaling for timer text
            double exponentialScale = Math.Pow(scale, 0.8);
            
            // Apply scaling to timer display font size with minimum constraint
            if (TimerTextBlock != null)
            {
                double calculatedSize = 48 * exponentialScale; // Base size is 48
                TimerTextBlock.FontSize = Math.Max(12.0, calculatedSize); // Minimum 12pt font
            }
            
            // Apply scaling to status text font size
            if (StatusTextBlock != null)
            {
                double calculatedSize = 12 * exponentialScale; // Base size is 12
                StatusTextBlock.FontSize = Math.Max(8.0, calculatedSize); // Minimum 8pt font
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TimerState
    {
        Stopped,
        Running,
        Paused,
        Completed
    }
}
