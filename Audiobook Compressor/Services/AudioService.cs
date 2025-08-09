/*
    Filename: AudioService.cs
    Last Updated: 2025-08-09 10:10 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Concrete implementation of IAudioService, wrapping AudioProcessor for MVVM architecture per Focus 13.1.0 Phase 2.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Service for managing audio processing operations, wrapping AudioProcessor for MVVM architecture
    /// </summary>
    public class AudioService : IAudioService
    {
        private AudioProcessor? _audioProcessor;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isProcessing;

        /// <summary>
        /// Event fired when processing progress changes
        /// </summary>
        public event EventHandler<AudioProcessingProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Event fired when a file is processed
        /// </summary>
        public event EventHandler<AudioFileProcessedEventArgs>? FileProcessed;

        /// <summary>
        /// Gets whether processing is currently running
        /// </summary>
        public bool IsProcessing => _isProcessing;

        /// <summary>
        /// Scans a directory for supported audio files
        /// </summary>
        public async IAsyncEnumerable<AudioFileInfo> ScanDirectoryAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                yield break;

            _cancellationTokenSource ??= new CancellationTokenSource();
            
            try
            {
                _audioProcessor = new AudioProcessor(_cancellationTokenSource.Token);
                
                await foreach (var file in _audioProcessor.ScanDirectoryAsync(directoryPath))
                {
                    yield return file;
                }
            }
            finally
            {
                // Don't dispose here as we might need the processor for processing
            }
        }

        /// <summary>
        /// Processes a list of audio files
        /// </summary>
        public async Task ProcessFilesAsync(List<AudioFileInfo> files, string outputPath, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0 || string.IsNullOrWhiteSpace(outputPath))
                return;

            _isProcessing = true;
            
            try
            {
                // Create a linked cancellation token that responds to both the provided token and our internal one
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                
                _audioProcessor = new AudioProcessor(_cancellationTokenSource.Token);
                
                // Subscribe to events and forward them
                _audioProcessor.ProgressChanged += OnProgressChanged;
                _audioProcessor.FileProcessed += OnFileProcessed;

                var totalFiles = files.Count;
                var processed = 0;

                foreach (var file in files)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    // Report overall progress
                    var overallProgress = processed / (double)totalFiles;
                    ProgressChanged?.Invoke(this, new AudioProcessingProgressEventArgs(file, overallProgress));

                    await _audioProcessor.ProcessAudioFileAsync(file, outputPath);
                    processed++;
                }
            }
            finally
            {
                if (_audioProcessor != null)
                {
                    _audioProcessor.ProgressChanged -= OnProgressChanged;
                    _audioProcessor.FileProcessed -= OnFileProcessed;
                }
                
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Processes a single audio file
        /// </summary>
        public async Task ProcessFileAsync(AudioFileInfo file, string outputPath, CancellationToken cancellationToken)
        {
            if (file == null || string.IsNullOrWhiteSpace(outputPath))
                return;

            _isProcessing = true;
            
            try
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _audioProcessor = new AudioProcessor(_cancellationTokenSource.Token);
                
                _audioProcessor.ProgressChanged += OnProgressChanged;
                _audioProcessor.FileProcessed += OnFileProcessed;

                await _audioProcessor.ProcessAudioFileAsync(file, outputPath);
            }
            finally
            {
                if (_audioProcessor != null)
                {
                    _audioProcessor.ProgressChanged -= OnProgressChanged;
                    _audioProcessor.FileProcessed -= OnFileProcessed;
                }
                
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Cancels any running processing operations
        /// </summary>
        public void CancelProcessing()
        {
            _cancellationTokenSource?.Cancel();
        }

        #region Event Handlers

        private void OnProgressChanged(object? sender, AudioProcessingProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private void OnFileProcessed(object? sender, AudioFileProcessedEventArgs e)
        {
            FileProcessed?.Invoke(this, e);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            CancelProcessing();
            _cancellationTokenSource?.Dispose();
            
            if (_audioProcessor != null)
            {
                _audioProcessor.ProgressChanged -= OnProgressChanged;
                _audioProcessor.FileProcessed -= OnFileProcessed;
            }
        }

        #endregion
    }
}