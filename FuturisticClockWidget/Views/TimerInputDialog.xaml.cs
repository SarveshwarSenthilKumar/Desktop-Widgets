using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FuturisticClockWidget.Views
{
    public partial class TimerInputDialog : Window
    {
        public TimeSpan SelectedTime { get; private set; }

        public TimerInputDialog(TimeSpan initialTime)
        {
            InitializeComponent();
            SelectedTime = initialTime;
            
            // Set initial values
            HoursTextBox.Text = ((int)initialTime.TotalHours).ToString();
            MinutesTextBox.Text = initialTime.Minutes.ToString();
            SecondsTextBox.Text = initialTime.Seconds.ToString();
            
            // Select all text in minutes textbox for easy editing
            MinutesTextBox.SelectAll();
            MinutesTextBox.Focus();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int hours = string.IsNullOrEmpty(HoursTextBox.Text) ? 0 : int.Parse(HoursTextBox.Text);
                int minutes = string.IsNullOrEmpty(MinutesTextBox.Text) ? 0 : int.Parse(MinutesTextBox.Text);
                int seconds = string.IsNullOrEmpty(SecondsTextBox.Text) ? 0 : int.Parse(SecondsTextBox.Text);
                
                // Validate ranges
                if (hours < 0 || hours > 23)
                {
                    MessageBox.Show("Hours must be between 0 and 23", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    HoursTextBox.Focus();
                    return;
                }
                
                if (minutes < 0 || minutes > 59)
                {
                    MessageBox.Show("Minutes must be between 0 and 59", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MinutesTextBox.Focus();
                    return;
                }
                
                if (seconds < 0 || seconds > 59)
                {
                    MessageBox.Show("Seconds must be between 0 and 59", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SecondsTextBox.Focus();
                    return;
                }
                
                // Ensure at least some time is set
                if (hours == 0 && minutes == 0 && seconds == 0)
                {
                    MessageBox.Show("Please set a timer duration greater than 0", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                SelectedTime = new TimeSpan(hours, minutes, seconds);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void QuickPreset1Min_Click(object sender, RoutedEventArgs e)
        {
            SetTimeValues(0, 1, 0);
        }

        private void QuickPreset5Min_Click(object sender, RoutedEventArgs e)
        {
            SetTimeValues(0, 5, 0);
        }

        private void QuickPreset10Min_Click(object sender, RoutedEventArgs e)
        {
            SetTimeValues(0, 10, 0);
        }

        private void QuickPreset15Min_Click(object sender, RoutedEventArgs e)
        {
            SetTimeValues(0, 15, 0);
        }

        private void QuickPreset30Min_Click(object sender, RoutedEventArgs e)
        {
            SetTimeValues(0, 30, 0);
        }

        private void QuickPreset1Hour_Click(object sender, RoutedEventArgs e)
        {
            SetTimeValues(1, 0, 0);
        }

        private void SetTimeValues(int hours, int minutes, int seconds)
        {
            HoursTextBox.Text = hours.ToString();
            MinutesTextBox.Text = minutes.ToString();
            SecondsTextBox.Text = seconds.ToString();
        }
    }
}
