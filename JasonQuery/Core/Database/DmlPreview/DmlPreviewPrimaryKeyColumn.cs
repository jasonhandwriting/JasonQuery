using JasonLibrary.Core.Schema;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal sealed class DmlPreviewPrimaryKeyColumn
    {
        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; } = int.MaxValue;

        public ColumnInfo ColumnInfo { get; set; }
    }
}
