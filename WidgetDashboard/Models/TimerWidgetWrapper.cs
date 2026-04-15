using System;
using System.Windows;
using FuturisticClockWidget.Views;

namespace WidgetDashboard.Models
{
    public class TimerWidgetWrapper : WidgetBase
    {
        public TimerWidgetWrapper()
        {
            Name = "Timer Widget";
            Description = "A countdown timer with customizable duration and controls";
        }

        public override string Name { get; } = "Timer Widget";
        public override string Description { get; } = "A countdown timer with customizable duration and controls";

        protected override Window CreateWidgetWindow()
        {
            var timerWindow = new TimerWindow();
            timerWindow.Closed += (s, e) => NotifyWidgetClosed();
            return timerWindow;
        }
    }
}
