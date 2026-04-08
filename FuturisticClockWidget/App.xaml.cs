using System;
using System.Windows;
using FuturisticClockWidget.Services;

namespace FuturisticClockWidget
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Handle startup logic
            try
            {
                var settings = SettingsManager.Current;
                
                // Check if startup setting has changed
                bool currentStartupStatus = StartupManager.IsStartupEnabled();
                if (settings.Application.StartWithWindows != currentStartupStatus)
                {
                    if (settings.Application.StartWithWindows)
                    {
                        StartupManager.EnableStartup();
                    }
                    else
                    {
                        StartupManager.DisableStartup();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if logging is enabled
                if (SettingsManager.Current?.Application.EnableLogging == true)
                {
                    // Simple logging to app data directory
                    try
                    {
                        string logPath = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "FuturisticClockWidget", "startup.log");
                        string? logDir = System.IO.Path.GetDirectoryName(logPath);
                        if (!string.IsNullOrEmpty(logDir))
                        {
                            System.IO.Directory.CreateDirectory(logDir);
                        }
                        System.IO.File.AppendAllText(logPath, 
                            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Startup error: {ex.Message}\n");
                    }
                    catch
                    {
                        // If logging fails, continue silently
                    }
                }
            }
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            // Save any final settings
            try
            {
                SettingsManager.SaveSettings();
            }
            catch
            {
                // Ignore errors during shutdown
            }
            
            base.OnExit(e);
        }
    }
}
