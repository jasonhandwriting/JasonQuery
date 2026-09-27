using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Core.Security.Legacy;
using JasonQuery.Database.Internal.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Security
{
    [TestClass]
    public class SqliteConnectionCredentialStorageStartupGateTests
    {
        [TestMethod]
        public void EnsureReady_WhenLegacy_MigratesCredentialBeforeReturning()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    const string domainUser = @"TESTDOMAIN\StartupUser";
                    const string logicalPassword = "startup-O'Brien-資料庫";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        1,
                        domainUser,
                        LegacyConnectionCredentialSecurity.Protect(logicalPassword, domainUser)
                    );

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        var gate = new SqliteConnectionCredentialStorageStartupGate();

                        Assert.IsTrue(gate.EnsureReady(connection));
                    }

                    Assert.AreEqual(logicalPassword, ReadPassword(databaseFilePath, 1));
                    Assert.AreEqual(ConnectionCredentialStorageContract.CurrentVersion, ReadPersistedVersion(databaseFilePath));
                    AssertMigrationCompatibleMetadataSchema(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void EnsureReady_WhenAlreadyV2_IsNoOpWithoutContentHeuristics()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    const string domainUser = @"TESTDOMAIN\CurrentUser";
                    var legacyLookingValue = LegacyConnectionCredentialSecurity.Protect("must-remain-unchanged", domainUser);

                    InsertDbInfo(databaseFilePath, 2, domainUser, legacyLookingValue);
                    SeedPersistedVersion(databaseFilePath, ConnectionCredentialStorageContract.CurrentVersion.ToString(CultureInfo.InvariantCulture));

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        var gate = new SqliteConnectionCredentialStorageStartupGate();

                        Assert.IsFalse(gate.EnsureReady(connection));
                    }

                    Assert.AreEqual(legacyLookingValue, ReadPassword(databaseFilePath, 2));
                    Assert.AreEqual(ConnectionCredentialStorageContract.CurrentVersion, ReadPersistedVersion(databaseFilePath));
                    AssertMigrationCompatibleMetadataSchema(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void EnsureReady_WhenVersionIsUnknown_FailsClosed()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    InsertDbInfo(databaseFilePath, 3, @"TESTDOMAIN\UnknownUser", "unchanged-value");
                    SeedPersistedVersion(databaseFilePath, "999");

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        var gate = new SqliteConnectionCredentialStorageStartupGate();

                        Assert.ThrowsExactly<NotSupportedException>(() => gate.EnsureReady(connection));
                    }

                    Assert.AreEqual("unchanged-value", ReadPassword(databaseFilePath, 3));
                    Assert.AreEqual("999", ReadRawPersistedVersion(databaseFilePath));
                }
            );
        }

        [TestMethod]
        public void EnsureReady_WhenMigrationFails_DoesNotAllowRuntimeReadyState()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    const string domainUser = @"TESTDOMAIN\RollbackUser";
                    var firstProtectedPassword = LegacyConnectionCredentialSecurity.Protect("must-roll-back", domainUser);

                    InsertDbInfo(databaseFilePath, 4, domainUser, firstProtectedPassword);
                    InsertDbInfo(databaseFilePath, 5, domainUser, "not-valid-legacy-ciphertext");

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        var gate = new SqliteConnectionCredentialStorageStartupGate();

                        Assert.ThrowsExactly<InvalidDataException>(() => gate.EnsureReady(connection));
                    }

                    Assert.AreEqual(firstProtectedPassword, ReadPassword(databaseFilePath, 4));
                    Assert.AreEqual("not-valid-legacy-ciphertext", ReadPassword(databaseFilePath, 5));
                    Assert.IsNull(ReadPersistedVersion(databaseFilePath));
                }
            );
        }

        [TestMethod]
        public void EnsureReady_WhenCalledAgainAfterMigration_IsIdempotent()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    const string domainUser = @"TESTDOMAIN\IdempotentStartupUser";
                    const string logicalPassword = "idempotent-startup-value";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        6,
                        domainUser,
                        LegacyConnectionCredentialSecurity.Protect(logicalPassword, domainUser)
                    );

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        var gate = new SqliteConnectionCredentialStorageStartupGate();

                        Assert.IsTrue(gate.EnsureReady(connection));
                        Assert.IsFalse(gate.EnsureReady(connection));
                    }

                    Assert.AreEqual(logicalPassword, ReadPassword(databaseFilePath, 6));
                    Assert.AreEqual(ConnectionCredentialStorageContract.CurrentVersion, ReadPersistedVersion(databaseFilePath));
                }
            );
        }

        [TestMethod]
        public void EnsureReady_WhenConnectionIsNotOpen_RejectsBeforeMigration()
        {
            using (var connection = new SQLiteConnection())
            {
                var gate = new SqliteConnectionCredentialStorageStartupGate();

                Assert.ThrowsExactly<InvalidOperationException>(() => gate.EnsureReady(connection));
            }
        }

        private static void WithTemporaryDatabase(Action<string> action)
        {
            var databaseFilePath = Path.Combine(Path.GetTempPath(), $"JasonQuery-Step5-{Guid.NewGuid():N}.db");

            try
            {
                SQLiteConnection.CreateFile(databaseFilePath);

                using (var connection = OpenConnection(databaseFilePath))
                using
                (
                    var command = new SQLiteCommand
                    (
                        "CREATE TABLE [DBInfo] " +
                        "([PID] INTEGER NOT NULL PRIMARY KEY, " +
                        "[DomainUser] TEXT NULL, " +
                        "[Password] TEXT NULL)",
                        connection
                    )
                )
                {
                    command.ExecuteNonQuery();
                }

                action(databaseFilePath);
            }
            finally
            {
                SQLiteConnection.ClearAllPools();

                if (File.Exists(databaseFilePath))
                {
                    File.Delete(databaseFilePath);
                }
            }
        }

        private static void AssertMigrationCompatibleMetadataSchema(string databaseFilePath)
        {
            using (var connection = OpenConnection(databaseFilePath))
            {
                using
                (
                    var command = new SQLiteCommand
                    (
                        "PRAGMA table_info('JasonQuerySecurityMetadata')",
                        connection
                    )
                )
                using (var reader = command.ExecuteReader())
                {
                    var rowCount = 0;

                    while (reader.Read())
                    {
                        var name = Convert.ToString(reader["name"]);
                        var declaredType = Convert.ToString(reader["type"]);
                        var primaryKeyOrdinal = Convert.ToInt32(reader["pk"]);

                        if (rowCount == 0)
                        {
                            Assert.AreEqual("PID", name);
                            Assert.AreEqual("INTEGER", declaredType.ToUpperInvariant());
                            Assert.AreEqual(1, primaryKeyOrdinal);
                        }

                        rowCount++;
                    }

                    Assert.AreEqual(3, rowCount);
                }

                using
                (
                    var command = new SQLiteCommand
                    (
                        "PRAGMA index_list('JasonQuerySecurityMetadata')",
                        connection
                    )
                )
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(SqliteConnectionCredentialStorageVersionStore.MetadataUniqueIndexName, Convert.ToString(reader["name"]));
                    Assert.AreEqual(1, Convert.ToInt32(reader["unique"]));
                    Assert.AreEqual("c", Convert.ToString(reader["origin"]));
                    Assert.IsFalse(reader.Read());
                }
            }
        }

        private static SQLiteConnection OpenConnection(string databaseFilePath)
        {
            var connection = new SQLiteConnection
            {
                ConnectionString = $"Data Source={databaseFilePath};Version=3;New=False;"
            };

            connection.Open();
            return connection;
        }

        private static void InsertDbInfo(string databaseFilePath, long pid, string domainUser, string password)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using
            (
                var command = new SQLiteCommand
                (
                    "INSERT INTO [DBInfo] ([PID], [DomainUser], [Password]) " +
                    "VALUES (@Pid, @DomainUser, @Password)",
                    connection
                )
            )
            {
                command.Parameters.AddWithValue("@Pid", pid);
                command.Parameters.AddWithValue("@DomainUser", (object)domainUser ?? DBNull.Value);
                command.Parameters.AddWithValue("@Password", (object)password ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        private static string ReadPassword(string databaseFilePath, long pid)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using (var command = new SQLiteCommand("SELECT [Password] FROM [DBInfo] WHERE [PID] = @Pid", connection))
            {
                command.Parameters.AddWithValue("@Pid", pid);

                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private static int? ReadPersistedVersion(string databaseFilePath)
        {
            using (var connection = OpenConnection(databaseFilePath))
            {
                var store = new SqliteConnectionCredentialStorageVersionStore();

                return store.ReadPersistedVersion(connection, null);
            }
        }

        private static string ReadRawPersistedVersion(string databaseFilePath)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using
            (
                var command = new SQLiteCommand
                (
                    "SELECT [MetadataValue] FROM [JasonQuerySecurityMetadata] " +
                    "WHERE [MetadataKey] = @MetadataKey",
                    connection
                )
            )
            {
                command.Parameters.AddWithValue("@MetadataKey", SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey);

                var value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private static void SeedPersistedVersion(string databaseFilePath, string persistedVersion)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using
            (
                var command = new SQLiteCommand
                (
                    "CREATE TABLE IF NOT EXISTS [JasonQuerySecurityMetadata] " +
                    "([MetadataKey] TEXT NOT NULL PRIMARY KEY, [MetadataValue] TEXT NOT NULL); " +
                    "INSERT OR REPLACE INTO [JasonQuerySecurityMetadata] ([MetadataKey], [MetadataValue]) " +
                    "VALUES (@MetadataKey, @MetadataValue);",
                    connection
                )
            )
            {
                command.Parameters.AddWithValue("@MetadataKey", SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey);
                command.Parameters.AddWithValue("@MetadataValue", persistedVersion);
                command.ExecuteNonQuery();
            }
        }
    }
}
