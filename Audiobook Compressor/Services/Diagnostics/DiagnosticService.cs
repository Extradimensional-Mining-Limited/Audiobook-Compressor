/*
    Filename: DiagnosticService.cs
    Last Updated: 2025-08-19 20:23 CEST
    Version: 1.2.L
    State: Experimental
    Signed: Meridian

    Synopsis:
    Professional diagnostic framework implementation per Focus 19.4.0 authorization.
    Provides zero-trace production removal through conditional compilation, automatic context capture,
    event correlation, and multi-channel output management for comprehensive application instrumentation.
*/

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Audiobook_Compressor.Services.Diagnostics
{
    /// <summary>
    /// Professional diagnostic service implementation with conditional compilation for zero-trace production builds
    /// </summary>
    public class DiagnosticService : IDiagnosticService
    {
        private readonly ConcurrentDictionary<string, Stack<object>> _contextStacks = new();
        private readonly ConcurrentDictionary<string, DiagnosticCorrelation> _correlations = new();
        private readonly object _lockObject = new();
        private DiagnosticOutputChannel _outputChannel = DiagnosticOutputChannel.Both;

        /// <summary>
        /// Logs a diagnostic event with automatic context capture
        /// </summary>
        public void LogEvent(string bugId, string message, object? context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if DEBUG
            var entry = CreateDiagnosticEntry(bugId, "EVENT", message,
                memberName, sourceFilePath, sourceLineNumber);

            if (context != null)
            {
                entry.Context = SerializeContext(context);
            }

            OutputDiagnosticEntry(entry);
#endif
        }

        /// <summary>
        /// Logs a comprehensive state capture with intelligent object serialization
        /// </summary>
        public void LogStateCapture(string bugId, string description, object state,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if DEBUG
            var entry = CreateDiagnosticEntry(bugId, "STATE", description,
                memberName, sourceFilePath, sourceLineNumber);

            entry.Context = SerializeContext(state);

            OutputDiagnosticEntry(entry);
#endif
        }

        /// <summary>
        /// Logs service-to-service interactions for event propagation analysis
        /// </summary>
        public void LogInteraction(string bugId, string interaction,
            string source, string target, object? details = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if DEBUG
            var message = $"{interaction}: {source} ? {target}";
            var entry = CreateDiagnosticEntry(bugId, "INTERACTION", message,
                memberName, sourceFilePath, sourceLineNumber);

            if (details != null)
            {
                entry.Context = SerializeContext(details);
            }

            OutputDiagnosticEntry(entry);
#endif
        }

        /// <summary>
        /// Begins event correlation for tracking related operations across service boundaries
        /// </summary>
        public string BeginCorrelation(string bugId, string operationName)
        {
#if DEBUG
            var correlationId = Guid.NewGuid().ToString("N")[..8]; // Short correlation ID
            var correlation = new DiagnosticCorrelation
            {
                Id = correlationId,
                BugId = bugId,
                OperationName = operationName,
                StartTime = DateTime.Now
            };

            _correlations.TryAdd(correlationId, correlation);

            var entry = CreateDiagnosticEntry(bugId, "CORRELATION_START", 
                $"Operation: {operationName}", "", "", 0);
            entry.CorrelationId = correlationId;

            OutputDiagnosticEntry(entry);

            return correlationId;
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Ends event correlation with operation result
        /// </summary>
        public void EndCorrelation(string correlationId, string result = "Completed")
        {
#if DEBUG
            if (_correlations.TryGetValue(correlationId, out var correlation))
            {
                correlation.EndTime = DateTime.Now;
                correlation.Result = result;

                var duration = correlation.EndTime.Value - correlation.StartTime;
                var message = $"Operation: {correlation.OperationName}, Result: {result}, Duration: {duration.TotalMilliseconds:F2}ms";

                var entry = CreateDiagnosticEntry(correlation.BugId, "CORRELATION_END", message, "", "", 0);
                entry.CorrelationId = correlationId;

                OutputDiagnosticEntry(entry);

                _correlations.TryRemove(correlationId, out _);
            }
#endif
        }

        /// <summary>
        /// Pushes diagnostic context for nested operation tracking
        /// </summary>
        public void PushContext(string bugId, string contextName, object contextData)
        {
#if DEBUG
            lock (_lockObject)
            {
                if (!_contextStacks.TryGetValue(bugId, out var stack))
                {
                    stack = new Stack<object>();
                    _contextStacks[bugId] = stack;
                }

                var contextEntry = new { Name = contextName, Data = contextData, Timestamp = DateTime.Now };
                stack.Push(contextEntry);

                var entry = CreateDiagnosticEntry(bugId, "CONTEXT_PUSH", 
                    $"Context: {contextName}", "", "", 0);
                entry.Context = SerializeContext(contextData);

                OutputDiagnosticEntry(entry);
            }
#endif
        }

        /// <summary>
        /// Pops diagnostic context when nested operation completes
        /// </summary>
        public void PopContext(string bugId)
        {
#if DEBUG
            lock (_lockObject)
            {
                if (_contextStacks.TryGetValue(bugId, out var stack) && stack.Count > 0)
                {
                    var contextEntry = stack.Pop();
                    
                    var entry = CreateDiagnosticEntry(bugId, "CONTEXT_POP",
                        "Context popped", "", "", 0);
                    entry.Context = SerializeContext(contextEntry);

                    OutputDiagnosticEntry(entry);

                    if (stack.Count == 0)
                    {
                        _contextStacks.TryRemove(bugId, out _);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Sets the output channel for diagnostic messages
        /// </summary>
        public void SetOutputChannel(DiagnosticOutputChannel channel)
        {
#if DEBUG
            _outputChannel = channel;
#endif
        }

        /// <summary>
        /// Flushes any buffered diagnostic output
        /// </summary>
        public void FlushOutput()
        {
#if DEBUG
            // For current implementation, output is immediate
            // Future enhancement could include buffering for performance
            Debug.WriteLine("DIAGNOSTIC: Output flushed");
#endif
        }

        #region Private Methods

#if DEBUG
        /// <summary>
        /// Creates a standardized diagnostic entry with automatic context
        /// </summary>
        private DiagnosticEntry CreateDiagnosticEntry(string bugId, string type, string message,
            string memberName, string sourceFilePath, int sourceLineNumber)
        {
            return new DiagnosticEntry
            {
                Timestamp = DateTime.Now,
                BugId = bugId,
                Type = type,
                Message = message,
                MemberName = memberName,
                SourceFile = ExtractFileName(sourceFilePath),
                LineNumber = sourceLineNumber,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
        }

        /// <summary>
        /// Intelligently serializes context objects for diagnostic output
        /// </summary>
        private string SerializeContext(object? context)
        {
            if (context == null) return "null";

            try
            {
                // Handle primitive types
                if (context.GetType().IsPrimitive || context is string)
                {
                    return context.ToString() ?? "null";
                }

                // Handle anonymous types and complex objects with JSON serialization
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                return JsonSerializer.Serialize(context, options);
            }
            catch (Exception ex)
            {
                return $"[Serialization Error: {ex.Message}]";
            }
        }

        /// <summary>
        /// Outputs diagnostic entry to configured channels
        /// </summary>
        private void OutputDiagnosticEntry(DiagnosticEntry entry)
        {
            var formattedEntry = FormatDiagnosticEntry(entry);

            if (_outputChannel == DiagnosticOutputChannel.DebugConsole || _outputChannel == DiagnosticOutputChannel.Both)
            {
                Debug.WriteLine(formattedEntry);
            }

            if (_outputChannel == DiagnosticOutputChannel.DiagnosticFile || _outputChannel == DiagnosticOutputChannel.Both)
            {
                WriteToDiagnosticFile(entry.BugId, formattedEntry);
            }
        }

        /// <summary>
        /// Formats diagnostic entry for output
        /// </summary>
        private string FormatDiagnosticEntry(DiagnosticEntry entry)
        {
            var sb = new StringBuilder();
            sb.Append($"DIAGNOSTIC[#{entry.BugId}] ");
            sb.Append($"{entry.Timestamp:HH:mm:ss.fff} ");
            sb.Append($"[{entry.Type}] ");
            sb.Append($"T{entry.ThreadId:D2} ");

            if (!string.IsNullOrEmpty(entry.CorrelationId))
            {
                sb.Append($"[{entry.CorrelationId}] ");
            }

            sb.Append($"{entry.SourceFile}:{entry.LineNumber} ");
            sb.Append($"{entry.MemberName}() ");
            sb.Append($">> {entry.Message}");

            if (!string.IsNullOrEmpty(entry.Context))
            {
                sb.AppendLine();
                sb.Append($"   Context: {entry.Context}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes diagnostic entry to bug-specific file
        /// </summary>
        private void WriteToDiagnosticFile(string bugId, string formattedEntry)
        {
            try
            {
                var diagnosticsFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Diagnostics");
                if (!System.IO.Directory.Exists(diagnosticsFolder))
                {
                    System.IO.Directory.CreateDirectory(diagnosticsFolder);
                }

                var fileName = $"#{bugId}-{DateTime.Now:yyyy-MM-dd}.log";
                var filePath = System.IO.Path.Combine(diagnosticsFolder, fileName);

                System.IO.File.AppendAllText(filePath, formattedEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DIAGNOSTIC ERROR: Failed to write to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts filename from full path for cleaner output
        /// </summary>
        private string ExtractFileName(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "";
            return System.IO.Path.GetFileName(fullPath);
        }
#endif

        #endregion
    }

    /// <summary>
    /// No-op implementation for production builds
    /// </summary>
    public class NullDiagnosticService : IDiagnosticService
    {
        public void LogEvent(string bugId, string message, object? context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { }

        public void LogStateCapture(string bugId, string description, object state,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { }

        public void LogInteraction(string bugId, string interaction,
            string source, string target, object? details = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { }

        public string BeginCorrelation(string bugId, string operationName) => string.Empty;

        public void EndCorrelation(string correlationId, string result = "Completed") { }

        public void PushContext(string bugId, string contextName, object contextData) { }

        public void PopContext(string bugId) { }

        public void SetOutputChannel(DiagnosticOutputChannel channel) { }

        public void FlushOutput() { }
    }
}