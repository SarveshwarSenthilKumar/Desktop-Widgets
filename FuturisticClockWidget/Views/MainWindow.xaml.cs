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
        private Point _resizeStartPosition;
        private Point _resizeStartScreenPoint;
        private Point _resizeStartMousePosition;
        private DispatcherTimer _resizeTimer;
        private string _currentCorner;
        private string _currentEdge;
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
        
        private void SizeSmall_Click(object sender, RoutedEventArgs e)
        {
            Width = 200;
            Height = 100;
            // Center the window on screen
            Left = (SystemParameters.PrimaryScreenWidth - 200) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - 100) / 2;
        }
        
        private void SizeMedium_Click(object sender, RoutedEventArgs e)
        {
            Width = 280;
            Height = 140;
            // Center the window on screen
            Left = (SystemParameters.PrimaryScreenWidth - 280) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - 140) / 2;
        }
        
        private void SizeLarge_Click(object sender, RoutedEventArgs e)
        {
            Width = 400;
            Height = 200;
            // Center the window on screen
            Left = (SystemParameters.PrimaryScreenWidth - 400) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - 200) / 2;
        }
        
        private void SizeExtraLarge_Click(object sender, RoutedEventArgs e)
        {
            Width = 600;
            Height = 300;
            // Center the window on screen
            Left = (SystemParameters.PrimaryScreenWidth - 600) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - 300) / 2;
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFontSizes();
        }
        
        private void CornerResize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isResizing = true;
                _resizeStartSize = new Size(ActualWidth, ActualHeight);
                _resizeStartPosition = new Point(Left, Top);
                _currentCorner = (sender as FrameworkElement)?.Tag as string;
                _resizeStartMousePosition = e.GetPosition(null); // Screen coordinates
                
                // Start timer for smooth resize
                _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 FPS
                _resizeTimer.Tick += ResizeTimer_Tick;
                _resizeTimer.Start();
                
                e.Handled = true;
            }
        }
        
        private void CornerResize_MouseMove(object sender, MouseEventArgs e)
        {
            // Mouse move is handled by timer for smoother operation
        }
        
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (_isResizing && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point currentMousePosition = Mouse.GetPosition(null); // Screen coordinates
                double deltaX = currentMousePosition.X - _resizeStartMousePosition.X;
                double deltaY = currentMousePosition.Y - _resizeStartMousePosition.Y;
                
                double newWidth = _resizeStartSize.Width;
                double newHeight = _resizeStartSize.Height;
                double newLeft = _resizeStartPosition.X;
                double newTop = _resizeStartPosition.Y;
                
                // Handle corner resize
                if (_currentCorner != null)
                {
                    switch (_currentCorner)
                    {
                        case "TopLeft":
                            newWidth = Math.Max(200, _resizeStartSize.Width - deltaX);
                            newHeight = Math.Max(100, _resizeStartSize.Height - deltaY);
                            // Position moves with mouse for top-left corner
                            newLeft = _resizeStartPosition.X + deltaX;
                            newTop = _resizeStartPosition.Y + deltaY;
                            break;
                        case "TopRight":
                            newWidth = Math.Max(200, _resizeStartSize.Width + deltaX);
                            newHeight = Math.Max(100, _resizeStartSize.Height - deltaY);
                            // Only Y position moves for top-right corner
                            newTop = _resizeStartPosition.Y + deltaY;
                            break;
                        case "BottomLeft":
                            newWidth = Math.Max(200, _resizeStartSize.Width - deltaX);
                            newHeight = Math.Max(100, _resizeStartSize.Height + deltaY);
                            // Only X position moves for bottom-left corner
                            newLeft = _resizeStartPosition.X + deltaX;
                            break;
                        case "BottomRight":
                            newWidth = Math.Max(200, _resizeStartSize.Width + deltaX);
                            newHeight = Math.Max(100, _resizeStartSize.Height + deltaY);
                            // Position stays fixed for bottom-right corner
                            break;
                    }
                }
                
                // Handle edge resize
                else if (_currentEdge != null)
                {
                    switch (_currentEdge)
                    {
                        case "Top":
                            newHeight = Math.Max(100, _resizeStartSize.Height - deltaY);
                            // Only Y position moves for top edge
                            newTop = _resizeStartPosition.Y + deltaY;
                            break;
                        case "Bottom":
                            newHeight = Math.Max(100, _resizeStartSize.Height + deltaY);
                            // Position stays fixed for bottom edge
                            break;
                        case "Left":
                            newWidth = Math.Max(200, _resizeStartSize.Width - deltaX);
                            // Only X position moves for left edge
                            newLeft = _resizeStartPosition.X + deltaX;
                            break;
                        case "Right":
                            newWidth = Math.Max(200, _resizeStartSize.Width + deltaX);
                            // Position stays fixed for right edge
                            break;
                    }
                }
                
                // Apply window properties with constraints
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + newWidth > SystemParameters.PrimaryScreenWidth) 
                    newLeft = SystemParameters.PrimaryScreenWidth - newWidth;
                if (newTop + newHeight > SystemParameters.PrimaryScreenHeight) 
                    newTop = SystemParameters.PrimaryScreenHeight - newHeight;
                
                Width = newWidth;
                Height = newHeight;
                Left = newLeft;
                Top = newTop;
            }
            else
            {
                // Stop resizing if mouse button is released
                StopResize();
            }
        }
        
        private void StopResize()
        {
            if (_resizeTimer != null)
            {
                _resizeTimer.Stop();
                _resizeTimer = null;
            }
            _isResizing = false;
            _currentCorner = null;
            _currentEdge = null;
        }
        
        private void CornerResize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopResize();
            e.Handled = true;
        }
        
        private void EdgeResize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isResizing = true;
                _resizeStartSize = new Size(ActualWidth, ActualHeight);
                _resizeStartPosition = new Point(Left, Top);
                _currentEdge = (sender as FrameworkElement)?.Tag as string;
                _resizeStartMousePosition = e.GetPosition(null); // Screen coordinates
                
                // Start timer for smooth resize
                _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 FPS
                _resizeTimer.Tick += ResizeTimer_Tick;
                _resizeTimer.Start();
                
                e.Handled = true;
            }
        }
        
        private void EdgeResize_MouseMove(object sender, MouseEventArgs e)
        {
            // Mouse move is handled by timer for smoother operation
        }
        
        private void EdgeResize_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopResize();
            e.Handled = true;
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
            if (e.ChangedButton == MouseButton.Left && !_isResizing)
            {
                // Only allow dragging if we're not currently resizing
                try
                {
                    // Focus this window to ensure proper control
                    Focus();
                    
                    // Check if we're clicking on the main content area (not resize areas)
                    Point clickPosition = e.GetPosition(this);
                    
                    // Define safe area for dragging (exclude edges and corners)
                    bool inResizeArea = false;
                    double edgeThreshold = 15;
                    double cornerThreshold = 20;
                    
                    // Check corners
                    if ((clickPosition.X < cornerThreshold && clickPosition.Y < cornerThreshold) ||
                        (clickPosition.X > ActualWidth - cornerThreshold && clickPosition.Y < cornerThreshold) ||
                        (clickPosition.X < cornerThreshold && clickPosition.Y > ActualHeight - cornerThreshold) ||
                        (clickPosition.X > ActualWidth - cornerThreshold && clickPosition.Y > ActualHeight - cornerThreshold))
                    {
                        inResizeArea = true;
                    }
                    
                    // Check edges
                    if (!inResizeArea &&
                        (clickPosition.X < edgeThreshold || clickPosition.X > ActualWidth - edgeThreshold ||
                         clickPosition.Y < edgeThreshold || clickPosition.Y > ActualHeight - edgeThreshold))
                    {
                        inResizeArea = true;
                    }
                    
                    // Only drag if not in resize area
                    if (!inResizeArea)
                    {
                        DragMove();
                    }
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
