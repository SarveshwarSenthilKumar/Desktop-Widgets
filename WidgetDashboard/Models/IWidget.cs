using System;
using System.ComponentModel;
using System.Windows;

namespace WidgetDashboard.Models
{
    public interface IWidget : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
        Window WidgetWindow { get; }
        bool IsRunning { get; }
        
        event EventHandler? WidgetClosed;
        
        void Start();
        void Stop();
        void Show();
        void Hide();
        void SetPosition(double x, double y);
        void SetSize(double width, double height);
    }
}
