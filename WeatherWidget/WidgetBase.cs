using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WeatherWidget
{
    public abstract class WidgetBase : IWidget
    {
        protected Window? _widgetWindow;
        private bool _isRunning = false;

        public bool IsRunning => _isRunning;
        public Window WidgetWindow => _widgetWindow ?? throw new InvalidOperationException("Widget not initialized");

        public abstract string Name { get; }
        public abstract string Description { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? WidgetClosed;

        public virtual void Start()
        {
            if (!_isRunning)
            {
                _widgetWindow = CreateWidgetWindow();
                if (_widgetWindow != null)
                {
                    // Set the Tag property so the window can reference back to this wrapper
                    _widgetWindow.Tag = this;
                    _widgetWindow.Closed += (s, e) => NotifyWidgetClosed();
                }
                _isRunning = true;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public virtual void Stop()
        {
            if (_isRunning)
            {
                _widgetWindow?.Close();
                _widgetWindow = null;
                _isRunning = false;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public virtual void Show()
        {
            _widgetWindow?.Show();
        }

        public virtual void Hide()
        {
            _widgetWindow?.Hide();
        }

        public virtual void SetPosition(double x, double y)
        {
            if (_widgetWindow != null)
            {
                _widgetWindow.Left = x;
                _widgetWindow.Top = y;
            }
        }

        public virtual void SetSize(double width, double height)
        {
            if (_widgetWindow != null)
            {
                _widgetWindow.Width = width;
                _widgetWindow.Height = height;
            }
        }

        protected abstract Window CreateWidgetWindow();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected void NotifyWidgetClosed()
        {
            WidgetClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
