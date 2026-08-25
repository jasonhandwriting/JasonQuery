using System.Collections.Generic;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public interface IPostgreSqlColumnLengthMetadataProvider
    {
        IReadOnlyList<PostgreSqlColumnLengthInfo> GetColumnLengthInfo(string schemaName, string tableName);

        bool TryGetColumnLengthInfo(string schemaName, string tableName, string columnName, out PostgreSqlColumnLengthInfo columnInfo);
    }
}
