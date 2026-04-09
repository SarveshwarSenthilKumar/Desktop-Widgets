using System;
using System.ComponentModel;
using System.Windows;
using CalendarWidget;

namespace WidgetDashboard.Models
{
    public class CalendarWidgetWrapper : IWidget, INotifyPropertyChanged
    {
        private readonly CalendarWidget.CalendarWidgetWrapper _calendarWidget;

        public string Name => _calendarWidget.Name;
        public string Description => _calendarWidget.Description;
        public bool IsRunning => _calendarWidget.IsRunning;
        public Window? WidgetWindow => _calendarWidget.WidgetWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CalendarWidgetWrapper()
        {
            _calendarWidget = new CalendarWidget.CalendarWidgetWrapper();
        }

        public void Start()
        {
            _calendarWidget.Start();
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

        public void SetPosition(double x, double y)
        {
            _calendarWidget.SetPosition(x, y);
        }

        public void SetSize(double width, double height)
        {
            _calendarWidget.SetSize(width, height);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
