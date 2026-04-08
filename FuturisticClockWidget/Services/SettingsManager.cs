using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using FuturisticClockWidget.Models;

namespace FuturisticClockWidget.Services
{
    public class SettingsManager
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FuturisticClockWidget");
        
        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        private static WidgetSettings? _instance;
        private static readonly object _lock = new object();

        public static WidgetSettings Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadSettings();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void SaveSettings()
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(SettingsDirectory);

                // Create a backup of existing settings
                if (File.Exists(SettingsFilePath))
                {
                    string backupPath = Path.Combine(SettingsDirectory, $"settings.backup.{DateTime.Now:yyyyMMddHHmmss}.json");
                    File.Copy(SettingsFilePath, backupPath);
                }

                // Save current settings
                string json = JsonSerializer.Serialize(_instance, JsonOptions);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error if logging is enabled
                if (Current?.Application.EnableLogging == true)
                {
                    LogError("Failed to save settings", ex);
                }
            }
        }

        public static void ResetToDefaults()
        {
            lock (_lock)
            {
                _instance = new WidgetSettings();
                SaveSettings();
            }
        }

        private static WidgetSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<WidgetSettings>(json, JsonOptions);
                    
                    // Validate and fix any invalid settings
                    return ValidateAndFixSettings(settings ?? new WidgetSettings());
                }
            }
            catch (Exception ex)
            {
                // Log error if logging is enabled
                try
                {
                    if (File.Exists(SettingsFilePath))
                    {
                        var tempSettings = JsonSerializer.Deserialize<WidgetSettings>(
                            File.ReadAllText(SettingsFilePath), JsonOptions);
                        if (tempSettings?.Application.EnableLogging == true)
                        {
                            LogError("Failed to load settings, using defaults", ex);
                        }
                    }
                }
                catch
                {
                    // If we can't even check the logging setting, just continue
                }
            }

            // Return default settings if loading fails
            return new WidgetSettings();
        }

        private static WidgetSettings ValidateAndFixSettings(WidgetSettings settings)
        {
            // Ensure window position is valid
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (settings.Window.Left < 0 || settings.Window.Left >= screenWidth)
                settings.Window.Left = 100;
            
            if (settings.Window.Top < 0 || settings.Window.Top >= screenHeight)
                settings.Window.Top = 100;

            // Ensure window size is reasonable
            settings.Window.Width = Math.Max(150, Math.Min(1920, settings.Window.Width));
            settings.Window.Height = Math.Max(80, Math.Min(1080, settings.Window.Height));

            // Ensure opacity is within valid range
            settings.Appearance.Opacity = Math.Max(0.1, Math.Min(1.0, settings.Appearance.Opacity));

            // Ensure font scale is reasonable and never 0
            settings.Appearance.FontScale = Math.Max(0.5, Math.Min(3.0, settings.Appearance.FontScale));
            if (settings.Appearance.FontScale <= 0)
                settings.Appearance.FontScale = 1.0; // Default to 1.0 if invalid

            // Validate time zone
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(settings.Clock.TimeZoneId);
            }
            catch
            {
                settings.Clock.TimeZoneId = TimeZoneInfo.Local.Id;
            }

            return settings;
        }

        private static void LogError(string message, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(SettingsDirectory, "error.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}: {ex.Message}\n{ex.StackTrace}\n\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // If logging fails, there's nothing we can do
            }
        }

        public static void ExportSettings(string filePath)
        {
            try
            {
                string json = JsonSerializer.Serialize(_instance, JsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                LogError("Failed to export settings", ex);
                throw;
            }
        }

        public static void ImportSettings(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Settings file not found", filePath);

                string json = File.ReadAllText(filePath);
                var importedSettings = JsonSerializer.Deserialize<WidgetSettings>(json, JsonOptions);
                
                if (importedSettings != null)
                {
                    lock (_lock)
                    {
                        _instance = ValidateAndFixSettings(importedSettings);
                        SaveSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to import settings", ex);
                throw;
            }
        }
    }
}
