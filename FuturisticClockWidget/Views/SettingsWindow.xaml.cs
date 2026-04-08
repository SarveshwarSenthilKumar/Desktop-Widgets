using System;
using System.Windows;
using FuturisticClockWidget.Models;
using FuturisticClockWidget.Services;

namespace FuturisticClockWidget.Views
{
    public partial class SettingsWindow : Window
    {
        private WidgetSettings? _originalSettings;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Create a copy of current settings
            _originalSettings = CloneSettings(SettingsManager.Current) ?? new WidgetSettings();

            // Apply settings to UI
            StartupCheckBox.IsChecked = _originalSettings.Application.StartWithWindows;
            HourFormat24CheckBox.IsChecked = _originalSettings.Clock.Is24HourFormat;
            ShowSecondsCheckBox.IsChecked = _originalSettings.Clock.ShowSeconds;
            ShowDateCheckBox.IsChecked = _originalSettings.Clock.ShowDate;
            ShowWeekNumberCheckBox.IsChecked = _originalSettings.Clock.ShowWeekNumber;
            ShowDayOfYearCheckBox.IsChecked = _originalSettings.Clock.ShowDayOfYear;
            ShowTimeZoneCheckBox.IsChecked = _originalSettings.Clock.ShowTimeZone;

            OpacitySlider.Value = _originalSettings.Appearance.Opacity;
            OpacityValueText.Text = $"{(int)(_originalSettings.Appearance.Opacity * 100)}%";

            FontScaleSlider.Value = _originalSettings.Appearance.FontScale;
            FontScaleValueText.Text = $"{(int)(_originalSettings.Appearance.FontScale * 100)}%";

            GlassEffectCheckBox.IsChecked = _originalSettings.Appearance.EnableGlassEffect;
            AnimationsCheckBox.IsChecked = _originalSettings.Appearance.EnableAnimations;

            TopmostCheckBox.IsChecked = _originalSettings.Window.Topmost;
            ShowInTaskbarCheckBox.IsChecked = _originalSettings.Window.ShowInTaskbar;

            LoggingCheckBox.IsChecked = _originalSettings.Application.EnableLogging;
            UpdatesCheckBox.IsChecked = _originalSettings.Application.CheckForUpdates;
            NotificationsCheckBox.IsChecked = _originalSettings.Application.ShowNotifications;

            // Add event handlers
            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;
            FontScaleSlider.ValueChanged += FontScaleSlider_ValueChanged;
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OpacityValueText != null)
            {
                OpacityValueText.Text = $"{(int)(e.NewValue * 100)}%";
            }
        }

        private void FontScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FontScaleValueText != null)
            {
                FontScaleValueText.Text = $"{(int)(e.NewValue * 100)}%";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Apply UI values to settings
                SettingsManager.Current.Application.StartWithWindows = StartupCheckBox.IsChecked ?? false;
                SettingsManager.Current.Clock.Is24HourFormat = HourFormat24CheckBox.IsChecked ?? true;
                SettingsManager.Current.Clock.ShowSeconds = ShowSecondsCheckBox.IsChecked ?? true;
                SettingsManager.Current.Clock.ShowDate = ShowDateCheckBox.IsChecked ?? true;
                SettingsManager.Current.Clock.ShowWeekNumber = ShowWeekNumberCheckBox.IsChecked ?? true;
                SettingsManager.Current.Clock.ShowDayOfYear = ShowDayOfYearCheckBox.IsChecked ?? true;
                SettingsManager.Current.Clock.ShowTimeZone = ShowTimeZoneCheckBox.IsChecked ?? true;

                SettingsManager.Current.Appearance.Opacity = OpacitySlider.Value;
                SettingsManager.Current.Appearance.FontScale = FontScaleSlider.Value;
                SettingsManager.Current.Appearance.EnableGlassEffect = GlassEffectCheckBox.IsChecked ?? true;
                SettingsManager.Current.Appearance.EnableAnimations = AnimationsCheckBox.IsChecked ?? true;

                SettingsManager.Current.Window.Topmost = TopmostCheckBox.IsChecked ?? true;
                SettingsManager.Current.Window.ShowInTaskbar = ShowInTaskbarCheckBox.IsChecked ?? false;

                SettingsManager.Current.Application.EnableLogging = LoggingCheckBox.IsChecked ?? false;
                SettingsManager.Current.Application.CheckForUpdates = UpdatesCheckBox.IsChecked ?? true;
                SettingsManager.Current.Application.ShowNotifications = NotificationsCheckBox.IsChecked ?? true;

                // Save settings
                SettingsManager.SaveSettings();

                // Show success message
                MessageBox.Show("Settings saved successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to their default values? This action cannot be undone.",
                "Reset Settings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Reset to defaults
                    SettingsManager.ResetToDefaults();
                    
                    // Reload UI with defaults
                    LoadSettings();
                    
                    MessageBox.Show("Settings have been reset to defaults.", "Reset Complete", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to reset settings: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private WidgetSettings CloneSettings(WidgetSettings original)
        {
            // Simple manual clone since these are simple POCOs
            return new WidgetSettings
            {
                Window = new WindowSettings
                {
                    Left = original.Window.Left,
                    Top = original.Window.Top,
                    Width = original.Window.Width,
                    Height = original.Window.Height,
                    Topmost = original.Window.Topmost,
                    ShowInTaskbar = original.Window.ShowInTaskbar,
                    PresetSize = original.Window.PresetSize
                },
                Clock = new ClockSettings
                {
                    ClockType = original.Clock.ClockType,
                    Is24HourFormat = original.Clock.Is24HourFormat,
                    HourMarkerMode = original.Clock.HourMarkerMode,
                    ShowSeconds = original.Clock.ShowSeconds,
                    ShowDate = original.Clock.ShowDate,
                    ShowWeekNumber = original.Clock.ShowWeekNumber,
                    ShowDayOfYear = original.Clock.ShowDayOfYear,
                    ShowTimeZone = original.Clock.ShowTimeZone,
                    TimeZoneId = original.Clock.TimeZoneId
                },
                Appearance = new AppearanceSettings
                {
                    PrimaryColor = original.Appearance.PrimaryColor,
                    SecondaryColor = original.Appearance.SecondaryColor,
                    Opacity = original.Appearance.Opacity,
                    EnableGlassEffect = original.Appearance.EnableGlassEffect,
                    EnableAnimations = original.Appearance.EnableAnimations,
                    FontScale = original.Appearance.FontScale,
                    Theme = original.Appearance.Theme
                },
                Application = new ApplicationSettings
                {
                    StartWithWindows = original.Application.StartWithWindows,
                    CheckForUpdates = original.Application.CheckForUpdates,
                    EnableLogging = original.Application.EnableLogging,
                    Language = original.Application.Language,
                    MinimizeToTray = original.Application.MinimizeToTray,
                    ShowNotifications = original.Application.ShowNotifications
                }
            };
        }
    }
}
