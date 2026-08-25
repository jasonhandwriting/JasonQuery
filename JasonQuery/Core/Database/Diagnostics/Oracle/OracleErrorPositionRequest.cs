namespace JasonQuery.Core.Database.Diagnostics.Oracle
{
    internal sealed class OracleErrorPositionRequest
    {
        public string EditorSql { get; set; } = string.Empty;

        public string OriginalExecutedSql { get; set; } = string.Empty;

        public string ExecutedSql { get; set; } = string.Empty;

        public string ErrorCode { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public int ReportedPosition { get; set; }

        public int PreferredExecutionStart { get; set; }

        public string ParameterPositionMapping { get; set; } = string.Empty;

        public int ParameterStartPosition { get; set; }
    }
}
