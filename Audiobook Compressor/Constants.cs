/*
    Filename: Constants.cs
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