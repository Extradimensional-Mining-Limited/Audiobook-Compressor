/*
    Last Updated: 2025-07-17 03:10 CEST
    Version: 1.0.3
    State: Stable
    Signed: User
    
    Synopsis:
    Defines application-wide constants and configuration values.
    Fixed critical issue where v1.0.2 was incorrectly looking for tools in build output directory.
    Now correctly resolves tools path during development.
*/

using System.IO;
using System.Reflection;

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
        
        // Application directory - looks in project folder during development
        public static string AppDirectory
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyDirectory = Path.GetDirectoryName(assembly.Location) ?? 
                    throw new InvalidOperationException("Unable to determine assembly directory");
                
                // If we're running from the build output directory (bin\Debug\net8.0)
                if (assemblyDirectory.Contains("bin" + Path.DirectorySeparatorChar + "Debug"))
                {
                    // Go up three levels to the project directory
                    return Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", ".."));
                }
                
                return assemblyDirectory;
            }
        }

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