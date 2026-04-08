using System;
using System.Windows;
using System.Windows.Controls;
using WidgetDashboard.Models;

namespace WidgetDashboard.Views
{
    public partial class MainWindow : Window
    {
        private readonly WidgetManager _widgetManager;

        public MainWindow()
        {
            InitializeComponent();
            _widgetManager = new WidgetManager();
            DataContext = _widgetManager;
            
            // Restore widget states when dashboard opens
            Loaded += (s, e) => _widgetManager.RestoreWidgetStates();
        }

        private void LaunchWidget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is IWidget widgetTemplate)
            {
                try
                {
                    // Create a new instance of the widget
                    IWidget newWidget;
                    
                    if (widgetTemplate is ClockWidgetWrapper)
                    {
                        newWidget = new ClockWidgetWrapper();
                    }
                    else
                    {
                        // For other widget types, we'd need to implement factory pattern
                        // For now, we'll just use the template
                        newWidget = widgetTemplate;
                    }

                    // Start the widget
                    _widgetManager.StartWidget(newWidget);
                    
                    // Show the widget
                    newWidget.Show();
                    
                    // Save state after launching
                    _widgetManager.SaveWidgetStates();
                    
                    // Position the widget at a default location
                    // You could implement smarter positioning here
                    var screenWidth = SystemParameters.PrimaryScreenWidth;
                    var screenHeight = SystemParameters.PrimaryScreenHeight;
                    var widgetCount = _widgetManager.ActiveWidgets.Count;
                    
                    // Arrange widgets in a grid pattern
                    var columns = 4;
                    var row = (widgetCount - 1) / columns;
                    var col = (widgetCount - 1) % columns;
                    
                    var x = 100 + (col * 320.0);
                    var y = 100 + (row * 200.0);
                    
                    // Ensure widget stays on screen
                    x = Math.Min(x, screenWidth - 300);
                    y = Math.Min(y, screenHeight - 150);
                    
                    newWidget.SetPosition(x, y);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching widget: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowWidget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is IWidget widget)
            {
                try
                {
                    widget.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error showing widget: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void HideWidget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is IWidget widget)
            {
                try
                {
                    widget.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error hiding widget: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopWidget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is IWidget widget)
            {
                try
                {
                    _widgetManager.StopWidget(widget);
                    _widgetManager.SaveWidgetStates();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error stopping widget: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopAllWidgets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _widgetManager.StopAllWidgets();
                _widgetManager.SaveWidgetStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping all widgets: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Save widget states before closing dashboard
            _widgetManager.SaveWidgetStates();
            base.OnClosed(e);
        }
    }
}
