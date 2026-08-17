using JasonQuery.Core.Database.Connection;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal sealed class DmlPreviewWhereConditionRequest
    {
        public DataSourceType DataSourceType { get; set; }

        public DataRow CurrentRow { get; set; }

        public DataTable OriginalTable { get; set; }

        public string RowIdentityColumnName { get; set; }

        public bool HasDeclaredPrimaryKey { get; set; }

        public IEnumerable<DmlPreviewPrimaryKeyColumn> PrimaryKeyColumns { get; set; }

        public bool UseUpperCaseKeywords { get; set; } = true;

        public IEnumerable<string> NullValueIndicators { get; set; }
    }
}