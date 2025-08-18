/*
    Filename: App.xaml.cs
    Last Updated: 2025-08-09 16:00 CEST
    Version: 1.2.J
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Enhanced dependency injection container with Phase 1 modularization services per Focus 16.2.0 implementation.
    Added IUIStateService and IPanelVisibilityService registration for MainViewModel modularization.
    Phase 2 enhancement per Focus 17.2.0: Added ISettingsBindingService registration for complex settings validation.
    Maintains settings persistence on application exit and graceful shutdown management.
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

            // Register Phase 1 Modularization Services
            services.AddSingleton<IUIStateService, UIStateService>();
            services.AddSingleton<IPanelVisibilityService, PanelVisibilityService>();

            // Register Phase 2 Modularization Services
            services.AddSingleton<ISettingsBindingService, SettingsBindingService>();

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