using JasonQuery.Database.Internal.Runtime.Modern;
using JasonQuery.ModernDbMigration;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace JasonQuery.ModernDbRuntime
{
    internal sealed class ModernDbRuntimeSession : IDisposable
    {
        private sqlite3 _database;

        internal bool IsOpen => _database != null;

        internal void Open(string databasePath, byte[] databasePasswordUtf8)
        {
            if (IsOpen)
            {
                throw new InvalidOperationException("A Modern DB runtime database is already open.");
            }

            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A database path is required.", nameof(databasePath));
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0)
            {
                throw new ArgumentException("A database credential is required.", nameof(databasePasswordUtf8));
            }

            sqlite3 database = null;

            try
            {
                database = ModernSqlCipherRuntime.OpenReadWrite(databasePath, databasePasswordUtf8);
                ConfigureNormalRuntime(database);
                ModernSqlCipherRuntime.ValidateFrozenProfile(database);
                ModernSqlCipherRuntime.QueryInt64(database, "SELECT count(*) FROM sqlite_master;");
                ModernSqlCipherRuntime.ValidateIntegrity(database);
                _database = database;
                database = null;
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(database);
            }
        }

        internal void Rekey(byte[] newDatabasePasswordUtf8)
        {
            EnsureOpen();

            if (newDatabasePasswordUtf8 == null || newDatabasePasswordUtf8.Length == 0)
            {
                throw new ArgumentException("A new Storage V2 database credential is required.", nameof(newDatabasePasswordUtf8));
            }

            ModernSqlCipherRuntime.Exec(_database, "PRAGMA wal_checkpoint(TRUNCATE);");
            ModernSqlCipherRuntime.Rekey(_database, newDatabasePasswordUtf8);
            ModernSqlCipherRuntime.ValidateFrozenProfile(_database);
            ModernSqlCipherRuntime.QueryInt64(_database, "SELECT count(*) FROM sqlite_master;");
            ModernSqlCipherRuntime.ValidateIntegrity(_database);
        }

        internal void CreateFreshDatabase(string databasePath, byte[] databasePasswordUtf8, byte[] plaintextTemplateBytes)
        {
            if (IsOpen)
            {
                throw new InvalidOperationException("A fresh Storage V2 database cannot be created while a runtime database is open.");
            }

            ModernStorageV2FreshDatabaseCreator.Create
            (
                databasePath,
                databasePasswordUtf8,
                plaintextTemplateBytes
            );
        }

        internal bool CanOpenWithoutKey(string databasePath)
        {
            if (IsOpen)
            {
                throw new InvalidOperationException("No-key validation cannot run while a runtime database is open.");
            }

            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A Storage V2 database path is required.", nameof(databasePath));
            }

            sqlite3 database = null;

            try
            {
                database = ModernSqlCipherRuntime.OpenWithoutKeyReadOnly(databasePath);

                if (database == null)
                {
                    return false;
                }

                ModernSqlCipherRuntime.QueryInt64(database, "SELECT count(*) FROM sqlite_master;");
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(database);
            }
        }

        internal ModernDatabaseRuntimeQueryResult ExecuteQuery(string sql, IReadOnlyList<ModernDatabaseRuntimeParameter> parameters)
        {
            EnsureOpen();

            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("Query SQL is required.", nameof(sql));
            }

            sqlite3_stmt statement = null;
            var temporaryBuffers = new List<byte[]>();

            try
            {
                string tail;

                ModernSqlCipherRuntime.CheckRc
                (
                    _database,
                    raw.sqlite3_prepare_v2(_database, sql, out statement, out tail),
                    "prepare normal-runtime query"
                );

                if (statement == null)
                {
                    throw new InvalidDataException("The normal-runtime query did not produce a SQLite statement.");
                }

                if (!string.IsNullOrWhiteSpace(tail))
                {
                    throw new InvalidDataException("A normal-runtime query must contain exactly one SQL statement.");
                }

                BindParameters(statement, parameters, 0, out var consumedParameters, temporaryBuffers);

                if (consumedParameters != (parameters == null ? 0 : parameters.Count))
                {
                    throw new InvalidDataException("The normal-runtime query parameter count does not match the SQL statement.");
                }

                var columnCount = raw.sqlite3_column_count(statement);
                var columnNames = new string[columnCount];

                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    columnNames[columnIndex] = raw.sqlite3_column_name(statement, columnIndex).utf8_to_string() ?? string.Empty;
                }

                var rows = new List<object[]>();

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(_database, rc, "step normal-runtime query");
                    }

                    var row = new object[columnCount];

                    for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        row[columnIndex] = ReadColumnValue(statement, columnIndex);
                    }

                    rows.Add(row);
                }

                return new ModernDatabaseRuntimeQueryResult(columnNames, rows);
            }
            finally
            {
                ClearBuffers(temporaryBuffers);
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        internal void ExecuteNonQuery(string sql, IReadOnlyList<ModernDatabaseRuntimeParameter> parameters)
        {
            EnsureOpen();

            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("Non-query SQL is required.", nameof(sql));
            }

            var remainingSql = sql;
            var parameterOffset = 0;

            while (!string.IsNullOrWhiteSpace(remainingSql))
            {
                sqlite3_stmt statement = null;
                var temporaryBuffers = new List<byte[]>();

                try
                {
                    string tail;

                    ModernSqlCipherRuntime.CheckRc
                    (
                        _database,
                        raw.sqlite3_prepare_v2(_database, remainingSql, out statement, out tail),
                        "prepare normal-runtime non-query"
                    );

                    if (statement == null)
                    {
                        if (string.Equals(remainingSql, tail, StringComparison.Ordinal))
                        {
                            throw new InvalidDataException("The normal-runtime non-query parser did not make progress.");
                        }

                        remainingSql = tail;
                        continue;
                    }

                    BindParameters(statement, parameters, parameterOffset, out var consumedParameters, temporaryBuffers);
                    parameterOffset += consumedParameters;
                    StepUntilDone(statement, "execute normal-runtime non-query");
                    remainingSql = tail;
                }
                finally
                {
                    ClearBuffers(temporaryBuffers);
                    ModernSqlCipherRuntime.FinalizeStatement(statement);
                }
            }

            if (parameterOffset != (parameters == null ? 0 : parameters.Count))
            {
                throw new InvalidDataException("The normal-runtime non-query parameter count does not match the SQL statements.");
            }
        }

        internal void ExecuteBatchNonQuery(IReadOnlyList<string> sqlStatements)
        {
            EnsureOpen();

            if (sqlStatements == null)
            {
                throw new ArgumentNullException(nameof(sqlStatements));
            }

            ModernSqlCipherRuntime.Exec(_database, "BEGIN IMMEDIATE;");

            try
            {
                for (var index = 0; index < sqlStatements.Count; index++)
                {
                    if (string.IsNullOrWhiteSpace(sqlStatements[index]))
                    {
                        continue;
                    }

                    ExecuteNonQuery(sqlStatements[index], Array.Empty<ModernDatabaseRuntimeParameter>());
                }

                ModernSqlCipherRuntime.Exec(_database, "COMMIT;");
            }
            catch
            {
                ModernSqlCipherRuntime.TryRollback(_database);
                throw;
            }
        }

        internal void Close()
        {
            var database = _database;

            _database = null;
            ModernSqlCipherRuntime.CloseDatabase(database);
        }

        public void Dispose()
        {
            Close();
        }

        private static void ConfigureNormalRuntime(sqlite3 database)
        {
            ModernSqlCipherRuntime.Exec(database, "PRAGMA journal_mode = DELETE;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA synchronous = FULL;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA temp_store = MEMORY;");
            ModernSqlCipherRuntime.Exec(database, "PRAGMA foreign_keys = OFF;");
        }

        private void BindParameters(sqlite3_stmt statement, IReadOnlyList<ModernDatabaseRuntimeParameter> parameters,
                                    int parameterOffset, out int consumedParameters, ICollection<byte[]> temporaryBuffers)
        {
            var bindCount = raw.sqlite3_bind_parameter_count(statement);
            var availableCount = parameters == null ? 0 : parameters.Count;

            if (parameterOffset < 0 || parameterOffset + bindCount > availableCount)
            {
                throw new InvalidDataException("The normal-runtime SQL statement does not have enough parameter values.");
            }

            for (var bindIndex = 1; bindIndex <= bindCount; bindIndex++)
            {
                var parameter = parameters[parameterOffset + bindIndex - 1];
                var sqliteParameterName = raw.sqlite3_bind_parameter_name(statement, bindIndex).utf8_to_string();

                if (!string.IsNullOrEmpty(sqliteParameterName) && !string.Equals(sqliteParameterName, parameter.Name, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The normal-runtime parameter order does not match the SQL parameter names.");
                }

                BindValue(statement, bindIndex, parameter.Value, temporaryBuffers);
            }

            consumedParameters = bindCount;
        }

        private void BindValue(sqlite3_stmt statement, int parameterIndex, object value, ICollection<byte[]> temporaryBuffers)
        {
            if (value == null || value == DBNull.Value)
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    _database,
                    raw.sqlite3_bind_null(statement, parameterIndex),
                    "bind normal-runtime NULL parameter"
                );

                return;
            }

            if (value is bool booleanValue)
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    _database,
                    raw.sqlite3_bind_int64(statement, parameterIndex, booleanValue ? 1L : 0L),
                    "bind normal-runtime Boolean parameter"
                );

                return;
            }

            if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long)
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    _database,
                    raw.sqlite3_bind_int64(statement, parameterIndex, Convert.ToInt64(value, CultureInfo.InvariantCulture)),
                    "bind normal-runtime Int64 parameter"
                );

                return;
            }

            if (value is float || value is double)
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    _database,
                    raw.sqlite3_bind_double(statement, parameterIndex, Convert.ToDouble(value, CultureInfo.InvariantCulture)),
                    "bind normal-runtime Double parameter"
                );

                return;
            }

            if (value is byte[] blobValue)
            {
                var blobCopy = new byte[blobValue.Length];

                Buffer.BlockCopy(blobValue, 0, blobCopy, 0, blobValue.Length);
                temporaryBuffers.Add(blobCopy);

                ModernSqlCipherRuntime.CheckRc
                (
                    _database,
                    raw.sqlite3_bind_blob(statement, parameterIndex, new ReadOnlySpan<byte>(blobCopy)),
                    "bind normal-runtime BLOB parameter"
                );

                return;
            }

            string textValue;

            if (value is decimal decimalValue)
            {
                textValue = decimalValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is DateTime dateTimeValue)
            {
                textValue = dateTimeValue.ToString("o", CultureInfo.InvariantCulture);
            }
            else if (value is Guid guidValue)
            {
                textValue = guidValue.ToString("D");
            }
            else if (value is string stringValue)
            {
                textValue = stringValue;
            }
            else
            {
                throw new NotSupportedException
                (
                    "The isolated Modern DB runtime does not support SQLite parameter type '" +
                    value.GetType().FullName + "'."
                );
            }

            var textBytes = new UTF8Encoding(false, true).GetBytes(textValue);

            temporaryBuffers.Add(textBytes);

            ModernSqlCipherRuntime.CheckRc
            (
                _database,
                raw.sqlite3_bind_text(statement, parameterIndex, new ReadOnlySpan<byte>(textBytes)),
                "bind normal-runtime TEXT parameter"
            );
        }

        private void StepUntilDone(sqlite3_stmt statement, string operation)
        {
            while (true)
            {
                var rc = raw.sqlite3_step(statement);

                if (rc == raw.SQLITE_DONE)
                {
                    return;
                }

                if (rc == raw.SQLITE_ROW)
                {
                    continue;
                }

                ModernSqlCipherRuntime.CheckRc(_database, rc, operation);
            }
        }

        private static object ReadColumnValue(sqlite3_stmt statement, int columnIndex)
        {
            var type = raw.sqlite3_column_type(statement, columnIndex);

            switch (type)
            {
                case raw.SQLITE_NULL:
                    {
                        return DBNull.Value;
                    }
                case raw.SQLITE_INTEGER:
                    {
                        return raw.sqlite3_column_int64(statement, columnIndex);
                    }
                case raw.SQLITE_FLOAT:
                    {
                        return raw.sqlite3_column_double(statement, columnIndex);
                    }
                case raw.SQLITE_TEXT:
                    {
                        return raw.sqlite3_column_text(statement, columnIndex).utf8_to_string() ?? string.Empty;
                    }
                case raw.SQLITE_BLOB:
                    {
                        return raw.sqlite3_column_blob(statement, columnIndex).ToArray();
                    }
                default:
                    {
                        throw new InvalidDataException("The isolated Modern DB runtime returned an unsupported SQLite column type.");
                    }
            }
        }

        private static void ClearBuffers(IEnumerable<byte[]> buffers)
        {
            foreach (var buffer in buffers)
            {
                if (buffer != null)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
        }

        private void EnsureOpen()
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("The isolated Modern DB runtime database is not open.");
            }
        }
    }
}
