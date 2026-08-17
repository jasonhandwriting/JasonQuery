using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.Oracle
{
    internal sealed class OracleMetadataContext
    {
        public DataTable TargetSchemaTable { get; set; }

        public string DbConnectionName { get; set; }

        public string DbUserNameUppercase { get; set; }

        public bool NeedSchemaRows { get; set; }

        public bool SortByColumnName { get; set; }

        public Func<string, DataTable> ExecuteQuery { get; set; }
    }
}