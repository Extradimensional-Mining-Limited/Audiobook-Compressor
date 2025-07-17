/*
    Last Updated: 2025-07-17 00:06 CEST
    Version: 1.0.2
    State: Stable
    Signed: User
    
    Synopsis:
    Code-behind for the main window. Implements:
    - Status and progress bar functionality
    - Window size management for expanders
    - ComboBox focus handling
    - Basic process simulation
    Updated file header structure and documented in Summary.md.
*/
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Audiobook_Compressor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double _statusProgress;
        private bool _isProgressVisible;
        private string _statusText = "Ready";

        public double StatusProgress
        {
            get => _statusProgress;
            set { _statusProgress = value; OnPropertyChanged(); }
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set { _isProgressVisible = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Add handlers for both expanders
            SettingsExpander.Expanded += Expander_ExpandedCollapsed;
            SettingsExpander.Collapsed += Expander_ExpandedCollapsed;
            LogExpander.Expanded += Expander_ExpandedCollapsed;
            LogExpander.Collapsed += Expander_ExpandedCollapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Helper method to update both progress and status
        public void UpdateStatus(string message, double? progress = null)
        {
            StatusText = message;
            if (progress.HasValue)
            {
                StatusProgress = progress.Value;
                IsProgressVisible = true;
            }
            else
            {
                IsProgressVisible = false;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable start button during test
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            try
            {
                // Show initial status
                UpdateStatus("Starting process...", 0);

                // Simulate some work with progress updates
                for (int i = 1; i <= 10; i++)
                {
                    // Simulate work
                    await Task.Delay(500);  // Half second delay
                    
                    // Update progress (convert to 0-1 range)
                    UpdateStatus($"Processing step {i} of 10...", i / 10.0);
                }

                // Show completion with full progress bar
                UpdateStatus("Process completed successfully!", 0);
                
                // Optional: Hide progress after a delay
                // await Task.Delay(1000);  // Show completed progress for 1 second
                // UpdateStatus("Ready");
            }
            finally
            {
                // Re-enable start button
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        private void Expander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            // Delay the resize slightly to allow animation to complete
            Dispatcher.BeginInvoke(() =>
            {
                InvalidateVisual();
                UpdateLayout();
                
                if (WindowState == WindowState.Normal)
                {
                    // Force height recalculation
                    Height = ActualHeight;
                    SizeToContent = SizeToContent.Height;
                }
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        private void EditableTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && 
                textBox.TemplatedParent is ComboBox comboBox)
            {
                // Update the ComboBox text when focus is lost
                comboBox.Text = textBox.Text;
                // Close the dropdown if it's open
                comboBox.IsDropDownOpen = false;
            }
        }
    }
}