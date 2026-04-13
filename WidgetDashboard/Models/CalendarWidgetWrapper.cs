using System;
using System.ComponentModel;
using System.Windows;
using CalendarWidget;

namespace WidgetDashboard.Models
{
    public class CalendarWidgetWrapper : IWidget
    {
        private readonly CalendarWidget.CalendarWidgetWrapper _calendarWidget;

        public string Name => _calendarWidget.Name;
        public string Description => _calendarWidget.Description;
        public bool IsRunning => _calendarWidget.IsRunning;
        public Window WidgetWindow => _calendarWidget.WidgetWindow;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? WidgetClosed;

        public CalendarWidgetWrapper()
        {
            _calendarWidget = new CalendarWidget.CalendarWidgetWrapper();
            
            // Forward property changed events
            if (_calendarWidget is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
            }
            
            // Forward widget closed events
            _calendarWidget.WidgetClosed += (s, e) => WidgetClosed?.Invoke(this, e);
        }

        public void Start()
        {
            _calendarWidget.Start();
            
            // Set the Tag property so the window can reference back to this wrapper
            if (_calendarWidget.WidgetWindow is Window window)
            {
                window.Tag = this;
            }
        }

        public void Stop()
        {
            _calendarWidget.Stop();
        }

        public void Show()
        {
            _calendarWidget.Show();
        }

        public void Hide()
        {
            _calendarWidget.Hide();
        }

        public void NotifyClosed()
        {
            // This method is called by the CalendarWindow when it's closed
            // Trigger the WidgetClosed event to notify the dashboard
            System.Diagnostics.Debug.WriteLine("CalendarWidgetWrapper.NotifyClosed called");
            WidgetClosed?.Invoke(this, EventArgs.Empty);
            System.Diagnostics.Debug.WriteLine("WidgetClosed event invoked");
        }

        public void SetPosition(double x, double y)
        {
            _calendarWidget.SetPosition(x, y);
        }

        public void SetSize(double width, double height)
        {
            _calendarWidget.SetSize(width, height);
        }
    }
}
