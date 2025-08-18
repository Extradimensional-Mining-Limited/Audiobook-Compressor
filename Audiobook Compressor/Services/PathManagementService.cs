/*
    Filename: PathManagementService.cs
    Last Updated: 2025-08-09 19:50 CEST
    Version: 1.2.K
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Phase 4 Modularization per Focus 18.2.0: Concrete path management service.
    Extracted complex path operations from MainViewModel (~100 lines).
    Handles dialog coordination, validation, collision detection, and settings persistence.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Concrete implementation of path management service
    /// Centralizes path operations and validation logic extracted from MainViewModel
    /// </summary>
    public class PathManagementService : IPathManagementService
    {
        #region Private Fields

        private readonly IDialogService _dialogService;
        private readonly IValidationService _validationService;
        private readonly ISettingsService _settingsService;

        #endregion

        #region Constructor

        public PathManagementService(
            IDialogService dialogService,
            IValidationService validationService,
            ISettingsService settingsService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        #endregion

        #region Path Operations

        /// <summary>
        /// Executes source path browse operation with validation
        /// </summary>
        /// <param name="currentPath">Current source path</param>
        /// <returns>PathOperationResult with new path or validation errors</returns>
        public async Task<PathOperationResult> BrowseSourcePathAsync(string currentPath)
        {
            var result = new PathOperationResult();

            try
            {
                // Show folder dialog
                var selectedPath = _dialogService.ShowFolderDialog(
                    "Select Source Library Folder",
                    currentPath ?? string.Empty);

                if (string.IsNullOrEmpty(selectedPath))
                {
                    // User cancelled
                    result.Success = false;
                    return result;
                }

                // Validate the selected path using ValidatePaths (single path validation)
                var pathValidation = _validationService.ValidatePaths(selectedPath, selectedPath);
                if (!pathValidation.IsValid)
                {
                    result.Success = false;
                    result.Errors.AddRange(pathValidation.Errors);
                    return result;
                }

                // Success
                result.Success = true;
                result.NewPath = selectedPath;

                // Raise path changed event
                var pathChangedArgs = new PathChangedEventArgs
                {
                    PathType = "Source",
                    NewPath = selectedPath,
                    PreviousPath = currentPath,
                    RequiresCollisionCheck = true
                };

                PathChanged?.Invoke(this, pathChangedArgs);

                System.Diagnostics.Debug.WriteLine($"PathManagementService: Source path browsed to '{selectedPath}'");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error browsing source path: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PathManagementService: Error browsing source path - {ex.Message}");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Executes output path browse operation with validation
        /// </summary>
        /// <param name="currentPath">Current output path</param>
        /// <returns>PathOperationResult with new path or validation errors</returns>
        public async Task<PathOperationResult> BrowseOutputPathAsync(string currentPath)
        {
            var result = new PathOperationResult();

            try
            {
                // Show folder dialog
                var selectedPath = _dialogService.ShowFolderDialog(
                    "Select Output Folder",
                    currentPath ?? string.Empty);

                if (string.IsNullOrEmpty(selectedPath))
                {
                    // User cancelled
                    result.Success = false;
                    return result;
                }

                // Validate the selected path using ValidatePaths (single path validation)
                var pathValidation = _validationService.ValidatePaths(selectedPath, selectedPath);
                if (!pathValidation.IsValid)
                {
                    result.Success = false;
                    result.Errors.AddRange(pathValidation.Errors);
                    return result;
                }

                // Success
                result.Success = true;
                result.NewPath = selectedPath;

                // Raise path changed event
                var pathChangedArgs = new PathChangedEventArgs
                {
                    PathType = "Output",
                    NewPath = selectedPath,
                    PreviousPath = currentPath,
                    RequiresCollisionCheck = true
                };

                PathChanged?.Invoke(this, pathChangedArgs);

                System.Diagnostics.Debug.WriteLine($"PathManagementService: Output path browsed to '{selectedPath}'");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error browsing output path: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PathManagementService: Error browsing output path - {ex.Message}");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Saves the current output path as default
        /// </summary>
        /// <param name="outputPath">Path to save as default</param>
        /// <returns>OperationResult indicating success/failure</returns>
        public async Task<OperationResult> SaveDefaultOutputPathAsync(string outputPath)
        {
            var result = new OperationResult();

            try
            {
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    result.Success = false;
                    result.Errors.Add("Output path cannot be empty");
                    return result;
                }

                // Save default path using settings service
                _settingsService.SetDefaultOutputPath(outputPath);

                result.Success = true;
                result.Message = "Default output path saved successfully.";

                System.Diagnostics.Debug.WriteLine($"PathManagementService: Default output path saved to '{outputPath}'");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error saving default output path: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PathManagementService: Error saving default output path - {ex.Message}");
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Restores the default output path
        /// </summary>
        /// <returns>PathOperationResult with default path or error</returns>
        public async Task<PathOperationResult> RestoreDefaultOutputPathAsync()
        {
            var result = new PathOperationResult();

            try
            {
                var defaultPath = _settingsService.GetDefaultOutputPath();
                
                if (string.IsNullOrWhiteSpace(defaultPath))
                {
                    result.Success = false;
                    result.Errors.Add("No default output path is configured");
                    return result;
                }

                // Validate the default path still exists using ValidatePaths
                var pathValidation = _validationService.ValidatePaths(defaultPath, defaultPath);
                if (!pathValidation.IsValid)
                {
                    result.Success = false;
                    result.Errors.Add("Default output path is no longer valid");
                    result.Errors.AddRange(pathValidation.Errors);
                    return result;
                }

                result.Success = true;
                result.NewPath = defaultPath;

                System.Diagnostics.Debug.WriteLine($"PathManagementService: Default output path restored to '{defaultPath}'");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error restoring default output path: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PathManagementService: Error restoring default output path - {ex.Message}");
            }

            return await Task.FromResult(result);
        }

        #endregion

        #region Path Validation

        /// <summary>
        /// Validates source and output path combination
        /// </summary>
        /// <param name="sourcePath">Source library path</param>
        /// <param name="outputPath">Output folder path</param>
        /// <returns>PathValidationResult with validation outcome</returns>
        public PathValidationResult ValidatePathCombination(string sourcePath, string outputPath)
        {
            var result = new PathValidationResult { IsValid = true };

            try
            {
                // Use existing validation service
                var pathValidationResult = _validationService.ValidatePaths(sourcePath, outputPath);
                
                result.IsValid = pathValidationResult.IsValid;
                result.Errors.AddRange(pathValidationResult.Errors);
                
                if (pathValidationResult.HasWarnings)
                {
                    result.Warnings.AddRange(pathValidationResult.Warnings);
                }

                System.Diagnostics.Debug.WriteLine(
                    $"PathManagementService: Path combination validation - " +
                    $"Source: '{sourcePath}', Output: '{outputPath}', Valid: {result.IsValid}");
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating path combination: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PathManagementService: Error validating path combination - {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Checks for potential path collisions
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="outputPath">Output path</param>
        /// <returns>CollisionDetectionResult with findings</returns>
        public CollisionDetectionResult CheckPathCollisions(string sourcePath, string outputPath)
        {
            var result = new CollisionDetectionResult();

            try
            {
                // Check if paths are the same (case-insensitive)
                if (string.Equals(sourcePath, outputPath, StringComparison.OrdinalIgnoreCase))
                {
                    result.HasCollisions = true;
                    result.RequiresUserConfirmation = true;
                    result.Collisions.Add("Source and Output folders are the same");
                    result.ConfirmationMessage = 
                        "Source and Output folders are the same. This may overwrite your source files.\n\n" +
                        "Do you want to continue with these paths?";
                }

                System.Diagnostics.Debug.WriteLine(
                    $"PathManagementService: Collision check - " +
                    $"Source: '{sourcePath}', Output: '{outputPath}', HasCollisions: {result.HasCollisions}");
            }
            catch (Exception ex)
            {
                result.HasCollisions = true;
                result.Collisions.Add($"Error checking path collisions: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PathManagementService: Error checking path collisions - {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when path operations require user confirmation
        /// </summary>
        public event EventHandler<PathConfirmationRequiredEventArgs>? ConfirmationRequired;

        /// <summary>
        /// Raised when path changes require external updates
        /// </summary>
        public event EventHandler<PathChangedEventArgs>? PathChanged;

        #endregion
    }
}