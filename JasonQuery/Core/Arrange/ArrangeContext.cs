using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Schema;
using JasonQuery.Display.Columns;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Arrange
{
    public class ArrangeContext
    {
        public DataTable SchemaTable { get; set; }
        public DataTable SourceData { get; set; }
        public DataTable SortedData { get; set; }
        public string DistinctTableNameViewName { get; set; } = string.Empty;
        public List<(string Schema, string Table)> TableNameForUpdateComment { get; set; }
        public bool HasAnyComment { get; set; } = false;
        public FormatterMode FormatterMode { get; set; }
        public ColumnInfoCollector columnInfoCollector { get; set; }
        public ColumnDisplayContext DisplayContext { get; set; }
        public string NullDisplayText { get; set; }
        public int LargeTextPreviewLength { get; set; }
        public string DateFormat { get; set; }
        public string DateTimeFormat { get; set; }
        public bool ShowColumnType { get; set; } = false;
        public bool ShowColumnComments { get; set; } = false; //控制 Header 是否需要顯示 Comment
        public bool LoadColumnCommentsForCellTip { get; set; } = false; //控制是否預先載入 Comment 給 CellTip 使用
        public bool ShowColumnDefaultValue { get; set; } = false;
        public Func<bool> IsCancellationRequested { get; set; }
        public string TruncatedText { get; set; } = "...(truncated)";
        public int totalRows { get; set; } = 0;
    }
}
