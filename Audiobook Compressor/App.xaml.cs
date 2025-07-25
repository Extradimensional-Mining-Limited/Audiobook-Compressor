/*
    Filename: App.xaml.cs
    Last Updated: 2025-07-25 03:32
    Version: 1.2.0
    State: Stable
    Signed: User

    Synopsis:
    - All file headers updated to v1.2.0, state Stable, signed User, with unified timestamp.
    - Documentation and changelog discipline enforced for release.
    - No code changes since last version except header and documentation updates.
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