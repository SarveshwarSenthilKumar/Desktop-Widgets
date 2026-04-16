using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

namespace WidgetDashboard.Models
{
    public class ExternalWindowWrapper : IWidget
    {
        private readonly WindowInfo _windowInfo;
        private readonly WidgetBase _baseWidget;
        private bool _isRunning;

        public string Name => _baseWidget.Name;
        public string Description => _baseWidget.Description;
        public Window WidgetWindow => throw new InvalidOperationException("External window wrapper doesn't provide a WPF Window object");
        public bool IsRunning => _isRunning;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? WidgetClosed;

        public ExternalWindowWrapper(WindowInfo windowInfo, WidgetBase baseWidget)
        {
            _windowInfo = windowInfo ?? throw new ArgumentNullException(nameof(windowInfo));
            _baseWidget = baseWidget ?? throw new ArgumentNullException(nameof(baseWidget));
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            OnPropertyChanged(nameof(IsRunning));
        }

        public void Stop()
        {
            if (!_isRunning) return;
            
            // Close the external window
            User32.SendMessage(_windowInfo.Handle, User32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            
            _isRunning = false;
            OnPropertyChanged(nameof(IsRunning));
            
            // Notify that the widget was closed
            WidgetClosed?.Invoke(this, EventArgs.Empty);
        }

        public void Show()
        {
            if (_isRunning)
            {
                User32.ShowWindow(_windowInfo.Handle, User32.SW_SHOW);
            }
        }

        public void Hide()
        {
            if (_isRunning)
            {
                User32.ShowWindow(_windowInfo.Handle, User32.SW_HIDE);
            }
        }

        public void SetPosition(double x, double y)
        {
            if (_isRunning)
            {
                User32.SetWindowPos(_windowInfo.Handle, IntPtr.Zero, (int)x, (int)y, 0, 0, 
                    User32.SWP_NOSIZE | User32.SWP_NOZORDER);
            }
        }

        public void SetSize(double width, double height)
        {
            if (_isRunning)
            {
                User32.SetWindowPos(_windowInfo.Handle, IntPtr.Zero, 0, 0, (int)width, (int)height, 
                    User32.SWP_NOMOVE | User32.SWP_NOZORDER);
            }
        }

        public void ReconnectToExistingWindow()
        {
            _isRunning = true;
            OnPropertyChanged(nameof(IsRunning));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class User32
    {
        public const int WM_CLOSE = 0x0010;
        public const int SW_SHOW = 5;
        public const int SW_HIDE = 0;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_STYLE = -16;
        public const int WS_VISIBLE = 0x10000000;
    }
}
