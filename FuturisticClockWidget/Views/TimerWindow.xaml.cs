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
        private bool _isAnalogMode = false;

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
                
                // Update analog display if in analog mode
                if (_isAnalogMode)
                {
                    UpdateAnalogDisplay();
                }
                
                if (RemainingTime.TotalSeconds <= 0)
                {
                    RemainingTime = TimeSpan.Zero;
                    _isRunning = false;
                    _timerState = TimerState.Completed;
                    _timer.Stop();
                    
                    // Update UI
                    OnPropertyChanged(nameof(TimerStatus));
                    UpdateButtonStates();
                    
                    // Update analog display one final time
                    if (_isAnalogMode)
                    {
                        UpdateAnalogDisplay();
                    }
                    
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
            
            // Update analog display
            if (_isAnalogMode)
            {
                UpdateAnalogDisplay();
            }
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
            
            // Update analog display
            if (_isAnalogMode)
            {
                UpdateAnalogDisplay();
            }
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
            
            // Update analog clock size if in analog mode
            if (_isAnalogMode)
            {
                UpdateAnalogClockSize();
            }
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Escape key to close
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }
            
            // Space to start/pause timer
            if (e.Key == Key.Space)
            {
                StartPause_Click(this, new RoutedEventArgs());
                return;
            }
            
            // R to reset timer
            if (e.Key == Key.R)
            {
                Reset_Click(this, new RoutedEventArgs());
                return;
            }
            
            // S to set timer
            if (e.Key == Key.S)
            {
                Set_Click(this, new RoutedEventArgs());
                return;
            }
            
            // T to toggle topmost
            if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Topmost = !Topmost;
                return;
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

        private void ToggleAnalogDigital_Click(object sender, RoutedEventArgs e)
        {
            _isAnalogMode = !_isAnalogMode;
            
            if (_isAnalogMode)
            {
                DigitalDisplay.Visibility = Visibility.Collapsed;
                AnalogDisplay.Visibility = Visibility.Visible;
                UpdateAnalogClockSize();
            }
            else
            {
                DigitalDisplay.Visibility = Visibility.Visible;
                AnalogDisplay.Visibility = Visibility.Collapsed;
            }
            
            // Update analog display if switching to analog mode
            if (_isAnalogMode)
            {
                UpdateAnalogDisplay();
            }
        }

        private void UpdateAnalogClockSize()
        {
            if (!_isAnalogMode) return;

            // Calculate optimal size based on window dimensions
            double availableHeight = ActualHeight - 80; // Account for buttons and margins
            double availableWidth = ActualWidth - 50; // Account for margins
            
            // Use the smaller dimension for a perfect circle
            double maxSize = Math.Min(availableHeight, availableWidth);
            
            // Ensure minimum size for visibility
            maxSize = Math.Max(maxSize, 60);
            
            // Apply size constraints
            var viewbox = FindChild<Viewbox>(AnalogDisplay);
            if (viewbox != null)
            {
                viewbox.MaxWidth = maxSize;
                viewbox.MaxHeight = maxSize;
            }
        }

        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childResult = FindChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }

        private void UpdateAnalogDisplay()
        {
            if (!_isAnalogMode) return;

            // Calculate progress (0 to 1, where 1 is completed)
            double progress = _initialTime.TotalSeconds > 0 
                ? 1 - (RemainingTime.TotalSeconds / _initialTime.TotalSeconds)
                : 1;

            // Update the timer hand position (12 o'clock is fully completed, clockwise rotation)
            double angle = -90 + (progress * 360); // Start at 12 o'clock (-90 degrees)
            
            // Calculate hand endpoint
            double handLength = 35; // Length of the timer hand
            double radians = angle * Math.PI / 180;
            double endX = 60 + handLength * Math.Cos(radians);
            double endY = 60 + handLength * Math.Sin(radians);
            
            TimerHand.X2 = endX;
            TimerHand.Y2 = endY;
            
            // Update progress arc
            UpdateProgressArc(progress);
        }

        private void UpdateProgressArc(double progress)
        {
            if (progress <= 0) return;

            // Create an arc path from 12 o'clock clockwise
            var startAngle = -90; // Start at 12 o'clock
            var endAngle = startAngle + (progress * 360);
            var radius = 55;
            var center = new Point(60, 60);

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(
                    center.X + radius * Math.Cos(startAngle * Math.PI / 180),
                    center.Y + radius * Math.Sin(startAngle * Math.PI / 180))
            };

            var arcSegment = new ArcSegment
            {
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                Point = new Point(
                    center.X + radius * Math.Cos(endAngle * Math.PI / 180),
                    center.Y + radius * Math.Sin(endAngle * Math.PI / 180))
            };

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);

            ProgressArc.Data = pathGeometry;
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
