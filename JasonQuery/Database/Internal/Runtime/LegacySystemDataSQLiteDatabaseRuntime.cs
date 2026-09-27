using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace JasonQuery.Database.Internal.Runtime
{
    internal sealed class LegacySystemDataSQLiteDatabaseRuntime : IJasonQueryDatabaseRuntime
    {
        public DataTable ExecuteQuery(string connectionString, string databasePassword, string sql)
        {
            using (var connection = OpenConnection(connectionString, databasePassword))
            using (var dataAdapter = new SQLiteDataAdapter(sql, connection))
            using (var dataSet = new DataSet())
            {
                dataAdapter.Fill(dataSet);

                if (dataSet.Tables.Count == 0)
                {
                    return new DataTable();
                }

                return dataSet.Tables[0];
            }
        }

        public void ExecuteNonQuery(string connectionString, string databasePassword, string sql,
                                    IReadOnlyList<JasonQueryDatabaseParameter> parameters)
        {
            using (var connection = OpenConnection(connectionString, databasePassword))
            using (var command = new SQLiteCommand(sql, connection))
            {
                AddParameters(command, parameters);
                command.ExecuteNonQuery();
            }
        }

        public void ExecuteBatchNonQuery(string connectionString, string databasePassword, IReadOnlyList<string> sqlStatements)
        {
            if (sqlStatements == null)
            {
                throw new ArgumentNullException(nameof(sqlStatements));
            }

            using (var connection = OpenConnection(connectionString, databasePassword))
            {
                foreach (var sqlStatement in sqlStatements)
                {
                    using (var command = new SQLiteCommand(sqlStatement, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public bool CanOpenDatabase(string connectionString, string databasePassword)
        {
            try
            {
                using (var connection = OpenConnection(connectionString, databasePassword))
                {
                    ValidateSystemConfig(connection);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ChangePassword(string connectionString, string currentDatabasePassword, string newDatabasePassword)
        {
            if (string.IsNullOrWhiteSpace(newDatabasePassword))
            {
                throw new ArgumentException("A new database password is required.", nameof(newDatabasePassword));
            }

            using (var connection = OpenConnection(connectionString, currentDatabasePassword))
            {
                ValidateSystemConfig(connection);
                connection.ChangePassword(newDatabasePassword);
            }
        }

        internal IDbConnection OpenValidatedConnection(string connectionString, string databasePassword)
        {
            var connection = OpenConnection(connectionString, databasePassword);

            try
            {
                ValidateSystemConfig(connection);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static SQLiteConnection OpenConnection(string connectionString, string databasePassword)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A database connection string is required.", nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A database password is required.", nameof(databasePassword));
            }

            var connection = new SQLiteConnection
            {
                ConnectionString = connectionString
            };

            try
            {
                connection.SetPassword(databasePassword);
                connection.Open();

                if (connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("The JasonQuery database connection did not open.");
                }

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static void ValidateSystemConfig(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
            {
                command.ExecuteScalar();
            }
        }

        private static void AddParameters(SQLiteCommand command, IReadOnlyList<JasonQueryDatabaseParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }

            for (var index = 0; index < parameters.Count; index++)
            {
                var parameter = parameters[index];

                if (parameter == null)
                {
                    throw new ArgumentException("Database parameters must not contain null entries.", nameof(parameters));
                }

                command.Parameters.Add
                (
                    new SQLiteParameter
                    (
                        parameter.Name,
                        parameter.Value ?? DBNull.Value
                    )
                );
            }
        }
    }
}
