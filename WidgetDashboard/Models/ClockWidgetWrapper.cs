using System.Windows;
using FuturisticClockWidget.Views;

namespace WidgetDashboard.Models
{
    public class ClockWidgetWrapper : WidgetBase
    {
        public override string Name => "Futuristic Clock";
        public override string Description => "A modern digital and analog clock widget with customizable styling";

        protected override Window CreateWidgetWindow()
        {
            var clockWindow = new MainWindow();
            clockWindow.Title = "Clock Widget";
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
    }
}
