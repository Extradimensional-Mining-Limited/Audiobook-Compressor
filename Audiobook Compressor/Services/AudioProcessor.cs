/*
    Filename: AudioProcessor.cs
    Last Updated: 2025-08-07 12:43 CEST
    Version: 1.2.F
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Core audio processing service with dependency injection support for comprehensive unit testing and Focus 5.0.4 Advanced panel "Defer to Rockit" logic implementation.
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
using Audiobook_Compressor.Services;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Handles audio file processing operations with contextual settings support
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
        private readonly IProcessRunner _processRunner;
        private readonly IFileSystem _fileSystem;

        public AudioProcessor(CancellationToken cancellationToken = default, IProcessRunner? processRunner = null, IFileSystem? fileSystem = null)
        {
            _ffmpegPath = Constants.FFmpegPath;
            _ffprobePath = Constants.FFprobePath;
            _cancellationToken = cancellationToken;
            _processRunner = processRunner ?? new DefaultProcessRunner();
            _fileSystem = fileSystem ?? new DefaultFileSystem();
        }

        /// <summary>
        /// Probes an audio file to get its metadata and populate AudioFileInfo
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

                using var process = _processRunner.Start(startInfo);
                if (process == null)
                {
                    Debug.WriteLine("Failed to start FFprobe process");
                    return false;
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync(_cancellationToken);

                if (process.ExitCode != 0)
                    return false;

                // Parse JSON output from FFprobe to populate audioFile properties
                try
                {
                    dynamic probeData = Newtonsoft.Json.JsonConvert.DeserializeObject(output);
                    var audioStream = probeData.streams[0];
                    audioFile.Bitrate = audioStream.bit_rate != null ? (int)audioStream.bit_rate : null;
                    // Store additional metadata if needed for processing decisions
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing FFprobe output: {ex.Message}");
                    return false;
                }

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
        /// Processes an audio file according to contextual settings (Focus 5.0.0 implementation)
        /// </summary>
        public async Task ProcessAudioFileAsync(AudioFileInfo audioFile, string outputBasePath)
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            try
            {
                // Get processing context from current UI settings
                var context = ProcessingContext.FromCurrentSettings();
                
                // Get detailed file information
                var fileInfo = await GetDetailedFileInfoAsync(audioFile);
                if (fileInfo == null)
                {
                    OnFileProcessed(audioFile, false);
                    return;
                }

                Debug.WriteLine($"Processing: {Path.GetFileName(audioFile.SourcePath)} - {fileInfo.Codec}, {fileInfo.Channels}ch, {fileInfo.Bitrate / 1000}kbps");
                Debug.WriteLine($"Context: {context.CurrentMode} mode, {context.SelectedAction} action");

                // Implement Focus 5.0.0 decision tree
                if (context.IsChannelModeMatch(fileInfo.Channels))
                {
                    // Simple path: file matches mode (mono file in mono mode, stereo file in stereo mode)
                    await ProcessAsNormal(audioFile, fileInfo, context, outputBasePath);
                }
                else
                {
                    // Complex path: file doesn't match mode (stereo file in mono mode, mono file in stereo mode)
                    await ProcessAsException(audioFile, fileInfo, context, outputBasePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing file: {ex.Message}");
                OnFileProcessed(audioFile, false);
            }
        }

        /// <summary>
        /// Gets detailed file information including channels, bitrate, and codec
        /// </summary>
        private async Task<DetailedFileInfo?> GetDetailedFileInfoAsync(AudioFileInfo audioFile)
        {
            try
            {
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

                using var probeProcess = _processRunner.Start(probeStartInfo);
                if (probeProcess == null)
                {
                    Debug.WriteLine("Failed to start FFprobe process");
                    return null;
                }

                var probeOutput = await probeProcess.StandardOutput.ReadToEndAsync();
                await probeProcess.WaitForExitAsync(_cancellationToken);

                if (probeProcess.ExitCode != 0)
                {
                    Debug.WriteLine($"Error probing file: {audioFile.SourcePath}");
                    return null;
                }

                dynamic probeData = Newtonsoft.Json.JsonConvert.DeserializeObject(probeOutput);
                var audioStream = probeData.streams[0];
                
                return new DetailedFileInfo
                {
                    Bitrate = audioStream.bit_rate != null ? (int)audioStream.bit_rate : 0,
                    Channels = audioStream.channels != null ? (int)audioStream.channels : 0,
                    Codec = audioStream.codec_name
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting file details: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Processes files that match the current mode (normal processing path)
        /// </summary>
        private async Task ProcessAsNormal(AudioFileInfo audioFile, DetailedFileInfo fileInfo, ProcessingContext context, string outputBasePath)
        {
            var settings = context.GetActiveSettings();
            
            // Apply threshold check for copy vs convert decision
            if (ShouldCopyFile(fileInfo, settings))
            {
                Debug.WriteLine("Action: File within threshold. Copying.");
                await CopyFile(audioFile, outputBasePath);
            }
            else
            {
                Debug.WriteLine("Action: File needs compression. Converting.");
                await BuildAndRunFFmpeg(audioFile, settings, outputBasePath);
            }
        }

        /// <summary>
        /// Processes files that don't match the current mode (exception processing path)
        /// </summary>
        private async Task ProcessAsException(AudioFileInfo audioFile, DetailedFileInfo fileInfo, ProcessingContext context, string outputBasePath)
        {
            if (context.CurrentMode == "Mono") // Mono mode handling stereo file
            {
                switch (context.SelectedAction)
                {
                    case "Copy":
                        Debug.WriteLine("Action: Mono mode - copying stereo file.");
                        await CopyFile(audioFile, outputBasePath);
                        break;
                        
                    case "Advanced":
                        Debug.WriteLine("Action: Mono mode - using advanced settings for stereo file.");
                        await ProcessWithAdvancedLogic(audioFile, fileInfo, context.AdvancedSettings, outputBasePath);
                        break;
                        
                    case "Convert":
                    default:
                        Debug.WriteLine("Action: Mono mode - converting stereo to mono.");
                        await BuildAndRunFFmpeg(audioFile, context.MainSettings, outputBasePath);
                        break;
                }
            }
            else // Stereo mode handling mono file
            {
                switch (context.SelectedAction)
                {
                    case "Copy":
                        Debug.WriteLine("Action: Stereo mode - copying mono file.");
                        await CopyFile(audioFile, outputBasePath);
                        break;
                        
                    case "Advanced":
                        Debug.WriteLine("Action: Stereo mode - using advanced settings for mono file.");
                        await ProcessWithAdvancedLogic(audioFile, fileInfo, context.AdvancedSettings, outputBasePath);
                        break;
                        
                    case "Convert":
                        Debug.WriteLine("Action: Stereo mode - converting mono to stereo with upmix logic.");
                        await HandleUpmixLogic(audioFile, fileInfo, context, outputBasePath);
                        break;
                        
                    default:
                        Debug.WriteLine("Action: Stereo mode - default copying mono file.");
                        await CopyFile(audioFile, outputBasePath);
                        break;
                }
            }
        }

        /// <summary>
        /// Processes files using Advanced panel logic with sub-threshold behavior
        /// </summary>
        private async Task ProcessWithAdvancedLogic(AudioFileInfo audioFile, DetailedFileInfo fileInfo, CompressionSettings settings, string outputBasePath)
        {
            Debug.WriteLine($"Processing with Advanced logic: threshold={settings.ConversionThreshold}, file bitrate={fileInfo.Bitrate}");
            
            // Parse threshold for comparison
            if (!Settings.TryParseBitrate(settings.ConversionThreshold, out int threshold))
            {
                threshold = Settings.DefaultConversionThreshold;
            }

            // Check if file is at or above threshold
            if (fileInfo.Bitrate >= threshold)
            {
                Debug.WriteLine("File at or above threshold - using main advanced settings.");
                await BuildAndRunFFmpeg(audioFile, settings, outputBasePath);
            }
            else
            {
                Debug.WriteLine($"File below threshold - applying sub-threshold action: {settings.SubThresholdAction}");
                
                switch (settings.SubThresholdAction)
                {
                    case "Copy":
                        Debug.WriteLine("Sub-threshold action: Copy");
                        await CopyFile(audioFile, outputBasePath);
                        break;
                        
                    case "DeferToRockit":
                        Debug.WriteLine("Sub-threshold action: Defer to Rockit");
                        await ProcessWithRockitQualityLogic(audioFile, fileInfo, settings, outputBasePath);
                        break;
                        
                    case "ConvertTo":
                        Debug.WriteLine($"Sub-threshold action: Convert to {settings.CustomTargetBitrate}");
                        await ProcessWithCustomBitrate(audioFile, fileInfo, settings, outputBasePath);
                        break;
                        
                    default:
                        Debug.WriteLine("Unknown sub-threshold action - defaulting to copy");
                        await CopyFile(audioFile, outputBasePath);
                        break;
                }
            }
        }

        /// <summary>
        /// Implements the "Defer to Rockit" quality-preserving logic from Focus 5.0.3
        /// </summary>
        private async Task ProcessWithRockitQualityLogic(AudioFileInfo audioFile, DetailedFileInfo fileInfo, CompressionSettings settings, string outputBasePath)
        {
            Debug.WriteLine($"Rockit Quality Logic: file channels={fileInfo.Channels}, target channels={settings.ChannelMode}");
            
            // Determine channel transformation scenario
            var targetChannels = settings.ChannelMode == "Mono" ? 1 : 2;
            
            if (fileInfo.Channels == targetChannels)
            {
                // Same channels - perform simple file copy (zero generational loss)
                Debug.WriteLine("Rockit Logic: Same channels detected - performing file copy for zero generational loss.");
                await CopyFile(audioFile, outputBasePath);
            }
            else if (fileInfo.Channels > targetChannels)
            {
                // Channel reduction (Stereo ? Mono) - VBR downmix with maxrate capped at original bitrate
                Debug.WriteLine($"Rockit Logic: Channel reduction ({fileInfo.Channels} ? {targetChannels}) - VBR downmix with maxrate={fileInfo.Bitrate}bps");
                await BuildAndRunFFmpegWithVBR(audioFile, settings, outputBasePath, fileInfo.Bitrate, false);
            }
            else
            {
                // Channel expansion (Mono ? Stereo) - VBR upmix with maxrate capped at estimated × 1.10
                var estimatedStereoBitrate = fileInfo.Bitrate * 2;
                var maxRate = (int)(estimatedStereoBitrate * 1.10);
                Debug.WriteLine($"Rockit Logic: Channel expansion ({fileInfo.Channels} ? {targetChannels}) - VBR upmix with maxrate={maxRate}bps");
                await BuildAndRunFFmpegWithVBR(audioFile, settings, outputBasePath, maxRate, true);
            }
        }

        /// <summary>
        /// Processes files with custom target bitrate for "Convert to:" sub-threshold action
        /// </summary>
        private async Task ProcessWithCustomBitrate(AudioFileInfo audioFile, DetailedFileInfo fileInfo, CompressionSettings settings, string outputBasePath)
        {
            // Create a temporary settings object with the custom bitrate
            var customSettings = new CompressionSettings
            {
                ChannelMode = settings.ChannelMode,
                TargetBitrate = settings.CustomTargetBitrate,
                SampleRate = settings.SampleRate,
                ConversionThreshold = settings.ConversionThreshold,
                EncodingType = settings.EncodingType,
                PassMode = settings.PassMode
            };
            
            Debug.WriteLine($"Processing with custom bitrate: {customSettings.TargetBitrate}");
            await BuildAndRunFFmpeg(audioFile, customSettings, outputBasePath);
        }

        /// <summary>
        /// Builds and runs FFmpeg with VBR quality settings and dynamic maxrate caps
        /// </summary>
        private async Task BuildAndRunFFmpegWithVBR(AudioFileInfo audioFile, CompressionSettings settings, string outputBasePath, int maxRateBps, bool forceUpmix)
        {
            try
            {
                var command = BuildFFmpegVBRCommand(audioFile, settings, outputBasePath, maxRateBps, forceUpmix);
                await ExecuteFFmpegCommand(audioFile, command);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in BuildAndRunFFmpegWithVBR: {ex.Message}");
                OnFileProcessed(audioFile, false);
            }
        }

        /// <summary>
        /// Builds FFmpeg command with VBR quality settings and maxrate caps
        /// </summary>
        private string BuildFFmpegVBRCommand(AudioFileInfo audioFile, CompressionSettings settings, string outputBasePath, int maxRateBps, bool forceUpmix)
        {
            var baseName = Path.GetFileNameWithoutExtension(audioFile.SourcePath);
            var sanitizedBaseName = SanitizeFilename(baseName);
            var relativeDir = Path.GetDirectoryName(audioFile.RelativePath);
            var outputDir = string.IsNullOrEmpty(relativeDir) ? outputBasePath : Path.Combine(outputBasePath, relativeDir);
            _fileSystem.CreateDirectory(outputDir);
            
            var destFile = Path.Combine(outputDir, sanitizedBaseName + ".m4b");

            // Build channel and filter arguments
            string channelArgs = "";
            string filterArgs = "";
            
            if (forceUpmix || settings.ChannelMode == "Stereo")
            {
                channelArgs = "-ac 2";
                if (forceUpmix)
                {
                    // Upmix mono to stereo
                    filterArgs = "-af \"pan=stereo|c0=c0|c1=c0\"";
                }
            }
            else if (settings.ChannelMode == "Mono")
            {
                channelArgs = "-ac 1";
                filterArgs = "-af \"pan=mono|c0=0.5*c0+0.5*c1\"";
            }

            // Build VBR quality arguments with maxrate cap
            var maxRateFormatted = Settings.FormatBitrate(maxRateBps);
            var bufSize = maxRateBps * 2; // Buffer size = 2x maxrate for VBR stability
            var bufSizeFormatted = Settings.FormatBitrate(bufSize);
            
            string bitrateArgs = $"-q:a 2 -maxrate {maxRateFormatted} -bufsize {bufSizeFormatted}";

            // Build sample rate arguments
            string sampleRateArgs = "";
            if (Settings.TryParseSampleRate(settings.SampleRate, out int sampleRate))
            {
                sampleRateArgs = $"-ar {sampleRate}";
            }

            return $"-i \"{audioFile.SourcePath}\" -vn -c:a aac {bitrateArgs} {sampleRateArgs} {channelArgs} {filterArgs} -map_metadata 0 -map_chapters 0 -movflags +faststart -y -v info \"{destFile}\"";
        }

        /// <summary>
        /// Executes FFmpeg command and monitors progress
        /// </summary>
        private async Task ExecuteFFmpegCommand(AudioFileInfo audioFile, string ffmpegArgs)
        {
            var ffmpegStartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var ffmpegProcess = _processRunner.Start(ffmpegStartInfo);
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

            // Verify output file was created successfully
            var outputFile = ExtractOutputFileFromCommand(ffmpegArgs);
            if (ffmpegProcess.ExitCode != 0 || !_fileSystem.Exists(outputFile) || _fileSystem.GetFileLength(outputFile) == 0)
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

        /// <summary>
        /// Extracts output filename from FFmpeg command arguments
        /// </summary>
        private static string ExtractOutputFileFromCommand(string ffmpegArgs)
        {
            var match = System.Text.RegularExpressions.Regex.Match(ffmpegArgs, @"""([^""]+\.m4b)""$");
            return match.Success ? match.Groups[1].Value : "";
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
            return 0.1; // Return small progress value to indicate activity
        }

        /// <summary>
        /// Determines if a file should be copied based on threshold settings
        /// </summary>
        private static bool ShouldCopyFile(DetailedFileInfo fileInfo, CompressionSettings settings)
        {
            if (!Settings.TryParseBitrate(settings.ConversionThreshold, out int threshold))
                return false;

            return fileInfo.Bitrate > 0 && fileInfo.Bitrate <= threshold;
        }

        /// <summary>
        /// Copies a file to the output directory maintaining directory structure
        /// </summary>
        private async Task CopyFile(AudioFileInfo audioFile, string outputBasePath)
        {
            try
            {
                var sourceExt = Path.GetExtension(audioFile.SourcePath);
                var baseName = Path.GetFileNameWithoutExtension(audioFile.SourcePath);
                var sanitizedBaseName = SanitizeFilename(baseName);
                var relativeDir = Path.GetDirectoryName(audioFile.RelativePath);
                var outputDir = string.IsNullOrEmpty(relativeDir) ? outputBasePath : Path.Combine(outputBasePath, relativeDir);
                
                _fileSystem.CreateDirectory(outputDir);
                var destFile = Path.Combine(outputDir, sanitizedBaseName + sourceExt);
                
                await Task.Run(() => _fileSystem.Copy(audioFile.SourcePath, destFile, true), _cancellationToken);
                OnFileProcessed(audioFile, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error copying file: {ex.Message}");
                OnFileProcessed(audioFile, false);
            }
        }

        /// <summary>
        /// Builds and runs FFmpeg command with the specified settings
        /// </summary>
        private async Task BuildAndRunFFmpeg(AudioFileInfo audioFile, CompressionSettings settings, string outputBasePath)
        {
            try
            {
                var command = BuildFFmpegCommand(audioFile, settings, outputBasePath, false);
                await ExecuteFFmpegCommand(audioFile, command);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in BuildAndRunFFmpeg: {ex.Message}");
                OnFileProcessed(audioFile, false);
            }
        }

        /// <summary>
        /// Builds and runs FFmpeg command with upmix logic for mono-to-stereo conversion
        /// </summary>
        private async Task BuildAndRunFFmpegWithUpmix(AudioFileInfo audioFile, CompressionSettings settings, string outputBasePath, bool applyTargetBitrate)
        {
            try
            {
                var command = BuildFFmpegCommand(audioFile, settings, outputBasePath, !applyTargetBitrate, true);
                await ExecuteFFmpegCommand(audioFile, command);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in BuildAndRunFFmpegWithUpmix: {ex.Message}");
                OnFileProcessed(audioFile, false);
            }
        }

        /// <summary>
        /// Handles the special mono-to-stereo conversion logic from Focus 5.0.0
        /// </summary>
        private async Task HandleUpmixLogic(AudioFileInfo audioFile, DetailedFileInfo fileInfo, ProcessingContext context, string outputBasePath)
        {
            // 1. Estimate the upmixed bitrate
            long estimatedStereoBitrate = fileInfo.Bitrate * 2;

            // 2. Get the user's stereo conversion threshold from main settings
            if (!Settings.TryParseBitrate(context.MainSettings.ConversionThreshold, out int stereoConversionThreshold))
            {
                stereoConversionThreshold = Settings.DefaultConversionThreshold;
            }

            Debug.WriteLine($"Upmix Logic: Current bitrate={fileInfo.Bitrate}, Estimated stereo={estimatedStereoBitrate}, Threshold={stereoConversionThreshold}");

            // 3. Compare and decide
            if (estimatedStereoBitrate >= stereoConversionThreshold)
            {
                // File needs full compression - upmix to stereo AND apply target bitrate
                Debug.WriteLine("Action: Estimated bitrate above threshold - full compression with upmix.");
                await BuildAndRunFFmpegWithUpmix(audioFile, context.MainSettings, outputBasePath, applyTargetBitrate: true);
            }
            else
            {
                // File is "good enough" - upmix to stereo but preserve quality
                Debug.WriteLine("Action: Estimated bitrate below threshold - quality preservation with upmix.");
                await BuildAndRunFFmpegWithUpmix(audioFile, context.MainSettings, outputBasePath, applyTargetBitrate: false);
            }
        }

        /// <summary>
        /// Builds FFmpeg command string based on settings and options
        /// </summary>
        private string BuildFFmpegCommand(AudioFileInfo audioFile, CompressionSettings settings, string outputBasePath, bool useHighQuality = false, bool forceUpmix = false)
        {
            var baseName = Path.GetFileNameWithoutExtension(audioFile.SourcePath);
            var sanitizedBaseName = SanitizeFilename(baseName);
            var relativeDir = Path.GetDirectoryName(audioFile.RelativePath);
            var outputDir = string.IsNullOrEmpty(relativeDir) ? outputBasePath : Path.Combine(outputBasePath, relativeDir);
            _fileSystem.CreateDirectory(outputDir);
            
            var destFile = Path.Combine(outputDir, sanitizedBaseName + ".m4b");

            // Build channel and filter arguments
            string channelArgs = "";
            string filterArgs = "";
            
            if (forceUpmix || settings.ChannelMode == "Stereo")
            {
                channelArgs = "-ac 2";
                if (forceUpmix)
                {
                    // Upmix mono to stereo
                    filterArgs = "-af \"pan=stereo|c0=c0|c1=c0\"";
                }
            }
            else if (settings.ChannelMode == "Mono")
            {
                channelArgs = "-ac 1";
                filterArgs = "-af \"pan=mono|c0=0.5*c0+0.5*c1\"";
            }

            // Build bitrate arguments
            string bitrateArgs;
            if (useHighQuality)
            {
                // Use high quality VBR instead of target bitrate (Focus 5.0.0 "high_quality_placeholder")
                bitrateArgs = "-q:a 2";
            }
            else
            {
                bitrateArgs = $"-b:a {settings.TargetBitrate}";
            }

            // Build sample rate arguments
            string sampleRateArgs = "";
            if (Settings.TryParseSampleRate(settings.SampleRate, out int sampleRate))
            {
                sampleRateArgs = $"-ar {sampleRate}";
            }

            return $"-i \"{audioFile.SourcePath}\" -vn -c:a aac {bitrateArgs} {sampleRateArgs} {channelArgs} {filterArgs} -map_metadata 0 -map_chapters 0 -movflags +faststart -y -v info \"{destFile}\"";
        }
    }

    /// <summary>
    /// Detailed file information for processing decisions
    /// </summary>
    public class DetailedFileInfo
    {
        public int Bitrate { get; init; }
        public int Channels { get; init; }
        public string Codec { get; init; } = string.Empty;
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