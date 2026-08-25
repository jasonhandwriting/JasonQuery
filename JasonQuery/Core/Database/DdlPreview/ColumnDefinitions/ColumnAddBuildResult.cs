namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal sealed class ColumnAddBuildResult
    {
        public bool Succeeded { get; set; }

        public string Sql { get; set; }

        public string ResolvedDataType { get; set; }

        public ColumnAddBuildFailureKind FailureKind { get; set; }

        public string ErrorMessage { get; set; }
    }
}
