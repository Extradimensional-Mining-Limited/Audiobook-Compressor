/*
    Last Updated: 2025-07-17 07:15 CEST
    Version: 1.1.0
    State: Stable
    Signed: User
    
    Synopsis:
    Complete audio processing service with async operation support.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Audiobook_Compressor.Models;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Handles audio file processing operations
    /// </summary>
    public class AudioProcessor
    {
        /// <summary>
        /// Event raised when processing progress changes
        /// </summary>
        public event EventHandler<AudioProcessingProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Event raised when a file's processing is complete
        /// </summary>
        public event EventHandler<AudioFileProcessedEventArgs>? FileProcessed;

        private readonly string _ffmpegPath;
        private readonly string _ffprobePath;
        private readonly CancellationToken _cancellationToken;

        public AudioProcessor(CancellationToken cancellationToken = default)
        {
            _ffmpegPath = Constants.FFmpegPath;
            _ffprobePath = Constants.FFprobePath;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Probes an audio file to get its metadata
        /// </summary>
        private async Task<bool> ProbeAudioFileAsync(AudioFileInfo audioFile)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = $"-v error -select_streams a:0 -show_entries stream=codec_name,bit_rate,channels,sample_rate -of json \"{audioFile.SourcePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Debug.WriteLine("Failed to start FFprobe process");
                    return false;
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync(_cancellationToken);

                if (process.ExitCode != 0)
                    return false;

                // Parse JSON output from FFprobe
                // TODO: Implement JSON parsing to populate audioFile properties

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error probing file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Scans a directory for supported audio files
        /// </summary>
        public async IAsyncEnumerable<AudioFileInfo> ScanDirectoryAsync(string sourcePath)
        {
            foreach (var extension in Settings.SupportedExtensions)
            {
                foreach (var file in Directory.EnumerateFiles(sourcePath, $"*{extension}", SearchOption.AllDirectories))
                {
                    if (_cancellationToken.IsCancellationRequested)
                        yield break;

                    if (file.Contains("Compressed_Audiobooks", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var audioFile = new AudioFileInfo
                    {
                        SourcePath = file,
                        RelativePath = Path.GetRelativePath(sourcePath, file),
                        Codec = "unknown" // Default value until probed
                    };

                    if (await ProbeAudioFileAsync(audioFile))
                        yield return audioFile;
                }
            }
        }

        /// <summary>
        /// Processes an audio file according to current settings
        /// </summary>
        public async Task ProcessAudioFileAsync(AudioFileInfo audioFile, string outputPath)
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            try
            {
                // Create output directory if it doesn't exist
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Check if file is already optimized (mono and below threshold)
                var isOptimized = Settings.CurrentChannel == "Mono" && 
                                audioFile.Bitrate.HasValue && 
                                audioFile.Bitrate.Value <= Settings.MonoCopyThreshold;

                if (isOptimized)
                {
                    // File is already optimized, just copy it
                    File.Copy(audioFile.SourcePath, outputPath, true);
                    OnFileProcessed(audioFile, true);
                    return;
                }

                var args = new List<string>
                {
                    "-i", $"\"{audioFile.SourcePath}\"",
                    "-vn",                                    // No video
                    "-c:a", "aac",                           // AAC codec
                    "-b:a", Settings.FormatBitrate(Settings.TargetBitrate),
                    "-ar", Settings.TargetSampleRate.ToString(),
                    "-af", "pan=mono|c0=0.5*c0+0.5*c1",      // Downmix to mono
                    "-map_metadata", "0",                     // Copy metadata
                    "-map_chapters", "0",                     // Copy chapters
                    "-movflags", "+faststart",               // Enable fast start
                    "-y",                                    // Overwrite output
                    "-v", "info",                            // Verbose output
                    $"\"{outputPath}\""
                };

                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = string.Join(" ", args),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    OnFileProcessed(audioFile, false);
                    return;
                }
                
                // Monitor progress
                await foreach (var line in ReadLinesAsync(process.StandardError))
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        process.Kill();
                        break;
                    }

                    // TODO: Parse FFmpeg progress output and raise ProgressChanged event
                }

                await process.WaitForExitAsync(_cancellationToken);
                OnFileProcessed(audioFile, process.ExitCode == 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing file: {ex.Message}");
                OnFileProcessed(audioFile, false);
            }
        }

        private static async IAsyncEnumerable<string> ReadLinesAsync(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                {
                    yield return line;
                }
            }
        }

        private void OnProgressChanged(AudioFileInfo file, double progress)
        {
            ProgressChanged?.Invoke(this, new AudioProcessingProgressEventArgs(file, progress));
        }

        private void OnFileProcessed(AudioFileInfo file, bool success)
        {
            FileProcessed?.Invoke(this, new AudioFileProcessedEventArgs(file, success));
        }
    }

    public class AudioProcessingProgressEventArgs : EventArgs
    {
        public AudioFileInfo File { get; }
        public double Progress { get; }

        public AudioProcessingProgressEventArgs(AudioFileInfo file, double progress)
        {
            File = file;
            Progress = progress;
        }
    }

    public class AudioFileProcessedEventArgs : EventArgs
    {
        public AudioFileInfo File { get; }
        public bool Success { get; }

        public AudioFileProcessedEventArgs(AudioFileInfo file, bool success)
        {
            File = file;
            Success = success;
        }
    }
}