using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace WidgetDashboard.Models
{
    public class WidgetManager : INotifyPropertyChanged
    {
        private readonly Dictionary<Type, Func<IWidget>> _widgetFactories;
        private readonly ObservableCollection<IWidget> _activeWidgets;
        private readonly ObservableCollection<IWidget> _availableWidgets;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReadOnlyObservableCollection<IWidget> AvailableWidgets { get; }
        public ReadOnlyObservableCollection<IWidget> ActiveWidgets { get; }

        public WidgetManager()
        {
            _widgetFactories = new Dictionary<Type, Func<IWidget>>();
            _activeWidgets = new ObservableCollection<IWidget>();
            _availableWidgets = new ObservableCollection<IWidget>();

            AvailableWidgets = new ReadOnlyObservableCollection<IWidget>(_availableWidgets);
            ActiveWidgets = new ReadOnlyObservableCollection<IWidget>(_activeWidgets);

            RegisterWidgetTypes();
        }

        private void RegisterWidgetTypes()
        {
            RegisterWidget<ClockWidgetWrapper>();
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
