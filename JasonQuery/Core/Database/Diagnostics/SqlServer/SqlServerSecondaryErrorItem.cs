namespace JasonQuery.Core.Database.Diagnostics.SqlServer
{
    internal sealed class SqlServerSecondaryErrorItem
    {
        public int LineNumber { get; set; }

        public string TargetText { get; set; } = string.Empty;

        public int PositionInLine { get; set; }
    }
}
