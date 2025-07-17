/*
    Last Updated: 2025-07-17 00:06 CEST
    Version: 1.0.2
    State: Stable
    Signed: User
    
    Synopsis:
    Defines application-wide constants and configuration values.
    Updated file header structure and documented in Summary.md.
*/

using System.IO;

namespace Audiobook_Compressor
{
    internal static class Constants
    {
        // Tool paths relative to executable
        private const string ToolsFolder = "tools";
        private const string FFmpegExe = "ffmpeg.exe";
        private const string FFprobeExe = "ffprobe.exe";

        // Full paths
        public static string FFmpegPath => Path.Combine(AppDirectory, ToolsFolder, FFmpegExe);
        public static string FFprobePath => Path.Combine(AppDirectory, ToolsFolder, FFprobeExe);
        
        // Application directory
        public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Verifies that all required external tools are present
        /// </summary>
        /// <returns>True if all tools are present, false otherwise</returns>
        public static bool VerifyToolsExist()
        {
            try
            {
                return File.Exists(FFmpegPath) && File.Exists(FFprobePath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of missing tools
        /// </summary>
        /// <returns>List of missing tool paths</returns>
        public static IEnumerable<string> GetMissingTools()
        {
            if (!File.Exists(FFmpegPath))
                yield return FFmpegPath;
            if (!File.Exists(FFprobePath))
                yield return FFprobePath;
        }
    }
}