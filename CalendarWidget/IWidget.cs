using System.ComponentModel;
using System.Windows;

namespace CalendarWidget
{
    public interface IWidget : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
        Window WidgetWindow { get; }
        bool IsRunning { get; }
        
        void Start();
        void Stop();
        void Show();
        void Hide();
        void SetPosition(double x, double y);
        void SetSize(double width, double height);
    }
}
