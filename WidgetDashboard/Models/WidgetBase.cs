using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WidgetDashboard.Models
{
    public abstract class WidgetBase : IWidget
    {
        protected Window? _widgetWindow;
        private bool _isRunning;

        public abstract string Name { get; }
        public abstract string Description { get; }
        
        public Window WidgetWindow => _widgetWindow ?? throw new InvalidOperationException("Widget not initialized");
        public bool IsRunning => _isRunning;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? WidgetClosed;

        public virtual void Start()
        {
            if (_isRunning) return;
            
            _widgetWindow = CreateWidgetWindow();
            _isRunning = true;
            OnPropertyChanged(nameof(IsRunning));
        }

        public virtual void ReconnectToExistingWindow(Window existingWindow)
        {
            if (_isRunning) return;
            
            _widgetWindow = existingWindow;
            _isRunning = true;
            OnPropertyChanged(nameof(IsRunning));
        }

        public virtual void Stop()
        {
            if (!_isRunning) return;
            
            _widgetWindow?.Close();
            _widgetWindow = null!;
            _isRunning = false;
            OnPropertyChanged(nameof(IsRunning));
            
            // Notify that the widget was closed
            WidgetClosed?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Show()
        {
            if (_widgetWindow != null && !_widgetWindow.IsVisible)
            {
                _widgetWindow.Show();
            }
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
