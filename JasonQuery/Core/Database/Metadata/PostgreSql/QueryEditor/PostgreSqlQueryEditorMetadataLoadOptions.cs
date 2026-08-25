using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.QueryEditor
{
    internal sealed class PostgreSqlQueryEditorMetadataLoadOptions
    {
        public DataTable TargetSchemaTable { get; set; }

        public string DbConnectionName { get; set; } = string.Empty;

        public bool IsVersion11OrGreater { get; set; }

        public bool SortByColumnName { get; set; }

        public bool EnableFunctionAutoComplete { get; set; }

        public bool EnableTableAutoComplete { get; set; }

        public bool EnableTriggerAutoComplete { get; set; }

        public bool EnableViewAutoComplete { get; set; }

        public int DoEventsInterval { get; set; } = 500;

        public void Validate()
        {
            if (TargetSchemaTable == null)
            {
                throw new InvalidOperationException("TargetSchemaTable cannot be null !");
            }

            if (string.IsNullOrWhiteSpace(DbConnectionName))
            {
                throw new InvalidOperationException("DbConnectionName cannot be null !");
            }
        }
    }
}
