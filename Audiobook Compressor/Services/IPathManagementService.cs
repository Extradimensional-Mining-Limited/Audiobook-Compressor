/*
    Filename: IPathManagementService.cs
    Last Updated: 2025-08-09 19:45 CEST
    Version: 1.2.K
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Phase 4 Modularization per Focus 18.2.0: Path management service interface.
    Centralizes path operations and validation logic extracted from MainViewModel.
    Handles dialog coordination, validation, collision detection, and settings persistence.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service interface for managing path operations and validation
    /// Centralizes path management logic extracted from MainViewModel
    /// </summary>
    public interface IPathManagementService
    {
        #region Path Operations
        
        /// <summary>
        /// Executes source path browse operation with validation
        /// </summary>
        /// <param name="currentPath">Current source path</param>
        /// <returns>PathOperationResult with new path or validation errors</returns>
        Task<PathOperationResult> BrowseSourcePathAsync(string currentPath);
        
        /// <summary>
        /// Executes output path browse operation with validation
        /// </summary>
        /// <param name="currentPath">Current output path</param>
        /// <returns>PathOperationResult with new path or validation errors</returns>
        Task<PathOperationResult> BrowseOutputPathAsync(string currentPath);
        
        /// <summary>
        /// Saves the current output path as default
        /// </summary>
        /// <param name="outputPath">Path to save as default</param>
        /// <returns>OperationResult indicating success/failure</returns>
        Task<OperationResult> SaveDefaultOutputPathAsync(string outputPath);
        
        /// <summary>
        /// Restores the default output path
        /// </summary>
        /// <returns>PathOperationResult with default path or error</returns>
        Task<PathOperationResult> RestoreDefaultOutputPathAsync();
        
        #endregion
        
        #region Path Validation
        
        /// <summary>
        /// Validates source and output path combination
        /// </summary>
        /// <param name="sourcePath">Source library path</param>
        /// <param name="outputPath">Output folder path</param>
        /// <returns>PathValidationResult with validation outcome</returns>
        PathValidationResult ValidatePathCombination(string sourcePath, string outputPath);
        
        /// <summary>
        /// Checks for potential path collisions
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="outputPath">Output path</param>
        /// <returns>CollisionDetectionResult with findings</returns>
        CollisionDetectionResult CheckPathCollisions(string sourcePath, string outputPath);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Raised when path operations require user confirmation
        /// </summary>
        event EventHandler<PathConfirmationRequiredEventArgs>? ConfirmationRequired;
        
        /// <summary>
        /// Raised when path changes require external updates
        /// </summary>
        event EventHandler<PathChangedEventArgs>? PathChanged;
        
        #endregion
    }
    
    /// <summary>
    /// Result of path operations
    /// </summary>
    public class PathOperationResult
    {
        public bool Success { get; set; }
        public string? NewPath { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
    
    /// <summary>
    /// General operation result
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    
    /// <summary>
    /// Result of path validation operations
    /// </summary>
    public class PathValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
    
    /// <summary>
    /// Result of collision detection
    /// </summary>
    public class CollisionDetectionResult
    {
        public bool HasCollisions { get; set; }
        public List<string> Collisions { get; set; } = new();
        public bool RequiresUserConfirmation { get; set; }
        public string? ConfirmationMessage { get; set; }
    }
    
    /// <summary>
    /// Event arguments for path confirmation requests
    /// </summary>
    public class PathConfirmationRequiredEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool UserConfirmed { get; set; }
    }
    
    /// <summary>
    /// Event arguments for path change notifications
    /// </summary>
    public class PathChangedEventArgs : EventArgs
    {
        public string PathType { get; set; } = string.Empty; // "Source" or "Output"
        public string? NewPath { get; set; }
        public string? PreviousPath { get; set; }
        public bool RequiresCollisionCheck { get; set; }
    }
}