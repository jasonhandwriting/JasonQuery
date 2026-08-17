using System;

namespace JasonLibrary.Core.Text.Formatting
{
    public sealed class SqlFormatResult
    {
        private SqlFormatResult(SqlFormatterEngineKind engineKind, bool success, string formattedSql, string errorMessage)
        {
            EngineKind = engineKind;
            Success = success;
            FormattedSql = formattedSql ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public SqlFormatterEngineKind EngineKind { get; }

        public bool Success { get; }

        public string FormattedSql { get; }

        public string ErrorMessage { get; }

        public static SqlFormatResult Succeeded(SqlFormatterEngineKind engineKind, string formattedSql)
        {
            if (formattedSql == null)
            {
                throw new ArgumentNullException(nameof(formattedSql));
            }

            return new SqlFormatResult(engineKind, true, formattedSql, string.Empty);
        }

        public static SqlFormatResult Failed(SqlFormatterEngineKind engineKind, string originalSql, string errorMessage)
        {
            return new SqlFormatResult(engineKind, false, originalSql, errorMessage);
        }
    }
}