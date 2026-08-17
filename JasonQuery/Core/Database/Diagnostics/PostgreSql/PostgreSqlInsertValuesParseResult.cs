using System.Collections.Generic;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlInsertValuesParseResult
    {
        public bool Success { get; set; }

        public string FailureReason { get; set; } = string.Empty;

        public string SchemaName { get; set; } = "public";

        public string TableName { get; set; } = string.Empty;

        public List<string> ColumnNames { get; } = new List<string>();

        public List<PostgreSqlInsertValueInfo> Values { get; } = new List<PostgreSqlInsertValueInfo>();

        public string FullTableName
        {
            get
            {
                return string.IsNullOrEmpty(SchemaName) ? TableName : $"{SchemaName}.{TableName}";
            }
        }

        public static PostgreSqlInsertValuesParseResult Fail(string reason)
        {
            return new PostgreSqlInsertValuesParseResult
            {
                Success = false,
                FailureReason = reason
            };
        }
    }
}