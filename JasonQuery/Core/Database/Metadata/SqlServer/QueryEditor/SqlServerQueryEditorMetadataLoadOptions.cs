using System;

namespace JasonQuery.Core.Database.Metadata.SqlServer.QueryEditor
{
    internal sealed class SqlServerQueryEditorMetadataLoadOptions
    {
        public string excludeIsMsShippedClause { get; set; } = string.Empty;

        public string sortByColumnName { get; set; } = string.Empty;

        public bool enableFunctionAutoComplete { get; set; }

        public bool enableTableAutoComplete { get; set; }

        public bool enableTriggerAutoComplete { get; set; }

        public bool enableViewAutoComplete { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(sortByColumnName))
            {
                throw new InvalidOperationException("sortByColumnName cannot be null !");
            }
        }
    }
}
