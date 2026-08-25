using JasonQuery.Core.Localization;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    internal static class PostgreSqlStringLengthDiagnosticText
    {
        public static string Title
        {
            get { return Get("JasonQuery diagnostic hint:", "PostgreSql22001DiagnosticTitle"); }
        }

        public static string ExactDescription
        {
            get { return Get("PostgreSQL did not return the column name. The following result was inferred by JasonQuery from the SQL statement and column metadata.", "PostgreSql22001ExactDescription"); }
        }

        public static string MultipleCandidateDescription
        {
            get { return Get("PostgreSQL did not return the column name. JasonQuery found multiple columns whose input values exceed their length limits.", "PostgreSql22001MultipleCandidateDescription"); }
        }

        public static string FallbackCandidateDescription
        {
            get { return Get("PostgreSQL did not return the column name. JasonQuery could not identify the exact column.", "PostgreSql22001FallbackCandidateDescription"); }
        }

        public static string FallbackCandidateHint
        {
            get { return Get("The following columns have length limits that match the PostgreSQL error message. Please check them first:", "PostgreSql22001FallbackCandidateHint"); }
        }

        public static string EstimatedColumnLabel
        {
            get { return Get("Estimated column", "PostgreSql22001EstimatedColumnLabel"); }
        }

        public static string ValuesRowLabel
        {
            get { return Get("VALUES row", "PostgreSql22001ValuesRowLabel"); }
        }

        public static string DataTypeLabel
        {
            get { return Get("Column type", "PostgreSql22001DataTypeLabel"); }
        }

        public static string ActualLengthLabel
        {
            get { return Get("Input length", "PostgreSql22001ActualLengthLabel"); }
        }

        public static string MaxLengthLabel
        {
            get { return Get("Length limit", "PostgreSql22001MaxLengthLabel"); }
        }

        public static string SourceValuePreviewLabel
        {
            get { return Get("Input value preview", "PostgreSql22001SourceValuePreviewLabel"); }
        }

        public static string CharactersUnit
        {
            get { return Get("characters", "PostgreSql22001CharactersUnit"); }
        }

        public static string ExactConfidenceLine
        {
            get { return Get("Diagnostic level: Exact", "PostgreSql22001ExactConfidenceLine"); }
        }

        public static string CandidateConfidenceLine
        {
            get { return Get("Diagnostic level: Candidate", "PostgreSql22001CandidateConfidenceLine"); }
        }

        public static string BuildValuesRowText(int rowIndex)
        {
            var format = Get("Row {0}", "PostgreSql22001ValuesRowFormat");
            return string.Format(format, rowIndex);
        }

        public static string BuildCandidateValuesRowPrefix(int rowIndex)
        {
            var format = Get("VALUES row {0}, ", "PostgreSql22001CandidateValuesRowPrefixFormat");
            return string.Format(format, rowIndex);
        }

        private static string Get(string defaultText, string key)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "Global", "Global", "msg", key, "Text");
        }
    }
}
