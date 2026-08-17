namespace JasonQuery.Core.Export
{
    internal sealed class ExportCellValueResolution
    {
        public bool IsNull { get; set; }
        public object Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}