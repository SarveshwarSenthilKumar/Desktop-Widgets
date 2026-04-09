using System;
using System.Windows;
using WidgetDashboard.Views;

namespace WidgetDashboard.Models
{
    public class CalendarWidgetWrapper : WidgetBase
    {
        private static int _instanceCount = 0;
        private readonly int _instanceId;
        private readonly string _uniqueId;

        public override string Name => $"Calendar Widget {_instanceId}";
        public override string Description => "A modern calendar widget with current date and month view";

        public CalendarWidgetWrapper()
        {
            _instanceCount++;
            _instanceId = _instanceCount;
            _uniqueId = Guid.NewGuid().ToString("N")[..8]; // Short unique ID
        }

        protected override Window CreateWidgetWindow()
        {
            var calendarWindow = new CalendarWindow();
            calendarWindow.Title = $"Calendar Widget {_instanceId}-{_uniqueId}";
            return calendarWindow;
        }

        public override void SetSize(double width, double height)
        {
            base.SetSize(width, height);
            
            // Trigger size change logic in calendar widget
            if (_widgetWindow is CalendarWindow calendarWindow)
            {
                calendarWindow.Width = width;
                calendarWindow.Height = height;
            }
        }
    }
}
