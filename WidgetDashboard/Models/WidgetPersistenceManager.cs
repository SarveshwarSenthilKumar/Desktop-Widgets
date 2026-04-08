using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace WidgetDashboard.Models
{
    public class WidgetState
    {
        public string WidgetType { get; set; } = string.Empty;
        public string WidgetId { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
        public DateTime LastStarted { get; set; }
    }

    public static class WindowFinder
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static List<WindowInfo> FindAllWindowsByTitle(string title)
        {
            var windows = new List<WindowInfo>();
            
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    var builder = new System.Text.StringBuilder(256);
                    GetWindowText(hWnd, builder, builder.Capacity);
                    var windowTitle = builder.ToString();
                    
                    if (windowTitle.Contains(title))
                    {
                        GetWindowThreadProcessId(hWnd, out uint processId);
                        windows.Add(new WindowInfo
                        {
                            Handle = hWnd,
                            Title = windowTitle,
                            ProcessId = (int)processId
                        });
                    }
                }
                return true;
            }, IntPtr.Zero);
            
            return windows;
        }

        public static WindowInfo? FindWindowByExactTitle(string title)
        {
            var windows = FindAllWindowsByTitle(title);
            return windows.FirstOrDefault(w => w.Title == title);
        }
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ProcessId { get; set; }
    }

    public class WidgetPersistenceManager
    {
        private static readonly string PersistenceFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WidgetDashboard",
            "widget_state.json"
        );

        public void SaveWidgetStates(IEnumerable<IWidget> widgets)
        {
            try
            {
                var states = new List<WidgetState>();
                
                foreach (var widget in widgets)
                {
                    if (widget.IsRunning)
                    {
                        var state = new WidgetState
                        {
                            WidgetType = widget.GetType().FullName ?? widget.GetType().Name,
                            WidgetId = widget.GetHashCode().ToString(),
                            WindowTitle = widget.WidgetWindow.Title,
                            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
                            PositionX = widget.WidgetWindow.Left,
                            PositionY = widget.WidgetWindow.Top,
                            Width = 0, // Don't save size - preserve widget defaults
                            Height = 0, // Don't save size - preserve widget defaults
                            IsVisible = widget.WidgetWindow.IsVisible,
                            LastStarted = DateTime.UtcNow
                        };
                        states.Add(state);
                    }
                }

                var directory = Path.GetDirectoryName(PersistenceFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(states, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(PersistenceFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - persistence shouldn't crash the app
                System.Diagnostics.Debug.WriteLine($"Error saving widget states: {ex.Message}");
            }
        }

        public List<WidgetState> LoadWidgetStates()
        {
            try
            {
                if (!File.Exists(PersistenceFilePath))
                {
                    return new List<WidgetState>();
                }

                var json = File.ReadAllText(PersistenceFilePath);
                var states = JsonSerializer.Deserialize<List<WidgetState>>(json);
                
                return states ?? new List<WidgetState>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading widget states: {ex.Message}");
                return new List<WidgetState>();
            }
        }

        public List<WindowInfo> FindExistingWidgetWindows()
        {
            var existingWindows = new List<WindowInfo>();
            
            try
            {
                // Search for widget windows across all processes
                var allWindows = WindowFinder.FindAllWindowsByTitle("Clock Widget");
                
                existingWindows.AddRange(allWindows);
                
                // Remove duplicates
                existingWindows = existingWindows.GroupBy(w => w.Handle).Select(g => g.First()).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding existing windows: {ex.Message}");
            }
            
            return existingWindows;
        }

        public List<WindowInfo> FindWidgetWindowsByTitle(string title)
        {
            try
            {
                return WindowFinder.FindAllWindowsByTitle(title);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding windows by title '{title}': {ex.Message}");
                return new List<WindowInfo>();
            }
        }

        public WindowInfo? FindWidgetWindowByExactTitle(string title)
        {
            try
            {
                return WindowFinder.FindWindowByExactTitle(title);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding window by exact title '{title}': {ex.Message}");
                return null;
            }
        }

        public void ClearPersistedStates()
        {
            try
            {
                if (File.Exists(PersistenceFilePath))
                {
                    File.Delete(PersistenceFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing widget states: {ex.Message}");
            }
        }
    }
}
