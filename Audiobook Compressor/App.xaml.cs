/*
    Filename: App.xaml.cs
    Last Updated: 2025-07-17 03:10 CEST
    Version: 1.0.3
    State: Stable
    Signed: User
    
    Synopsis:
    Application entry point code-behind.
    Fixed v1.0.2 broken dependency validation that prevented app from starting.
    Now properly displays missing tools with correct path information.
*/

using System.Configuration;
using System.Data;
using System.Windows;
using System.Linq;

namespace Audiobook_Compressor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Verify tools exist
            if (!Constants.VerifyToolsExist())
            {
                var missingTools = Constants.GetMissingTools().ToList();
                System.Windows.MessageBox.Show(
                    $"Required tools are missing:\n\nLooking in: {Constants.AppDirectory}\n\nMissing:\n{string.Join("\n", missingTools)}\n\nThe application will now close.",
                    "Missing Required Tools",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Current.Shutdown();
            }
        }
    }

}