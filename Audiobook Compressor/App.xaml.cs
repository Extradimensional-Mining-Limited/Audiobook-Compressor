/*
    Last Updated: 2025-07-17 00:06 CEST
    Version: 1.0.2
    State: Stable
    Signed: User
    
    Synopsis:
    Application entry point code-behind.
    Currently implements basic application lifecycle management.
    Updated file header structure and documented in Summary.md
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
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Verify tools exist
            if (!Constants.VerifyToolsExist())
            {
                var missingTools = Constants.GetMissingTools().ToList();
                MessageBox.Show(
                    $"Required tools are missing:\n\n{string.Join("\n", missingTools)}\n\nThe application will now close.",
                    "Missing Required Tools",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Current.Shutdown();
            }
        }
    }

}