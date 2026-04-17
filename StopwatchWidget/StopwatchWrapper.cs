using System;
using System.Windows;

namespace StopwatchWidget
{
    public class StopwatchWrapper : WidgetBase
    {
        private static int _instanceCount = 0;
        private readonly int _instanceId;
        private readonly string _uniqueId;

        public override string Name => $"Stopwatch Widget";
        public override string Description => "A professional stopwatch with lap times and precision timing";
        
        public string WidgetId => $"Stopwatch_{_instanceId}_{_uniqueId}";

        public StopwatchWrapper()
        {
            _instanceCount++;
            _instanceId = _instanceCount;
            _uniqueId = Guid.NewGuid().ToString("N")[..8]; // Short unique ID
        }

        protected override Window CreateWidgetWindow()
        {
            var stopwatchWindow = new StopwatchWindow();
            stopwatchWindow.Title = $"Stopwatch {_instanceId}-{_uniqueId}";
            return stopwatchWindow;
        }

        public override void SetSize(double width, double height)
        {
            base.SetSize(width, height);
            
            // Trigger size change logic in stopwatch widget
            if (_widgetWindow is StopwatchWindow stopwatchWindow)
            {
                stopwatchWindow.Width = width;
                stopwatchWindow.Height = height;
            }
        }
        
        public void NotifyClosed()
        {
            // This method is called by the StopwatchWindow when it's closed
            // Trigger the WidgetClosed event to notify the dashboard
            NotifyWidgetClosed();
        }
    }
}
