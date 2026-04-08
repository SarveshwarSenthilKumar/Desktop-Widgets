using System;
using System.IO;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace FuturisticClockWidget.Services
{
    public static class StartupManager
    {
        private const string AppName = "FuturisticClockWidget";
        private const string TaskName = "FuturisticClockWidgetStartup";

        public static bool IsStartupEnabled()
        {
            try
            {
                // Check Task Scheduler first (preferred method)
                using (var ts = new TaskService())
                {
                    var task = ts.GetTask(TaskName);
                    if (task != null)
                    {
                        return task.Enabled;
                    }
                }

                // Fallback to registry check
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(AppName) as string;
                        return !string.IsNullOrEmpty(value) && File.Exists(value);
                    }
                }
            }
            catch
            {
                // If we can't check, assume it's not enabled
            }

            return false;
        }

        public static void EnableStartup()
        {
            try
            {
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                
                // Try Task Scheduler first (more reliable)
                if (EnableStartupViaTaskScheduler(appPath))
                {
                    return;
                }

                // Fallback to registry
                EnableStartupViaRegistry(appPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to enable startup", ex);
            }
        }

        public static void DisableStartup()
        {
            try
            {
                // Try Task Scheduler first
                DisableStartupViaTaskScheduler();

                // Also remove from registry (cleanup)
                DisableStartupViaRegistry();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to disable startup", ex);
            }
        }

        private static bool EnableStartupViaTaskScheduler(string appPath)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    // Remove existing task if any
                    var existingTask = ts.GetTask(TaskName);
                    if (existingTask != null)
                    {
                        ts.RootFolder.DeleteTask(TaskName);
                    }

                    // Create new task
                    var td = ts.NewTask();
                    td.RegistrationInfo.Description = "Start Futuristic Clock Widget on Windows startup";
                    td.RegistrationInfo.Author = AppName;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.StartWhenAvailable = true;
                    td.Settings.RunOnlyIfNetworkAvailable = false;
                    td.Settings.AllowHardTerminate = true;
                    td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.Normal;

                    // Trigger at logon
                    td.Triggers.Add(new LogonTrigger { Enabled = true });

                    // Action to start the application
                    td.Actions.Add(new ExecAction(appPath, null, Path.GetDirectoryName(appPath)));

                    // Register the task
                    ts.RootFolder.RegisterTaskDefinition(TaskName, td);
                    
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void DisableStartupViaTaskScheduler()
        {
            try
            {
                using (var ts = new TaskService())
                {
                    var task = ts.GetTask(TaskName);
                    if (task != null)
                    {
                        ts.RootFolder.DeleteTask(TaskName);
                    }
                }
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }

        private static void EnableStartupViaRegistry(string appPath)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue(AppName, $"\"{appPath}\"");
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static void DisableStartupViaRegistry()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch
            {
                // Ignore if the key doesn't exist
            }
        }

        public static bool TestStartupAccess()
        {
            try
            {
                // Test if we can write to Task Scheduler
                using (var ts = new TaskService())
                {
                    // This will throw if we don't have permissions
                    var tasks = ts.AllTasks;
                    return true;
                }
            }
            catch
            {
                try
                {
                    // Test if we can write to registry
                    using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        return key != null;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
