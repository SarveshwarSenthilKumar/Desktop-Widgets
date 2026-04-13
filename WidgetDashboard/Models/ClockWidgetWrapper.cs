using System;
using System.Windows;
using FuturisticClockWidget.Views;

namespace WidgetDashboard.Models
{
    public class ClockWidgetWrapper : WidgetBase
    {
        private static int _instanceCount = 0;
        private readonly int _instanceId;
        private readonly string _uniqueId;

        public override string Name => $"Futuristic Clock {_instanceId}";
        public override string Description => "A modern digital and analog clock widget with customizable styling";

        public ClockWidgetWrapper()
        {
            _instanceCount++;
            _instanceId = _instanceCount;
            _uniqueId = Guid.NewGuid().ToString("N")[..8]; // Short unique ID
        }

        protected override Window CreateWidgetWindow()
        {
            var clockWindow = new MainWindow();
            clockWindow.Title = $"Clock Widget {_instanceId}-{_uniqueId}";
            
            // Set the Tag property so the window can reference back to this wrapper
            clockWindow.Tag = this;
            
            return clockWindow;
        }

        public override void SetSize(double width, double height)
        {
            base.SetSize(width, height);
            
            // Trigger the size change logic in the clock widget
            if (_widgetWindow is MainWindow clockWindow)
            {
                // The clock widget has built-in size presets, so we'll trigger the size change
                clockWindow.Width = width;
                clockWindow.Height = height;
            }
        }
        
        public void NotifyClosed()
        {
            // This method is called by the MainWindow when it's closed
            // Trigger the WidgetClosed event to notify the dashboard
            NotifyWidgetClosed();
        }
    }
}
