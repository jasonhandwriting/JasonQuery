using JasonLibrary.Core;
using JasonQuery.Core.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace JasonQuery.Core.Logging
{
    public static class TraceLogger
    {
        private static readonly object SyncRoot = new object();
        private static readonly AsyncLocal<TraceScopeState> CurrentScope = new AsyncLocal<TraceScopeState>();

        private static StreamWriter _writer;
        private static string _sessionId = string.Empty;
        private static long _sequence;
        private static long _operationSequence;
        private static Func<TraceLogContext> _contextProvider;

        public static string LogDirectoryPath { get; private set; } = string.Empty;
        public static string CurrentLogFilePath { get; private set; } = string.Empty;
        public static string LastWriteError { get; private set; } = string.Empty;

        public static bool IsEnabled
        {
            get
            {
                lock (SyncRoot)
                {
                    return AppConfigHelper.HasGenerateLogFile && _writer != null;
                }
            }
        }

        public static void Initialize(string applicationDirectory)
        {
            lock (SyncRoot)
            {
                CloseWriterUnsafe(false);

                LogDirectoryPath = TraceLogFileManager.ResolveLogDirectory(applicationDirectory);
                CurrentLogFilePath = string.Empty;
                LastWriteError = string.Empty;
                AppConfigHelper.LogFileName = string.Empty;

                if (!string.IsNullOrWhiteSpace(LogDirectoryPath))
                {
                    TraceLogFileManager.CleanupOldLogFiles(LogDirectoryPath, DateTimeOffset.UtcNow, TraceLogFileManager.DefaultRetentionDays);
                }
            }
        }

        public static void SetContextProvider(Func<TraceLogContext> contextProvider)
        {
            lock (SyncRoot)
            {
                _contextProvider = contextProvider;
            }
        }

        public static bool SetEnabled(bool enabled)
        {
            lock (SyncRoot)
            {
                if (!enabled)
                {
                    CloseWriterUnsafe(true);
                    AppConfigHelper.HasGenerateLogFile = false;
                    return true;
                }

                if (_writer != null)
                {
                    AppConfigHelper.HasGenerateLogFile = true;
                    return true;
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(LogDirectoryPath))
                    {
                        throw new IOException("The runtime log directory is unavailable.");
                    }

                    var now = DateTimeOffset.Now;
                    var processId = GetCurrentProcessId();

                    CurrentLogFilePath = TraceLogFileManager.CreateSessionLogFilePath(LogDirectoryPath, now, processId);

                    _sessionId = TraceLogFileManager.CreateSessionId(now, processId);
                    _sequence = 0;
                    _operationSequence = 0;
                    CurrentScope.Value = null;

                    _writer = new StreamWriter(CurrentLogFilePath, false, new UTF8Encoding(true))
                    {
                        AutoFlush = true
                    };

                    _writer.WriteLine(TraceCsvFormatter.Header);

                    AppConfigHelper.LogFileName = CurrentLogFilePath;
                    AppConfigHelper.HasGenerateLogFile = true;
                    LastWriteError = string.Empty;

                    WriteEntryUnsafe
                    (
                        CreateEntry("Info", "SessionStart", string.Empty, string.Empty, 0,
                                    "Application", "Session", "Started", null, null, null, null,
                                    string.Empty, string.Empty, 0, BuildSessionMessage())
                    );

                    return _writer != null;
                }
                catch (Exception ex)
                {
                    LastWriteError = ex.Message;
                    CloseWriterUnsafe(false);
                    AppConfigHelper.HasGenerateLogFile = false;
                    return false;
                }
            }
        }

        public static void Shutdown()
        {
            SetEnabled(false);
        }

        public static void LogStage(string stage, [CallerMemberName] string callerName = "",
                                    [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            LogStage(ResolveLegacyCategory(callerFile), stage, null, callerName, callerFile, callerLine);
        }

        public static void LogStage(string category, string action, TraceLogContext context, [CallerMemberName] string callerName = "",
                                    [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            lock (SyncRoot)
            {
                if (!AppConfigHelper.HasGenerateLogFile || _writer == null)
                {
                    return;
                }

                var parent = CurrentScope.Value;
                var operationId = CreateOperationId();

                WriteEntryUnsafe
                (
                    CreateEntry("Info", "Stage", operationId,
                                parent?.OperationId ?? string.Empty,
                                parent == null ? 0 : parent.Depth + 1, category, action,
                                "Completed", null, GetPrivateMemoryMb(), null, context,
                                callerName, Path.GetFileName(callerFile), callerLine, null)
                );
            }
        }

        public static void LogError(string action, Exception exception, TraceLogContext context = null, [CallerMemberName] string callerName = "",
                                    [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            if (exception == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (!AppConfigHelper.HasGenerateLogFile || _writer == null)
                {
                    return;
                }

                var parent = CurrentScope.Value;
                var message = BuildExceptionLogMessage(exception, context?.Message);

                WriteEntryUnsafe
                (
                    CreateEntry("Error", "Error", CreateOperationId(),
                                parent?.OperationId ?? string.Empty,
                                parent == null ? 0 : parent.Depth + 1,
                                context?.Category ?? "Application", action, "Failed", null,
                                GetPrivateMemoryMb(), null, context, callerName,
                                Path.GetFileName(callerFile), callerLine, message)
                );
            }
        }

        public static IDisposable Time(string label, [CallerMemberName] string callerName = "",
                                       [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            return CreateTimer(ResolveLegacyCategory(callerFile), label, null, callerName, callerFile, callerLine);
        }

        public static IDisposable Time(string category, string action, TraceLogContext context, [CallerMemberName] string callerName = "",
                                       [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        {
            return CreateTimer(category, action, context, callerName, callerFile, callerLine);
        }

        private static IDisposable CreateTimer(string category, string action, TraceLogContext context,
                                               string callerName, string callerFile, int callerLine)
        {
            lock (SyncRoot)
            {
                if (!AppConfigHelper.HasGenerateLogFile || _writer == null)
                {
                    return EmptyTraceScope.Instance;
                }

                return new TraceTimer(category, action, context, callerName, callerFile, callerLine, _sessionId);
            }
        }

        public static string GetCallerInfo()
        {
            var stackTrace = new StackTrace(true);

            for (var i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                var declaringType = method.DeclaringType;

                if (declaringType != typeof(TraceLogger))
                {
                    var methodName = method.Name;
                    var fileName = Path.GetFileName(frame.GetFileName()) ?? "UnknownFile";
                    int lineNumber = frame.GetFileLineNumber();

                    return $"{methodName} | {fileName}: Line {lineNumber}";
                }
            }

            return "UnknownCaller";
        }

        public static string GetStackTraceContent(string stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
            {
                return string.Empty;
            }

            var result = string.Empty;
            var parts = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i];

                if (value.Contains("\\") && value.Contains(".cs"))
                {
                    result += $"{value}\r\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.TrimEnd('\r', '\n');
            }
            else
            {
                result = stackTrace;
            }

            return result.Trim();
        }

        public static string GetStackTraceMessageAndContent(string stackTrace, string message, bool isErrorMessageOnly = false)
        {
            var result = string.Empty;
            var parts = string.IsNullOrWhiteSpace(stackTrace) ? Array.Empty<string>() : stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i];

                if (value.Contains("\\") && value.Contains(".cs"))
                {
                    result += $"{value.Trim()}\r\n\r\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.TrimEnd('\r', '\n');
            }
            else
            {
                result = stackTrace ?? string.Empty;
            }

            if (isErrorMessageOnly)
            {
                return $"{message}\r\n\r\n{result}";
            }

            return $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{message}\r\n\r\n{MyGlobal.PleaseTryAgain}\r\n\r\n{result}";
        }

        public static string BuildExceptionDisplayMessage2(Exception ex, bool errorMessageOnly = false)
        {
            if (ex == null)
            {
                return MyGlobal.AnUnexpectedErrorHasOccurred;
            }

            var sourceInfo = ExtractSourceLocationsFromStackTrace(ex.StackTrace);
            var message = ex.Message ?? string.Empty;

            if (errorMessageOnly)
            {
                return string.Format("{0}\r\n\r\n{1}", message, sourceInfo);
            }

            return string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}\r\n\r\n{3}", MyGlobal.AnUnexpectedErrorHasOccurred,
                                 message, MyGlobal.PleaseTryAgain, sourceInfo);
        }

        private static string ExtractSourceLocationsFromStackTrace(string stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
            {
                return string.Empty;
            }

            var parts = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i];

                if (value.Contains("\\") && value.Contains(".cs"))
                {
                    sb.AppendLine(value.Trim());
                    sb.AppendLine();
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString().TrimEnd('\r', '\n');
            }

            return stackTrace;
        }

        private static TraceLogEntry CreateEntry(string level, string eventType, string operationId,
                                                 string parentOperationId, int depth, string category,
                                                 string action, string status, long? durationMs,
                                                 double? privateMemoryMb, double? memoryDeltaMb,
                                                 TraceLogContext context, string method, string sourceFile,
                                                 int sourceLine, string message)
        {
            context = ResolveContext(context);

            return new TraceLogEntry
            {
                Timestamp = DateTimeOffset.Now,
                Sequence = Interlocked.Increment(ref _sequence),
                Level = level,
                EventType = eventType,
                SessionId = _sessionId,
                OperationId = operationId,
                ParentOperationId = parentOperationId,
                Depth = depth,
                Category = category,
                Action = action,
                Status = status,
                DurationMs = durationMs,
                PrivateMemoryMb = privateMemoryMb,
                MemoryDeltaMb = memoryDeltaMb,
                AppVersion = GetAppVersion(),
                DatabaseType = context?.DatabaseType,
                DatabaseVersion = context?.DatabaseVersion,
                ConnectionName = context?.ConnectionName,
                ObjectType = context?.ObjectType,
                ObjectName = context?.ObjectName,
                RequestedRows = context?.RequestedRows,
                ReturnedRows = context?.ReturnedRows,
                ColumnCount = context?.ColumnCount,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Method = method,
                SourceFile = sourceFile,
                SourceLine = sourceLine,
                Message = message ?? context?.Message
            };
        }

        private static TraceLogContext ResolveContext(TraceLogContext context)
        {
            TraceLogContext defaults = null;

            try
            {
                defaults = _contextProvider?.Invoke();
            }
            catch
            {
                //診斷內容提供失敗時，仍應保留主要的執行日誌
            }

            if (context == null)
            {
                return defaults;
            }

            if (defaults == null)
            {
                return context;
            }

            return new TraceLogContext
            {
                Category = context.Category ?? defaults.Category,
                DatabaseType = context.DatabaseType ?? defaults.DatabaseType,
                DatabaseVersion = context.DatabaseVersion ?? defaults.DatabaseVersion,
                ConnectionName = context.ConnectionName ?? defaults.ConnectionName,
                ObjectType = context.ObjectType ?? defaults.ObjectType,
                ObjectName = context.ObjectName ?? defaults.ObjectName,
                RequestedRows = context.RequestedRows ?? defaults.RequestedRows,
                ReturnedRows = context.ReturnedRows ?? defaults.ReturnedRows,
                ColumnCount = context.ColumnCount ?? defaults.ColumnCount,
                Message = context.Message ?? defaults.Message
            };
        }

        private static void WriteEntry(TraceLogEntry entry)
        {
            lock (SyncRoot)
            {
                WriteEntryUnsafe(entry);
            }
        }

        private static void WriteEntryUnsafe(TraceLogEntry entry)
        {
            if (_writer == null || entry == null)
            {
                return;
            }

            try
            {
                _writer.WriteLine(TraceCsvFormatter.Format(entry));
            }
            catch (Exception ex)
            {
                LastWriteError = ex.Message;
                CloseWriterUnsafe(false);
                AppConfigHelper.HasGenerateLogFile = false;
            }
        }

        private static void CloseWriterUnsafe(bool writeSessionEnd)
        {
            if (_writer == null)
            {
                return;
            }

            if (writeSessionEnd)
            {
                WriteEntryUnsafe
                (
                    CreateEntry("Info", "SessionEnd", string.Empty, string.Empty, 0,
                                "Application", "Session", "Completed", null,
                                GetPrivateMemoryMb(), null, null, string.Empty, string.Empty, 0, string.Empty)
                );

                if (_writer == null)
                {
                    return;
                }
            }

            try
            {
                _writer.Dispose();
            }
            catch
            {
                //記錄功能不得影響 JasonQuery 原有功能
            }
            finally
            {
                _writer = null;
                _sessionId = string.Empty;
                CurrentScope.Value = null;
            }
        }

        private static string CreateOperationId()
        {
            return $"OP{Interlocked.Increment(ref _operationSequence):D6}";
        }

        private static string ResolveLegacyCategory(string callerFile)
        {
            var sourceName = Path.GetFileNameWithoutExtension(callerFile);

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return "Application";
            }

            var separatorIndex = sourceName.IndexOf('.');

            return separatorIndex > 0 ? sourceName.Substring(0, separatorIndex) : sourceName;
        }

        private static int GetCurrentProcessId()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return process.Id;
            }
        }

        private static double GetPrivateMemoryMb()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return process.PrivateMemorySize64 / (1024.0 * 1024.0);
            }
        }

        private static string GetAppVersion()
        {
            if (!string.IsNullOrWhiteSpace(AppConfigHelper.LocalVersion))
            {
                return AppConfigHelper.LocalVersion;
            }

            return typeof(TraceLogger).Assembly.GetName().Version?.ToString() ?? string.Empty;
        }

        private static string BuildSessionMessage()
        {
            var processArchitecture = IntPtr.Size == 8 ? "x64" : "x86";

            return $"OS={Environment.OSVersion}; CLR={Environment.Version}; Process={processArchitecture}";
        }

        private static string BuildExceptionLogMessage(Exception exception, string contextMessage)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(contextMessage))
            {
                sb.AppendLine(contextMessage);
            }

            sb.Append(exception.GetType().FullName);
            sb.Append(": ");
            sb.Append(exception.Message);

            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                sb.AppendLine();
                sb.Append(exception.StackTrace);
            }

            return sb.ToString();
        }

        private sealed class TraceTimer : IDisposable
        {
            private readonly string _category;
            private readonly string _action;
            private readonly TraceLogContext _context;
            private readonly string _callerName;
            private readonly string _callerFile;
            private readonly int _callerLine;
            private readonly Stopwatch _stopwatch;
            private readonly double _startMemoryMb;
            private readonly TraceScopeState _scopeState;
            private readonly TraceScopeState _previousScope;
            private readonly string _sessionIdAtStart;
            private bool _disposed;

            public TraceTimer(string category, string action, TraceLogContext context,
                              string callerName, string callerFile, int callerLine, string sessionIdAtStart)
            {
                _category = category;
                _action = action;
                _context = context;
                _callerName = callerName;
                _callerFile = Path.GetFileName(callerFile);
                _callerLine = callerLine;
                _sessionIdAtStart = sessionIdAtStart;
                _previousScope = CurrentScope.Value;

                _scopeState = new TraceScopeState
                {
                    OperationId = CreateOperationId(),
                    Depth = _previousScope == null ? 0 : _previousScope.Depth + 1
                };

                CurrentScope.Value = _scopeState;
                _startMemoryMb = GetPrivateMemoryMb();
                _stopwatch = Stopwatch.StartNew();

                WriteEntry
                (
                    CreateEntry("Info", "Start", _scopeState.OperationId,
                                _previousScope?.OperationId ?? string.Empty, _scopeState.Depth,
                                _category, _action, "Started", null, _startMemoryMb, null,
                                _context, _callerName, _callerFile, _callerLine, null)
                );
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _stopwatch.Stop();

                lock (SyncRoot)
                {
                    var isSameSession = AppConfigHelper.HasGenerateLogFile && _writer != null
                                        && string.Equals(_sessionId, _sessionIdAtStart, StringComparison.Ordinal);

                    if (isSameSession)
                    {
                        var endMemoryMb = GetPrivateMemoryMb();

                        WriteEntryUnsafe
                        (
                            CreateEntry("Info", "End", _scopeState.OperationId, _previousScope?.OperationId ?? string.Empty,
                                        _scopeState.Depth, _category, _action, "Completed", _stopwatch.ElapsedMilliseconds, endMemoryMb,
                                        endMemoryMb - _startMemoryMb, _context, _callerName, _callerFile, _callerLine, null)
                        );
                    }

                    if (CurrentScope.Value?.OperationId == _scopeState.OperationId)
                    {
                        CurrentScope.Value = isSameSession ? _previousScope : null;
                    }
                }
            }
        }

        private sealed class TraceScopeState
        {
            public string OperationId { get; set; }
            public int Depth { get; set; }
        }

        private sealed class EmptyTraceScope : IDisposable
        {
            public static readonly EmptyTraceScope Instance = new EmptyTraceScope();

            private EmptyTraceScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
