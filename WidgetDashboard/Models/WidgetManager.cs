using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WidgetDashboard.Models
{
    public class WidgetManager : INotifyPropertyChanged
    {
        private readonly Dictionary<Type, Func<IWidget>> _widgetFactories;
        private readonly ObservableCollection<IWidget> _activeWidgets;
        private readonly ObservableCollection<IWidget> _availableWidgets;
        private readonly WidgetPersistenceManager _persistenceManager;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReadOnlyObservableCollection<IWidget> AvailableWidgets { get; }
        public ReadOnlyObservableCollection<IWidget> ActiveWidgets { get; }

        public WidgetManager()
        {
            _widgetFactories = new Dictionary<Type, Func<IWidget>>();
            _activeWidgets = new ObservableCollection<IWidget>();
            _availableWidgets = new ObservableCollection<IWidget>();
            _persistenceManager = new WidgetPersistenceManager();

            AvailableWidgets = new ReadOnlyObservableCollection<IWidget>(_availableWidgets);
            ActiveWidgets = new ReadOnlyObservableCollection<IWidget>(_activeWidgets);

            RegisterWidgetTypes();
        }

        private void RegisterWidgetTypes()
        {
            RegisterWidget<ClockWidgetWrapper>();
            RegisterWidget<CalendarWidgetWrapper>();
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
            }
        }

        public void StopWidget(IWidget widget)
        {
            if (widget != null && _activeWidgets.Contains(widget))
            {
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
