/*
    Filename: IProcessRunner.cs
    Last Updated: 2025-08-07 12:43 CEST
    Version: 1.2.E
    State: Experimental
    Signed: Vanguard

    Synopsis:
    Abstraction interface for external process execution enabling dependency injection and comprehensive unit testing of AudioProcessor logic.
*/

using System.Diagnostics;

namespace Audiobook_Compressor.Services
{
    /// <summary>
    /// Abstraction for running external processes (e.g., FFmpeg, FFprobe)
    /// </summary>
    public interface IProcessRunner
    {
        Process? Start(ProcessStartInfo startInfo);
    }

    /// <summary>
    /// Default implementation using System.Diagnostics.Process
    /// </summary>
    public class DefaultProcessRunner : IProcessRunner
    {
        public Process? Start(ProcessStartInfo startInfo)
        {
            return Process.Start(startInfo);
        }
    }
}
