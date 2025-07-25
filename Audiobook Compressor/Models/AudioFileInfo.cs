/*
    Filename: AudioFileInfo.cs
    Last Updated: 2025-07-25 03:32
    Version: 1.2.0
    State: Stable
    Signed: User

    Synopsis:
    - All file headers updated to v1.2.0, state Stable, signed User, with unified timestamp.
    - Documentation and changelog discipline enforced for release.
    - No code changes since last version except header and documentation updates.
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