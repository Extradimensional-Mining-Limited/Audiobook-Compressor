/*
    Filename: App.xaml.cs
    Last Updated: 2025-08-09 13:55 CEST
    Version: 1.2.H
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Enhanced dependency injection container with settings persistence on application exit per Focus 15.4.0 implementation.
    MainViewModel is now Singleton for graceful shutdown access, with OnExit calling settings persistence.
*/

using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Audiobook_Compressor.Services;
using Audiobook_Compressor.ViewModels;

namespace Audiobook_Compressor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;

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
                return;
            }

            // Configure dependency injection
            ConfigureServices();

            // Create and show main window with proper ViewModel
            var mainWindow = new MainWindow();
            var mainViewModel = _serviceProvider?.GetRequiredService<MainViewModel>();
            
            if (mainViewModel != null)
            {
                mainWindow.DataContext = mainViewModel;
            }
            
            mainWindow.Show();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddTransient<IAudioService, AudioService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IValidationService, ValidationService>();

            // Register ViewModels - Singleton for application exit access
            services.AddSingleton<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Get MainViewModel from DI container and call graceful shutdown
                var mainViewModel = _serviceProvider?.GetService<MainViewModel>();
                mainViewModel?.OnApplicationExit();
            }
            catch (Exception ex)
            {
                // Log error but allow application to exit
                System.Diagnostics.Debug.WriteLine($"Error during application shutdown: {ex.Message}");
            }
            finally
            {
                // Always cleanup DI container
                _serviceProvider?.Dispose();
                base.OnExit(e);
            }
        }
    }
}