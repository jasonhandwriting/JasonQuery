using System.Collections.Generic;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlUpdateSetParseResult
    {
        public bool Success { get; set; }

        public string FailureReason { get; set; } = string.Empty;

        public string SchemaName { get; set; } = "public";

        public string TableName { get; set; } = string.Empty;

        public List<PostgreSqlUpdateSetValueInfo> SetValues { get; } = new List<PostgreSqlUpdateSetValueInfo>();

        public string FullTableName
        {
            get
            {
                return string.IsNullOrEmpty(SchemaName) ? TableName : $"{SchemaName}.{TableName}";
            }
        }

        public static PostgreSqlUpdateSetParseResult Fail(string reason)
        {
            return new PostgreSqlUpdateSetParseResult
            {
                Success = false,
                FailureReason = reason
            };
        }
    }
}
