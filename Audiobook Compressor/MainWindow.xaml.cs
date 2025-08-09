/*
    Filename: MainWindow.xaml.cs
    Last Updated: 2025-08-09 12:35 CEST
    Version: 1.2.G
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Minimal MVVM code-behind per Focus 14.1.0 Phase 6. All UI logic moved to MainViewModel with data binding.
    Only essential view-specific functionality retained that cannot be handled through binding.
*/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Audiobook_Compressor.ViewModels;

namespace Audiobook_Compressor
{
    /// <summary>
    /// MainWindow with minimal code-behind - all logic handled by MainViewModel via data binding
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // DataContext is set by App.xaml.cs via dependency injection
            // No event handlers or UI logic - everything handled by MainViewModel binding
            
            // Only view-specific logic that cannot be bound
            SettingsExpander.Expanded += Expander_ExpandedCollapsed;
            SettingsExpander.Collapsed += Expander_ExpandedCollapsed;
            LogExpander.Expanded += Expander_ExpandedCollapsed;
            LogExpander.Collapsed += Expander_ExpandedCollapsed;
        }

        /// <summary>
        /// View-specific logic for expander animations - cannot be handled by data binding
        /// </summary>
        private void Expander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            // Window resizing logic for expander animations
            Dispatcher.BeginInvoke(() =>
            {
                InvalidateVisual();
                UpdateLayout();
                
                if (WindowState == WindowState.Normal)
                {
                    Height = ActualHeight;
                    SizeToContent = SizeToContent.Height;
                }
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        #region Legacy INotifyPropertyChanged (kept for compatibility)
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
        }

        #endregion
    }
}
