using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.Database.DdlPreview
{
    internal sealed class TableDdlRequest
    {
        public DataSourceType DataSourceType { get; set; }

        public TableDdlOperation Operation { get; set; }

        public string SchemaDatabase { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string NewTableName { get; set; }

        public string Comment { get; set; }

        public bool HasExistingSqlServerComment { get; set; }

        public bool OracleCascadeConstraints { get; set; }

        public bool OraclePurge { get; set; }

        public bool OracleReuseStorage { get; set; }

        public bool PostgreSqlDropCascade { get; set; }

        public bool PostgreSqlOnly { get; set; }

        public bool PostgreSqlRestartIdentity { get; set; }

        public bool PostgreSqlTruncateCascade { get; set; }
    }
}