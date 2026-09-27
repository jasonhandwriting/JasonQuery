using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Database.Internal.Runtime
{
    internal interface IJasonQueryDatabaseRuntime
    {
        DataTable ExecuteQuery(string connectionString, string databasePassword, string sql);

        void ExecuteNonQuery(string connectionString, string databasePassword, string sql,
                             IReadOnlyList<JasonQueryDatabaseParameter> parameters);

        void ExecuteBatchNonQuery(string connectionString, string databasePassword, IReadOnlyList<string> sqlStatements);

        bool CanOpenDatabase(string connectionString, string databasePassword);

        void ChangePassword(string connectionString, string currentDatabasePassword, string newDatabasePassword);
    }
}
