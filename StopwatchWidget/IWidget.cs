using System;
using System.ComponentModel;
using System.Windows;

namespace StopwatchWidget
{
    public interface IWidget : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
        bool IsRunning { get; }
        Window WidgetWindow { get; }
        
        event EventHandler? WidgetClosed;
        
        void Start();
        void Stop();
        void Show();
        void Hide();
        void SetPosition(double x, double y);
        void SetSize(double width, double height);
    }
}
