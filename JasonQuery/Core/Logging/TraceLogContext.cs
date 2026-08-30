namespace JasonQuery.Core.Logging
{
    public sealed class TraceLogContext
    {
        public string Category { get; set; }
        public string DatabaseType { get; set; }
        public string DatabaseVersion { get; set; }
        public string ConnectionName { get; set; }
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public int? RequestedRows { get; set; }
        public int? ReturnedRows { get; set; }
        public int? ColumnCount { get; set; }
        public string Message { get; set; }
    }
}
