using JasonLibrary.Core.Schema;
using JasonQuery.Core.Database.Connection;
using System.Collections.Generic;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal sealed class DmlPreviewLiteralFormatRequest
    {
        public DataSourceType DataSourceType { get; set; }

        public string CellValue { get; set; }

        public ColumnInfo ColumnInfo { get; set; }

        public string DefaultValue { get; set; }

        public bool AllowDefaultKeyword { get; set; } = true;

        public bool UseUpperCaseKeywords { get; set; } = true;

        public IEnumerable<string> NullValueIndicators { get; set; }
    }
}
