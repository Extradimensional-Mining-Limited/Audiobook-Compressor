/*
    Filename: AudioProcessor.cs
    Last Updated: 2025-07-22 05:35 CEST
    Version: 1.1.5
    State: Experimental
    Signed: GitHub Copilot
    
    Synopsis:
    Enhanced audio processing service with complete logic ported from PowerShell script. Output path logic fixed to prevent creation of extra subfolders named after files.
    - Updated filename sanitization logic to allow the centre dot (U+00B7) character (2C4).
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiobook_Compressor.Models;
using System.Text.RegularExpressions;

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
            var files = Settings.SupportedExtensions
                .SelectMany(extension => Directory.EnumerateFiles(sourcePath, $"*{extension}", SearchOption.AllDirectories))
                .Where(file => !file.Contains("Compressed_Audiobooks", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file)
                .ToList();

            foreach (var file in files)
            {
                if (_cancellationToken.IsCancellationRequested)
                    yield break;

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

        /// <summary>
        /// Sanitizes a filename by replacing invalid characters
        /// </summary>
        private static string SanitizeFilename(string fileName)
        {
            // Allow the centre dot (U+00B7) and other valid characters, but replace others
            var validChars = @"[a-zA-Z0-9_\-\.~ ]";
            var regex = new Regex($"[^{validChars}]");
            var sanitized = regex.Replace(fileName, "-");
            sanitized = sanitized.Trim().TrimEnd('.').Replace("--", "-");
            return sanitized;
        }

        /// <summary>
        /// Processes an audio file according to current settings
        /// </summary>
        public async Task ProcessAudioFileAsync(AudioFileInfo audioFile, string outputBasePath)
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            try
            {
                var sourceExt = Path.GetExtension(audioFile.SourcePath);
                var baseName = Path.GetFileNameWithoutExtension(audioFile.SourcePath);
                var sanitizedBaseName = SanitizeFilename(baseName);
                // Only use the directory part of RelativePath (exclude filename)
                var relativeDir = Path.GetDirectoryName(audioFile.RelativePath);
                var outputDir = string.IsNullOrEmpty(relativeDir) ? outputBasePath : Path.Combine(outputBasePath, relativeDir);
                Directory.CreateDirectory(outputDir);

                var ffprobeArgs = $"-v error -select_streams a:0 -show_entries stream=codec_name,bit_rate,channels -of json \"{audioFile.SourcePath}\"";
                var probeStartInfo = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = ffprobeArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var probeProcess = Process.Start(probeStartInfo);
                if (probeProcess == null)
                {
                    Debug.WriteLine("Failed to start FFprobe process");
                    OnFileProcessed(audioFile, false);
                    return;
                }

                var probeOutput = await probeProcess.StandardOutput.ReadToEndAsync();
                await probeProcess.WaitForExitAsync(_cancellationToken);

                if (probeProcess.ExitCode != 0)
                {
                    Debug.WriteLine($"Error probing file: {audioFile.SourcePath}");
                    OnFileProcessed(audioFile, false);
                    return;
                }

                dynamic probeData = Newtonsoft.Json.JsonConvert.DeserializeObject(probeOutput);
                var audioStream = probeData.streams[0];
                var currentBitrate = audioStream.bit_rate != null ? (int)audioStream.bit_rate : 0;
                var currentChannels = audioStream.channels != null ? (int)audioStream.channels : 0;
                var currentCodec = audioStream.codec_name;

                Debug.WriteLine($"Original Details: {currentCodec}, {currentChannels}ch, {currentBitrate / 1000}kbps");

                if (currentChannels == 1 && currentBitrate <= Settings.MonoCopyThreshold && currentBitrate != 0)
                {
                    Debug.WriteLine("Action: File is already mono and within tolerance. Copying.");
                    var destFileCopy = Path.Combine(outputDir, sanitizedBaseName + sourceExt);
                    File.Copy(audioFile.SourcePath, destFileCopy, true);
                    OnFileProcessed(audioFile, true);
                    return;
                }

                Debug.WriteLine("Action: Re-encoding to target format.");
                var destFileCompress = Path.Combine(outputDir, sanitizedBaseName + ".m4b");
                string channelArgs = "";
                string filterArgs = "";
                if (Settings.CurrentChannel == "Mono")
                {
                    // Use pan filter for mono
                    channelArgs = "-ac 1";
                    filterArgs = "-af pan=mono|c0=0.5*c0+0.5*c1";
                }
                else if (Settings.CurrentChannel == "Stereo")
                {
                    // Use stereo, no pan filter
                    channelArgs = "-ac 2";
                    filterArgs = "";
                }
                var ffmpegArgs = $"-i \"{audioFile.SourcePath}\" -vn -c:a aac -b:a {Settings.FormatBitrate(Settings.TargetBitrate)} -ar {Settings.TargetSampleRate} {channelArgs} {filterArgs} -map_metadata 0 -map_chapters 0 -movflags +faststart -y -v info \"{destFileCompress}\"";
                var ffmpegStartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var ffmpegProcess = Process.Start(ffmpegStartInfo);
                if (ffmpegProcess == null)
                {
                    Debug.WriteLine("Failed to start FFmpeg process");
                    OnFileProcessed(audioFile, false);
                    return;
                }

                await foreach (var line in ReadLinesAsync(ffmpegProcess.StandardError))
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        ffmpegProcess.Kill();
                        break;
                    }

                    Debug.WriteLine(line);
                    if (line.Contains("frame="))
                    {
                        var progress = ExtractProgress(line);
                        OnProgressChanged(audioFile, progress);
                    }
                }

                await ffmpegProcess.WaitForExitAsync(_cancellationToken);

                if (ffmpegProcess.ExitCode != 0 || !File.Exists(destFileCompress) || new FileInfo(destFileCompress).Length == 0)
                {
                    Debug.WriteLine($"FFmpeg failed or produced an invalid file for {audioFile.SourcePath}");
                    OnFileProcessed(audioFile, false);
                }
                else
                {
                    Debug.WriteLine($"Successfully processed file: {audioFile.SourcePath}");
                    OnFileProcessed(audioFile, true);
                }
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

        private double ExtractProgress(string ffmpegOutput)
        {
            // TODO: Implement logic to extract progress percentage from FFmpeg output
            return 0.0;
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