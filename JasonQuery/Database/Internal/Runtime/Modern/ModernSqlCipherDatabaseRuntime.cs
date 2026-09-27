using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.Database.Internal.Runtime.Modern
{
    internal sealed class ModernSqlCipherDatabaseRuntime : IJasonQueryDatabaseRuntime, IDisposable
    {
        private readonly string _runtimeExecutablePath;
        private readonly object _syncRoot = new object();

        private ModernDatabaseRuntimeProcessClient _client;
        private string _openDatabasePath;
        private byte[] _openCredentialFingerprint;
        private bool _disposed;

        internal ModernSqlCipherDatabaseRuntime() : this(GetDefaultExecutablePath())
        {
        }

        internal ModernSqlCipherDatabaseRuntime(string runtimeExecutablePath)
        {
            if (string.IsNullOrWhiteSpace(runtimeExecutablePath))
            {
                throw new ArgumentException("A Modern DB runtime executable path is required.", nameof(runtimeExecutablePath));
            }

            _runtimeExecutablePath = Path.GetFullPath(runtimeExecutablePath);
        }

        public DataTable ExecuteQuery(string connectionString, string databasePassword, string sql)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                try
                {
                    EnsureOpen(connectionString, databasePassword);

                    var result = _client.ExecuteQuery(sql, Array.Empty<ModernDatabaseRuntimeParameter>());

                    return CreateDataTable(result);
                }
                finally
                {
                    DisposeClient();
                }
            }
        }

        public void ExecuteNonQuery(string connectionString, string databasePassword, string sql, IReadOnlyList<JasonQueryDatabaseParameter> parameters)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                try
                {
                    EnsureOpen(connectionString, databasePassword);
                    _client.ExecuteNonQuery(sql, ConvertParameters(parameters));
                }
                finally
                {
                    DisposeClient();
                }
            }
        }

        public void ExecuteBatchNonQuery(string connectionString, string databasePassword, IReadOnlyList<string> sqlStatements)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                try
                {
                    EnsureOpen(connectionString, databasePassword);
                    _client.ExecuteBatchNonQuery(sqlStatements);
                }
                finally
                {
                    DisposeClient();
                }
            }
        }

        public bool CanOpenDatabase(string connectionString, string databasePassword)
        {
            ThrowIfDisposed();

            var databasePath = ExtractDatabasePath(connectionString);
            var credentialBytes = EncodeDatabasePassword(databasePassword);

            try
            {
                using (var client = new ModernDatabaseRuntimeProcessClient(_runtimeExecutablePath))
                {
                    client.Open(databasePath, credentialBytes);
                    client.CloseDatabase();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                Array.Clear(credentialBytes, 0, credentialBytes.Length);
            }
        }

        public void ChangePassword(string connectionString, string currentDatabasePassword, string newDatabasePassword)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                var databasePath = ExtractDatabasePath(connectionString);
                var currentCredentialBytes = EncodeDatabasePassword(currentDatabasePassword);
                var newCredentialBytes = EncodeDatabasePassword(newDatabasePassword);

                try
                {
                    if (FixedTimeEquals(currentCredentialBytes, newCredentialBytes))
                    {
                        throw new ArgumentException
                        (
                            "The new Storage V2 database credential must differ from the current credential.",
                            nameof(newDatabasePassword)
                        );
                    }

                    DisposeClient();

                    using (var client = new ModernDatabaseRuntimeProcessClient(_runtimeExecutablePath))
                    {
                        client.Open(databasePath, currentCredentialBytes);
                        client.Rekey(newCredentialBytes);
                        client.CloseDatabase();
                    }

                    if (!CanOpenDatabase(connectionString, newDatabasePassword))
                    {
                        throw new InvalidDataException
                        (
                            "The Storage V2 database could not be validated with its new credential after rekey."
                        );
                    }

                    if (CanOpenDatabase(connectionString, currentDatabasePassword))
                    {
                        throw new InvalidDataException
                        (
                            "The Storage V2 database still accepts its previous credential after rekey."
                        );
                    }
                }
                finally
                {
                    DisposeClient();
                    Array.Clear(currentCredentialBytes, 0, currentCredentialBytes.Length);
                    Array.Clear(newCredentialBytes, 0, newCredentialBytes.Length);
                }
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                DisposeClient();
            }
        }

        internal void CreateFreshDatabase(string databaseFilePath, string databasePassword, byte[] plaintextTemplateBytes)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                if (string.IsNullOrWhiteSpace(databaseFilePath))
                {
                    throw new ArgumentException("A fresh Storage V2 database path is required.", nameof(databaseFilePath));
                }

                if (plaintextTemplateBytes == null || plaintextTemplateBytes.Length == 0 || plaintextTemplateBytes.Length > ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes)
                {
                    throw new ArgumentException("The fresh Storage V2 template payload is invalid.", nameof(plaintextTemplateBytes));
                }

                var credentialBytes = EncodeDatabasePassword(databasePassword);

                try
                {
                    using (var client = new ModernDatabaseRuntimeProcessClient(_runtimeExecutablePath))
                    {
                        client.CreateFreshDatabase
                        (
                            Path.GetFullPath(databaseFilePath),
                            credentialBytes,
                            plaintextTemplateBytes
                        );
                    }
                }
                finally
                {
                    Array.Clear(credentialBytes, 0, credentialBytes.Length);
                }
            }
        }

        internal bool CanOpenWithoutKey(string databaseFilePath)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                if (string.IsNullOrWhiteSpace(databaseFilePath))
                {
                    throw new ArgumentException("A Storage V2 database path is required.", nameof(databaseFilePath));
                }

                using (var client = new ModernDatabaseRuntimeProcessClient(_runtimeExecutablePath))
                {
                    return client.CanOpenWithoutKey(Path.GetFullPath(databaseFilePath));
                }
            }
        }

        internal ModernDatabaseRuntimeIdentity ProbeQualifiedRuntime()
        {
            ThrowIfDisposed();

            using (var client = new ModernDatabaseRuntimeProcessClient(_runtimeExecutablePath))
            {
                return client.ProbeQualifiedRuntime();
            }
        }

        internal void ReleaseDatabaseHandle()
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();
                DisposeClient();
            }
        }

        internal static string ExtractDatabasePath(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A JasonQuery internal database connection string is required.", nameof(connectionString));
            }

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (!builder.TryGetValue("Data Source", out var value) || value == null)
            {
                throw new InvalidDataException("The JasonQuery internal database connection string does not contain Data Source.");
            }

            var databasePath = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new InvalidDataException("The JasonQuery internal database Data Source is empty.");
            }

            return Path.GetFullPath(databasePath);
        }

        private void EnsureOpen(string connectionString, string databasePassword)
        {
            var databasePath = ExtractDatabasePath(connectionString);
            var credentialBytes = EncodeDatabasePassword(databasePassword);
            var fingerprint = ComputeSha256(credentialBytes);

            try
            {
                if (_client != null && string.Equals(_openDatabasePath, databasePath, StringComparison.OrdinalIgnoreCase)
                    && FixedTimeEquals(_openCredentialFingerprint, fingerprint))
                {
                    return;
                }

                DisposeClient();
                _client = new ModernDatabaseRuntimeProcessClient(_runtimeExecutablePath);
                _client.Open(databasePath, credentialBytes);
                _openDatabasePath = databasePath;
                _openCredentialFingerprint = fingerprint;
                fingerprint = null;
            }
            catch
            {
                DisposeClient();
                throw;
            }
            finally
            {
                Array.Clear(credentialBytes, 0, credentialBytes.Length);

                if (fingerprint != null)
                {
                    Array.Clear(fingerprint, 0, fingerprint.Length);
                }
            }
        }

        private void DisposeClient()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            _openDatabasePath = null;

            if (_openCredentialFingerprint != null)
            {
                Array.Clear(_openCredentialFingerprint, 0, _openCredentialFingerprint.Length);
                _openCredentialFingerprint = null;
            }
        }

        private static List<ModernDatabaseRuntimeParameter> ConvertParameters(IReadOnlyList<JasonQueryDatabaseParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return new List<ModernDatabaseRuntimeParameter>();
            }

            var result = new List<ModernDatabaseRuntimeParameter>(parameters.Count);

            for (var index = 0; index < parameters.Count; index++)
            {
                var parameter = parameters[index] ?? throw new ArgumentException("A database parameter is null.", nameof(parameters));

                result.Add(new ModernDatabaseRuntimeParameter(parameter.Name, parameter.Value));
            }

            return result;
        }

        private static DataTable CreateDataTable(ModernDatabaseRuntimeQueryResult result)
        {
            var table = new DataTable
            {
                Locale = CultureInfo.InvariantCulture
            };

            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var columnIndex = 0; columnIndex < result.ColumnNames.Length; columnIndex++)
            {
                var columnName = CreateUniqueColumnName(result.ColumnNames[columnIndex], columnIndex, usedNames);

                table.Columns.Add(columnName, typeof(object));
            }

            for (var rowIndex = 0; rowIndex < result.Rows.Count; rowIndex++)
            {
                var sourceRow = result.Rows[rowIndex];
                var row = table.NewRow();

                for (var columnIndex = 0; columnIndex < sourceRow.Length; columnIndex++)
                {
                    row[columnIndex] = sourceRow[columnIndex] ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        private static string CreateUniqueColumnName(string rawName, int columnIndex, ISet<string> usedNames)
        {
            var baseName = string.IsNullOrWhiteSpace(rawName) ? "Column" + (columnIndex + 1).ToString(CultureInfo.InvariantCulture) : rawName;
            var candidate = baseName;
            var suffix = 2;

            while (!usedNames.Add(candidate))
            {
                candidate = baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture);
                suffix++;
            }

            return candidate;
        }

        private static byte[] EncodeDatabasePassword(string databasePassword)
        {
            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A resolved Storage V2 database credential is required.", nameof(databasePassword));
            }

            return new UTF8Encoding(false, true).GetBytes(databasePassword);
        }

        private static byte[] ComputeSha256(byte[] value)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(value);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;

            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }

        private static string GetDefaultExecutablePath()
        {
            return Path.Combine
            (
                AppDomain.CurrentDomain.BaseDirectory,
                "ModernRuntime",
                "JasonQuery.ModernDbRuntime.exe"
            );
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ModernSqlCipherDatabaseRuntime));
            }
        }
    }
}
