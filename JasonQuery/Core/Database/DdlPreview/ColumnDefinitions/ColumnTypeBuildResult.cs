namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal sealed class ColumnTypeBuildResult
    {
        public bool Succeeded { get; set; }

        public string ResolvedDataType { get; set; }

        public string ErrorMessage { get; set; }
    }
}