using System;

namespace FuturisticClockWidget.Models
{
    public enum ClockType
    {
        Digital,
        Analog
    }
    
    public enum HourMarkerMode
    {
        Cardinal,    // Only 12, 3, 6, 9
        Full         // All 12 hours
    }

    public class WidgetSettings
    {
        public WindowSettings Window { get; set; } = new WindowSettings();
        public ClockSettings Clock { get; set; } = new ClockSettings();
        public AppearanceSettings Appearance { get; set; } = new AppearanceSettings();
        public ApplicationSettings Application { get; set; } = new ApplicationSettings();
    }

    public class WindowSettings
    {
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 280;
        public double Height { get; set; } = 140;
        public bool Topmost { get; set; } = true;
        public bool ShowInTaskbar { get; set; } = false;
        public WindowSize PresetSize { get; set; } = WindowSize.Medium;
    }

    public class ClockSettings
    {
        public ClockType ClockType { get; set; } = ClockType.Digital;
        public bool Is24HourFormat { get; set; } = true;
        public HourMarkerMode HourMarkerMode { get; set; } = HourMarkerMode.Cardinal;
        public bool ShowSeconds { get; set; } = true;
        public bool ShowDate { get; set; } = true;
        public bool ShowWeekNumber { get; set; } = true;
        public bool ShowDayOfYear { get; set; } = true;
        public bool ShowTimeZone { get; set; } = true;
        public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;
    }

    public class AppearanceSettings
    {
        public string PrimaryColor { get; set; } = "#00D4FF";
        public string SecondaryColor { get; set; } = "#FF6B9D";
        public double Opacity { get; set; } = 0.9;
        public bool EnableGlassEffect { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public double FontScale { get; set; } = 1.0;
        public Theme Theme { get; set; } = Theme.Dark;
    }

    public class ApplicationSettings
    {
        public bool StartWithWindows { get; set; } = false;
        public bool CheckForUpdates { get; set; } = true;
        public bool EnableLogging { get; set; } = false;
        public string Language { get; set; } = "en-US";
        public bool MinimizeToTray { get; set; } = false;
        public bool ShowNotifications { get; set; } = true;
    }

    public enum WindowSize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        ExtraLarge = 3,
        Custom = 4
    }

    public enum Theme
    {
        Light,
        Dark,
        System,
        Custom
    }
}
