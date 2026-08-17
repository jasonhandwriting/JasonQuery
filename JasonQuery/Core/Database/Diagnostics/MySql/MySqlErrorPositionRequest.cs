namespace JasonQuery.Core.Database.Diagnostics.MySql
{
    internal sealed class MySqlErrorPositionRequest
    {
        public string EditorSql { get; set; } = string.Empty;

        public string OriginalExecutedSql { get; set; } = string.Empty;

        public string ExecutedSql { get; set; } = string.Empty;

        public string ErrorCode { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public int ReportedPosition { get; set; }

        public int PositionOffset { get; set; }

        public int PreferredExecutionStart { get; set; }

        public string ParameterPositionMapping { get; set; } = string.Empty;

        public int ParameterStartPosition { get; set; }

        public string CurrentDatabaseName { get; set; } = string.Empty;
    }
}