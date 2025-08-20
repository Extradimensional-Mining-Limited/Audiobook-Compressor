/*
    Filename: IDiagnosticService.cs
    Last Updated: 2025-08-19 20:23 CEST
    Version: 1.2.L
    State: Experimental
    Signed: Meridian

    Synopsis:
    Professional diagnostic framework service interface per Focus 19.4.0 authorization.
    Provides comprehensive instrumentation capabilities with zero-trace production removal,
    automatic context capture, event correlation, and auditable documentation integration.
*/

using System.Runtime.CompilerServices;

namespace Audiobook_Compressor.Services.Diagnostics
{
    /// <summary>
    /// Professional diagnostic service for comprehensive application instrumentation
    /// with zero-trace production removal and automatic context capture
    /// </summary>
    public interface IDiagnosticService
    {
        /// <summary>
        /// Logs a diagnostic event with automatic context capture
        /// </summary>
        /// <param name="bugId">Bug identifier for tracking (e.g., "33")</param>
        /// <param name="message">Diagnostic message</param>
        /// <param name="context">Optional context object for structured data capture</param>
        /// <param name="memberName">Automatically captured calling member name</param>
        /// <param name="sourceFilePath">Automatically captured source file path</param>
        /// <param name="sourceLineNumber">Automatically captured source line number</param>
        void LogEvent(string bugId, string message, object? context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// Logs a comprehensive state capture with intelligent object serialization
        /// </summary>
        /// <param name="bugId">Bug identifier for tracking</param>
        /// <param name="description">Description of the state being captured</param>
        /// <param name="state">State object to serialize and capture</param>
        /// <param name="memberName">Automatically captured calling member name</param>
        /// <param name="sourceFilePath">Automatically captured source file path</param>
        /// <param name="sourceLineNumber">Automatically captured source line number</param>
        void LogStateCapture(string bugId, string description, object state,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// Logs service-to-service interactions for event propagation analysis
        /// </summary>
        /// <param name="bugId">Bug identifier for tracking</param>
        /// <param name="interaction">Description of the interaction</param>
        /// <param name="source">Source service or component</param>
        /// <param name="target">Target service or component</param>
        /// <param name="details">Optional interaction details</param>
        /// <param name="memberName">Automatically captured calling member name</param>
        /// <param name="sourceFilePath">Automatically captured source file path</param>
        /// <param name="sourceLineNumber">Automatically captured source line number</param>
        void LogInteraction(string bugId, string interaction,
            string source, string target, object? details = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// Begins event correlation for tracking related operations across service boundaries
        /// </summary>
        /// <param name="bugId">Bug identifier for tracking</param>
        /// <param name="operationName">Name of the operation being correlated</param>
        /// <returns>Correlation ID for use with EndCorrelation</returns>
        string BeginCorrelation(string bugId, string operationName);

        /// <summary>
        /// Ends event correlation with operation result
        /// </summary>
        /// <param name="correlationId">Correlation ID from BeginCorrelation</param>
        /// <param name="result">Operation result description</param>
        void EndCorrelation(string correlationId, string result = "Completed");

        /// <summary>
        /// Pushes diagnostic context for nested operation tracking
        /// </summary>
        /// <param name="bugId">Bug identifier for tracking</param>
        /// <param name="contextName">Name of the context being pushed</param>
        /// <param name="contextData">Context data for the operation</param>
        void PushContext(string bugId, string contextName, object contextData);

        /// <summary>
        /// Pops diagnostic context when nested operation completes
        /// </summary>
        /// <param name="bugId">Bug identifier for tracking</param>
        void PopContext(string bugId);

        /// <summary>
        /// Sets the output channel for diagnostic messages
        /// </summary>
        /// <param name="channel">Target output channel</param>
        void SetOutputChannel(DiagnosticOutputChannel channel);

        /// <summary>
        /// Flushes any buffered diagnostic output
        /// </summary>
        void FlushOutput();
    }

    /// <summary>
    /// Diagnostic output channel options
    /// </summary>
    public enum DiagnosticOutputChannel
    {
        /// <summary>
        /// Output to debug console only
        /// </summary>
        DebugConsole,

        /// <summary>
        /// Output to diagnostic file only
        /// </summary>
        DiagnosticFile,

        /// <summary>
        /// Output to both debug console and diagnostic file
        /// </summary>
        Both
    }

    /// <summary>
    /// Diagnostic entry for internal framework use
    /// </summary>
    public class DiagnosticEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string BugId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Context { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public string? CorrelationId { get; set; }
        public int ThreadId { get; set; } = Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// Diagnostic correlation context for tracking related operations
    /// </summary>
    public class DiagnosticCorrelation
    {
        public string Id { get; set; } = string.Empty;
        public string BugId { get; set; } = string.Empty;
        public string OperationName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}