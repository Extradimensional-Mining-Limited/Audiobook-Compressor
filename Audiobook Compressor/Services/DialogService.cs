/*
    Filename: DialogService.cs
    Last Updated: 2025-08-09 10:10 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of IDialogService, abstracting dialog operations from UI logic per Focus 13.1.0 Phase 2.
*/

using System;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for displaying dialogs and user interactions
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// Shows a folder selection dialog
        /// </summary>
        public string? ShowFolderDialog(string description, string? selectedPath = null)
        {
            try
            {
                using var dialog = new WinForms.FolderBrowserDialog
                {
                    Description = description,
                    UseDescriptionForTitle = true
                };

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    dialog.SelectedPath = selectedPath;
                }

                var result = dialog.ShowDialog();
                return result == WinForms.DialogResult.OK ? dialog.SelectedPath : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing folder dialog: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Shows a confirmation dialog with Yes/No options
        /// </summary>
        public bool ShowConfirmationDialog(string message, string title)
        {
            try
            {
                var result = System.Windows.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                return result == MessageBoxResult.Yes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing confirmation dialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shows a warning dialog
        /// </summary>
        public void ShowWarningDialog(string message, string title)
        {
            try
            {
                System.Windows.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing warning dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an error dialog
        /// </summary>
        public void ShowErrorDialog(string message, string title)
        {
            try
            {
                System.Windows.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an information dialog
        /// </summary>
        public void ShowInformationDialog(string message, string title)
        {
            try
            {
                System.Windows.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing information dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a dialog asking user to choose between options with custom buttons
        /// </summary>
        public int ShowChoiceDialog(string message, string title, params string[] options)
        {
            try
            {
                if (options == null || options.Length == 0)
                    return -1;

                // For simple two-option cases, use standard MessageBox
                if (options.Length == 2)
                {
                    var result = System.Windows.MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    return result == MessageBoxResult.Yes ? 0 : (result == MessageBoxResult.No ? 1 : -1);
                }

                // For more complex cases, use the first option as default action
                // This is a simplified implementation - a full implementation would use a custom dialog
                var confirmMessage = $"{message}\n\nChoose '{options[0]}' to continue, or Cancel to abort.";
                var confirmResult = System.Windows.MessageBox.Show(
                    confirmMessage,
                    title,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                return confirmResult == MessageBoxResult.OK ? 0 : -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing choice dialog: {ex.Message}");
                return -1;
            }
        }
    }
}