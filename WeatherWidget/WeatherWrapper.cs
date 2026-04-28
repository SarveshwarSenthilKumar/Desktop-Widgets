using System;
using System.Windows;

namespace WeatherWidget
{
    public class WeatherWrapper : WidgetBase
    {
        private static int _instanceCount = 0;
        private readonly int _instanceId;
        private readonly string _uniqueId;

        public override string Name => $"Weather Widget";
        public override string Description => "Real-time weather information with forecast";
        
        public string WidgetId => $"Weather_{_instanceId}_{_uniqueId}";

        public WeatherWrapper()
        {
            _instanceCount++;
            _instanceId = _instanceCount;
            _uniqueId = Guid.NewGuid().ToString("N")[..8]; // Short unique ID
        }

        protected override Window CreateWidgetWindow()
        {
            var weatherWindow = new WeatherWindow();
            weatherWindow.Title = $"Weather {_instanceId}-{_uniqueId}";
            return weatherWindow;
        }

        public override void SetSize(double width, double height)
        {
            base.SetSize(width, height);
            
            // Trigger size change logic in weather widget
            if (_widgetWindow is WeatherWindow weatherWindow)
            {
                weatherWindow.Width = width;
                weatherWindow.Height = height;
            }
        }
        
        public void NotifyClosed()
        {
            // This method is called by the WeatherWindow when it's closed
            // Trigger the WidgetClosed event to notify the dashboard
            NotifyWidgetClosed();
        }
    }
}
