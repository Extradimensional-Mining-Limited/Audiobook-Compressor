/*
    Filename: AudioFileInfo.cs
    Last Updated: 2025-07-17 07:15 CEST
    Version: 1.1.0
    State: Stable
    Signed: User
    
    Synopsis:
    Audio file model with complete metadata support.
*/

using System.IO;

namespace Audiobook_Compressor.Models
{
    /// <summary>
    /// Represents an audio file and its metadata
    /// </summary>
    public class AudioFileInfo
    {
        /// <summary>
        /// Full path to the source audio file
        /// </summary>
        public required string SourcePath { get; init; }

        /// <summary>
        /// Path relative to the source directory
        /// </summary>
        public required string RelativePath { get; init; }

        /// <summary>
        /// Audio codec name (e.g., "mp3", "aac")
        /// </summary>
        public required string Codec { get; init; }

        /// <summary>
        /// Current bitrate in bits per second
        /// </summary>
        public int? Bitrate { get; set; }

        /// <summary>
        /// Gets the output path for this file
        /// </summary>
        public string GetOutputPath(string outputDirectory)
        {
            return Path.Combine(outputDirectory, RelativePath);
        }
    }
}