/*
    Filename: IAudioService.cs
    Last Updated: 2025-08-09 10:05 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Service interface for audio processing operations, abstracting AudioProcessor functionality for MVVM architecture per Focus 13.1.0 Phase 1.
    Uses existing event argument classes from AudioProcessor.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing audio processing operations
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Event fired when processing progress changes
        /// </summary>
        event EventHandler<AudioProcessingProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Event fired when a file is processed
        /// </summary>
        event EventHandler<AudioFileProcessedEventArgs> FileProcessed;

        /// <summary>
        /// Scans a directory for supported audio files
        /// </summary>
        /// <param name="directoryPath">Directory to scan</param>
        /// <returns>Enumerable of found audio files</returns>
        IAsyncEnumerable<AudioFileInfo> ScanDirectoryAsync(string directoryPath);

        /// <summary>
        /// Processes a list of audio files
        /// </summary>
        /// <param name="files">Files to process</param>
        /// <param name="outputPath">Output directory</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the processing operation</returns>
        Task ProcessFilesAsync(List<AudioFileInfo> files, string outputPath, CancellationToken cancellationToken);

        /// <summary>
        /// Processes a single audio file
        /// </summary>
        /// <param name="file">File to process</param>
        /// <param name="outputPath">Output directory</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the processing operation</returns>
        Task ProcessFileAsync(AudioFileInfo file, string outputPath, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels any running processing operations
        /// </summary>
        void CancelProcessing();

        /// <summary>
        /// Gets whether processing is currently running
        /// </summary>
        bool IsProcessing { get; }
    }
}