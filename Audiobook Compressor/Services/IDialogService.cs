/*
    Filename: IDialogService.cs
    Last Updated: 2025-08-09 10:05 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for dialog operations, abstracting MessageBox and folder dialogs from MainWindow for MVVM architecture per Focus 13.1.0 Phase 1.
*/

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for displaying dialogs and user interactions
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a folder selection dialog
        /// </summary>
        /// <param name="description">Dialog description text</param>
        /// <param name="selectedPath">Initially selected path</param>
        /// <returns>Selected folder path or null if cancelled</returns>
        string? ShowFolderDialog(string description, string? selectedPath = null);

        /// <summary>
        /// Shows a confirmation dialog with Yes/No options
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Dialog title</param>
        /// <returns>True if user clicked Yes/OK, false otherwise</returns>
        bool ShowConfirmationDialog(string message, string title);

        /// <summary>
        /// Shows a warning dialog
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="title">Dialog title</param>
        void ShowWarningDialog(string message, string title);

        /// <summary>
        /// Shows an error dialog
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="title">Dialog title</param>
        void ShowErrorDialog(string message, string title);

        /// <summary>
        /// Shows an information dialog
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="title">Dialog title</param>
        void ShowInformationDialog(string message, string title);

        /// <summary>
        /// Shows a dialog asking user to choose between options with custom buttons
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Dialog title</param>
        /// <param name="options">Array of option strings</param>
        /// <returns>Index of selected option, -1 if cancelled</returns>
        int ShowChoiceDialog(string message, string title, params string[] options);
    }
}