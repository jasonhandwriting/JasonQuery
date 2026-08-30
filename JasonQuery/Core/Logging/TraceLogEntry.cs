using System;

namespace JasonQuery.Core.Logging
{
    internal sealed class TraceLogEntry
    {
        public DateTimeOffset Timestamp { get; set; }
        public long Sequence { get; set; }
        public string Level { get; set; }
        public string EventType { get; set; }
        public string SessionId { get; set; }
        public string OperationId { get; set; }
        public string ParentOperationId { get; set; }
        public int Depth { get; set; }
        public string Category { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public long? DurationMs { get; set; }
        public double? PrivateMemoryMb { get; set; }
        public double? MemoryDeltaMb { get; set; }
        public string AppVersion { get; set; }
        public string DatabaseType { get; set; }
        public string DatabaseVersion { get; set; }
        public string ConnectionName { get; set; }
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public int? RequestedRows { get; set; }
        public int? ReturnedRows { get; set; }
        public int? ColumnCount { get; set; }
        public int ThreadId { get; set; }
        public string Method { get; set; }
        public string SourceFile { get; set; }
        public int SourceLine { get; set; }
        public string Message { get; set; }
    }
}
