using System.Collections.Generic;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlStringLengthDiagnostic
    {
        public PostgreSqlStringLengthDiagnosticConfidence Confidence { get; set; } = PostgreSqlStringLengthDiagnosticConfidence.Unknown;

        public string Message { get; set; } = string.Empty;

        public List<PostgreSqlStringLengthCandidate> Candidates { get; } = new List<PostgreSqlStringLengthCandidate>();

        public bool HasDiagnosticMessage
        {
            get { return !string.IsNullOrWhiteSpace(Message); }
        }

        public static PostgreSqlStringLengthDiagnostic None()
        {
            return new PostgreSqlStringLengthDiagnostic
            {
                Confidence = PostgreSqlStringLengthDiagnosticConfidence.Unknown,
                Message = string.Empty
            };
        }

        public static PostgreSqlStringLengthDiagnostic Unknown(string reason)
        {
            //無法判斷時不顯示任何額外提示
            return None();
        }
    }
}
