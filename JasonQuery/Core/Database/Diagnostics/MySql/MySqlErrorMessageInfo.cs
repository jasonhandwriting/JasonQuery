namespace JasonQuery.Core.Database.Diagnostics.MySql
{
    internal sealed class MySqlErrorMessageInfo
    {
        public MySqlErrorMessageKind Kind { get; set; }

        public string TargetText { get; set; } = string.Empty;

        public string ClauseName { get; set; } = string.Empty;

        public int LineNumber { get; set; }

        public string NearText { get; set; } = string.Empty;
    }
}