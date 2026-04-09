using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WidgetDashboard.Models
{
    public abstract class PositionAwareWidget : WidgetBase
    {
        private string _widgetId = string.Empty;
        
        public string WidgetId
        {
            get => _widgetId;
            set
            {
                _widgetId = value;
                base.OnPropertyChanged();
            }
        }

        protected void SetupPositionTracking(Window window)
        {
            if (window == null) return;

            // Track position changes
            window.LocationChanged += (s, e) => OnPositionChanged();
            window.SizeChanged += (s, e) => OnPositionChanged();
            
            // Restore saved position if available
            RestorePosition();
        }

        private void OnPositionChanged()
        {
            // This will trigger position saving through the WidgetManager
            // The actual saving logic is handled by the WidgetManager
            // when it detects position changes
        }

        private void RestorePosition()
        {
            // Position restoration is handled by the WidgetManager
            // when the widget is created and connected to existing windows
        }
    }
}
