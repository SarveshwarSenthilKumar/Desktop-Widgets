using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfColor = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;

namespace StopwatchWidget
{
    public partial class StopwatchWindow : Window, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch;
        private readonly ObservableCollection<LapTimeEntry> _lapTimes;
        private WpfColor _backgroundColor = WpfColor.FromArgb(20, 0, 0, 0);
        private WpfColor _baseColor = WpfColor.FromRgb(0, 212, 255);
        private int _lapCounter = 0;
        private bool _isAnalogMode = false;

        public ObservableCollection<LapTimeEntry> LapTimes => _lapTimes;
        public bool HasLapTimes => _lapTimes.Count > 0;

        public string ElapsedTimeString
        {
            get
            {
                var elapsed = _stopwatch.Elapsed;
                return $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}";
            }
        }

        public string Status
        {
            get
            {
                if (_stopwatch.IsRunning) return "Running";
                if (_stopwatch.Elapsed.TotalMilliseconds > 0) return "Stopped";
                return "Ready";
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

        public StopwatchWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            _stopwatch = new Stopwatch();
            _lapTimes = new ObservableCollection<LapTimeEntry>();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Update every 10ms for smooth display
            };
            _timer.Tick += Timer_Tick;
            
            // Initialize UI
            UpdateButtonStates();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ElapsedTimeString));
            OnPropertyChanged(nameof(Status));
            
            // Update analog display if in analog mode
            if (_isAnalogMode)
            {
                UpdateAnalogDisplay();
            }
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (_stopwatch.IsRunning)
            {
                StopStopwatch();
            }
            else
            {
                StartStopwatch();
            }
        }

        private void Lap_Click(object sender, RoutedEventArgs e)
        {
            if (_stopwatch.IsRunning)
            {
                _lapCounter++;
                var lapTime = _stopwatch.Elapsed;
                _lapTimes.Insert(0, new LapTimeEntry(_lapCounter, lapTime));
                OnPropertyChanged(nameof(HasLapTimes));
                
                // Keep only last 10 lap times to prevent UI from getting too crowded
                if (_lapTimes.Count > 10)
                {
                    _lapTimes.RemoveAt(_lapTimes.Count - 1);
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _stopwatch.Reset();
            _timer.Stop();
            _lapTimes.Clear();
            _lapCounter = 0;
            
            OnPropertyChanged(nameof(ElapsedTimeString));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(HasLapTimes));
            
            UpdateButtonStates();
        }

        private void ClearLaps_Click(object sender, RoutedEventArgs e)
        {
            _lapTimes.Clear();
            OnPropertyChanged(nameof(HasLapTimes));
        }

        private void StartStopwatch()
        {
            _stopwatch.Start();
            _timer.Start();
            UpdateButtonStates();
            OnPropertyChanged(nameof(Status));
        }

        private void StopStopwatch()
        {
            _stopwatch.Stop();
            _timer.Stop();
            UpdateButtonStates();
            OnPropertyChanged(nameof(Status));
        }

        private void UpdateButtonStates()
        {
            if (StartStopButton != null)
            {
                StartStopButton.Content = _stopwatch.IsRunning ? "Stop" : "Start";
            }
            
            if (LapButton != null)
            {
                LapButton.IsEnabled = _stopwatch.IsRunning;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error moving window: {ex.Message}");
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
            Width = 280;
            Height = 180;
        }

        private void SizeMedium_Click(object sender, RoutedEventArgs e)
        {
            Width = 320;
            Height = 200;
        }

        private void SizeLarge_Click(object sender, RoutedEventArgs e)
        {
            Width = 380;
            Height = 250;
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
                Debug.WriteLine($"Error changing background color: {ex.Message}");
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
                Debug.WriteLine($"Error changing base color: {ex.Message}");
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
                Debug.WriteLine($"Error resetting colors: {ex.Message}");
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
            
            Debug.WriteLine("StopwatchWindow.OnClosed called");
            
            // Clean up timer
            _timer.Stop();
            _stopwatch.Stop();
            
            // Notify the wrapper that the widget was closed
            if (Tag is StopwatchWrapper wrapper)
            {
                Debug.WriteLine("Found wrapper in Tag, calling NotifyClosed");
                wrapper.NotifyClosed();
            }
            else
            {
                Debug.WriteLine($"Tag is {Tag?.GetType().Name ?? "null"}, expected StopwatchWrapper");
                
                // Try to call NotifyClosed via reflection as a fallback
                try
                {
                    if (Tag != null)
                    {
                        var notifyClosedMethod = Tag.GetType().GetMethod("NotifyClosed");
                        if (notifyClosedMethod != null)
                        {
                            Debug.WriteLine($"Found NotifyClosed method via reflection on {Tag.GetType().Name}");
                            notifyClosedMethod.Invoke(Tag, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to call NotifyClosed via reflection: {ex.Message}");
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
                Debug.WriteLine($"Error updating colors: {ex.Message}");
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
                        Debug.WriteLine($"Error updating text block: {ex.Message}");
                        // Continue with other text blocks
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateTextColors: {ex.Message}");
            }
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

        private void UpdateAnalogDisplay()
        {
            if (!_isAnalogMode) return;

            // Calculate seconds position (0-59)
            var elapsed = _stopwatch.Elapsed;
            double seconds = elapsed.Seconds + (elapsed.Milliseconds / 1000.0);
            
            // Calculate hand angle (12 o'clock is 0 degrees, clockwise)
            double angle = -90 + (seconds * 6); // 6 degrees per second
            
            // Calculate hand endpoint (canvas is 100x100, center is 50,50)
            double handLength = 30; // Length of the stopwatch hand
            double radians = angle * Math.PI / 180;
            double endX = 50 + handLength * Math.Cos(radians);
            double endY = 50 + handLength * Math.Sin(radians);
            
            StopwatchHand.X2 = endX;
            StopwatchHand.Y2 = endY;
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

    public class LapTimeEntry
    {
        public int LapNumber { get; set; }
        public TimeSpan LapTimeValue { get; set; }
        
        public string LapTime
        {
            get
            {
                var time = LapTimeValue;
                return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}";
            }
        }

        public LapTimeEntry(int lapNumber, TimeSpan lapTime)
        {
            LapNumber = lapNumber;
            LapTimeValue = lapTime;
        }
    }
}
