using System;
using System.Windows;

namespace CalendarWidget
{
    public class CalendarWidgetWrapper : WidgetBase
    {
        private static int _instanceCount = 0;
        private readonly int _instanceId;
        private readonly string _uniqueId;

        public override string Name => $"Calendar Widget";
        public override string Description => "A futuristic calendar widget with enhanced features and useful information";
        
        public string WidgetId => $"Calendar_{_instanceId}_{_uniqueId}";

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
        
        public void NotifyClosed()
        {
            // This method is called by the CalendarWindow when it's closed
            // Trigger the WidgetClosed event to notify the dashboard
            NotifyWidgetClosed();
        }
    }
}
