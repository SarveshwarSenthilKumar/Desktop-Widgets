using System;
using System.ComponentModel;
using System.Windows;
using StopwatchWidget;

namespace WidgetDashboard.Models
{
    public class StopwatchWidgetWrapper : IWidget
    {
        private readonly StopwatchWidget.StopwatchWrapper _stopwatchWidget;

        public string Name => _stopwatchWidget.Name;
        public string Description => _stopwatchWidget.Description;
        public bool IsRunning => _stopwatchWidget.IsRunning;
        public Window WidgetWindow => _stopwatchWidget.WidgetWindow;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? WidgetClosed;

        public StopwatchWidgetWrapper()
        {
            _stopwatchWidget = new StopwatchWidget.StopwatchWrapper();
            
            // Forward property changed events
            if (_stopwatchWidget is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
            }
            
            // Forward widget closed events
            _stopwatchWidget.WidgetClosed += (s, e) => WidgetClosed?.Invoke(this, e);
        }

        public void Start()
        {
            _stopwatchWidget.Start();
            
            // Set the Tag property so the window can reference back to this wrapper
            // This ensures the StopwatchWindow can call NotifyClosed on the correct wrapper
            if (_stopwatchWidget.WidgetWindow is Window window)
            {
                window.Tag = this;
                System.Diagnostics.Debug.WriteLine($"Set Tag on StopwatchWindow to {this.GetType().Name}");
            }
        }

        public void Stop()
        {
            _stopwatchWidget.Stop();
        }

        public void Show()
        {
            _stopwatchWidget.Show();
        }

        public void Hide()
        {
            _stopwatchWidget.Hide();
        }

        public void NotifyClosed()
        {
            // This method is called by the StopwatchWindow when it's closed
            // Forward the notification to the underlying stopwatch widget
            System.Diagnostics.Debug.WriteLine("StopwatchWidgetWrapper.NotifyClosed called");
            _stopwatchWidget.NotifyClosed();
            System.Diagnostics.Debug.WriteLine("Forwarded NotifyClosed to underlying stopwatch widget");
        }

        public void SetPosition(double x, double y)
        {
            _stopwatchWidget.SetPosition(x, y);
        }

        public void SetSize(double width, double height)
        {
            _stopwatchWidget.SetSize(width, height);
        }
    }
}
