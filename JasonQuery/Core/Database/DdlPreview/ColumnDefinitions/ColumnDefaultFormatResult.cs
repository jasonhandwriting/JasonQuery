namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal sealed class ColumnDefaultFormatResult
    {
        public bool Succeeded { get; set; }

        public string SqlClause { get; set; }

        public string ErrorMessage { get; set; }
    }
}
