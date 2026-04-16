using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using CalendarWidget;

namespace WidgetDashboard.Models
{
    public class WidgetManager : INotifyPropertyChanged
    {
        private readonly Dictionary<Type, Func<IWidget>> _widgetFactories;
        private readonly ObservableCollection<IWidget> _activeWidgets;
        private readonly ObservableCollection<IWidget> _availableWidgets;
        private readonly WidgetPersistenceManager _persistenceManager;
        private readonly DispatcherTimer _monitoringTimer;
        private readonly HashSet<IntPtr> _monitoredWindowHandles;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReadOnlyObservableCollection<IWidget> AvailableWidgets { get; }
        public ReadOnlyObservableCollection<IWidget> ActiveWidgets { get; }

        public WidgetManager()
        {
            _widgetFactories = new Dictionary<Type, Func<IWidget>>();
            _activeWidgets = new ObservableCollection<IWidget>();
            _availableWidgets = new ObservableCollection<IWidget>();
            _persistenceManager = new WidgetPersistenceManager();
            _monitoredWindowHandles = new HashSet<IntPtr>();
            _monitoringTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Check every 2 seconds
            };
            _monitoringTimer.Tick += MonitorExternalWindows;

            AvailableWidgets = new ReadOnlyObservableCollection<IWidget>(_availableWidgets);
            ActiveWidgets = new ReadOnlyObservableCollection<IWidget>(_activeWidgets);

            RegisterWidgetTypes();
            
            // Start monitoring for external widget closures
            _monitoringTimer.Start();
        }

        private void RegisterWidgetTypes()
        {
            RegisterWidget<ClockWidgetWrapper>();
            RegisterWidget<Models.CalendarWidgetWrapper>();
            RegisterWidget<TimerWidgetWrapper>();
            RegisterWidget<StopwatchWidgetWrapper>();
        }

        public void RegisterWidget<T>() where T : IWidget, new()
        {
            _widgetFactories[typeof(T)] = () => new T();
            _availableWidgets.Add(Activator.CreateInstance<T>());
        }

        public IWidget CreateWidget<T>() where T : IWidget
        {
            if (_widgetFactories.TryGetValue(typeof(T), out var factory))
            {
                return factory();
            }
            throw new ArgumentException($"Widget type {typeof(T).Name} is not registered");
        }

        public IWidget CreateWidget(Type widgetType)
        {
            if (_widgetFactories.TryGetValue(widgetType, out var factory))
            {
                return factory();
            }
            throw new ArgumentException($"Widget type {widgetType.Name} is not registered");
        }

        public void StartWidget(IWidget widget)
        {
            if (widget != null && !_activeWidgets.Contains(widget))
            {
                widget.Start();
                _activeWidgets.Add(widget);
                
                // Listen for widget closed events
                widget.WidgetClosed += OnWidgetClosed;
                
                // Track external window handle if applicable
                if (widget is ExternalWindowWrapper externalWrapper)
                {
                    var windowInfo = _persistenceManager.FindWidgetWindowByExactTitle(widget.Name);
                    if (windowInfo != null)
                    {
                        _monitoredWindowHandles.Add(windowInfo.Handle);
                    }
                }
            }
        }

        public void StopWidget(IWidget widget)
        {
            if (widget != null && _activeWidgets.Contains(widget))
            {
                // Unsubscribe from widget closed events
                widget.WidgetClosed -= OnWidgetClosed;
                
                // Remove window handle from monitoring
                if (widget is ExternalWindowWrapper externalWrapper)
                {
                    var windowInfo = _persistenceManager.FindWidgetWindowByExactTitle(widget.Name);
                    if (windowInfo != null)
                    {
                        _monitoredWindowHandles.Remove(windowInfo.Handle);
                    }
                }
                
                widget.Stop();
                _activeWidgets.Remove(widget);
            }
        }

        public void StopAllWidgets()
        {
            foreach (var widget in _activeWidgets.ToList())
            {
                StopWidget(widget);
            }
        }

        public void SaveWidgetStates()
        {
            _persistenceManager.SaveWidgetStates(_activeWidgets);
        }

        public void RestoreWidgetStates()
        {
            var states = _persistenceManager.LoadWidgetStates();
            
            foreach (var state in states)
            {
                try
                {
                    // Try to find existing window by exact title using cross-process detection
                    var existingWindowInfo = _persistenceManager.FindWidgetWindowByExactTitle(state.WindowTitle);
                    
                    if (existingWindowInfo != null)
                    {
                        // Found existing widget window, create external wrapper
                        var widgetType = Type.GetType(state.WidgetType);
                        if (widgetType == null)
                        {
                            // Try to find by simple name if full name fails
                            widgetType = _widgetFactories.Keys.FirstOrDefault(t => t.Name == state.WidgetType.Split('.').Last());
                        }

                        if (widgetType != null && _widgetFactories.ContainsKey(widgetType))
                        {
                            var widget = _widgetFactories[widgetType]();
                            
                            // Create external window wrapper
                            if (widget is WidgetBase widgetBase)
                            {
                                var externalWindowWrapper = new ExternalWindowWrapper(existingWindowInfo, widgetBase);
                                externalWindowWrapper.ReconnectToExistingWindow();
                                
                                // Add to active widgets
                                _activeWidgets.Add(externalWindowWrapper);
                                
                                // Restore only position from saved state, preserve original size
                                externalWindowWrapper.SetPosition(state.PositionX, state.PositionY);
                                
                                // Show or hide based on saved state
                                if (state.IsVisible)
                                {
                                    externalWindowWrapper.Show();
                                }
                                else
                                {
                                    externalWindowWrapper.Hide();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error restoring widget {state.WidgetType}: {ex.Message}");
                }
            }
        }

        public void ClearPersistedStates()
        {
            _persistenceManager.ClearPersistedStates();
        }

        private void MonitorExternalWindows(object? sender, EventArgs e)
        {
            var handlesToRemove = new List<IntPtr>();
            
            foreach (var handle in _monitoredWindowHandles)
            {
                if (!IsWindowValid(handle))
                {
                    handlesToRemove.Add(handle);
                }
            }
            
            foreach (var handle in handlesToRemove)
            {
                _monitoredWindowHandles.Remove(handle);
                
                // Find the corresponding widget and remove it
                var widgetToRemove = _activeWidgets.FirstOrDefault(w => 
                    w is ExternalWindowWrapper externalWrapper &&
                    GetWindowHandle(externalWrapper) == handle);
                
                if (widgetToRemove != null)
                {
                    System.Diagnostics.Debug.WriteLine($"External widget {widgetToRemove.Name} window closed, removing from active list");
                    
                    // Unsubscribe from events
                    widgetToRemove.WidgetClosed -= OnWidgetClosed;
                    
                    // Remove from active widgets
                    _activeWidgets.Remove(widgetToRemove);
                    
                    // Save the updated widget states
                    SaveWidgetStates();
                }
            }
        }
        
        private bool IsWindowValid(IntPtr handle)
        {
            return handle != IntPtr.Zero && User32.IsWindow(handle);
        }
        
        private IntPtr GetWindowHandle(ExternalWindowWrapper wrapper)
        {
            // Try to get the window handle from the wrapper
            var windowInfo = _persistenceManager.FindWidgetWindowByExactTitle(wrapper.Name);
            return windowInfo?.Handle ?? IntPtr.Zero;
        }

        private void OnWidgetClosed(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"OnWidgetClosed called with sender: {sender?.GetType().Name ?? "null"}");
            
            if (sender is IWidget widget && _activeWidgets.Contains(widget))
            {
                System.Diagnostics.Debug.WriteLine($"Removing widget {widget.Name} from active list");
                
                // Unsubscribe from events to prevent memory leaks
                widget.WidgetClosed -= OnWidgetClosed;
                
                // Remove from active widgets
                _activeWidgets.Remove(widget);
                
                // Save the updated widget states
                SaveWidgetStates();
                
                System.Diagnostics.Debug.WriteLine($"Widget {widget.Name} removed from active list. Active widgets count: {_activeWidgets.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Widget not found in active list or sender is null");
            }
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
