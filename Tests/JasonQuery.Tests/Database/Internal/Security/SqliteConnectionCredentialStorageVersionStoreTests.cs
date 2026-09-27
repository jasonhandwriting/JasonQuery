using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Database.Internal.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Security
{
    [TestClass]
    public class SqliteConnectionCredentialStorageVersionStoreTests
    {
        [TestMethod]
        public void MetadataIdentifiers_AreStableDatabaseWideNames()
        {
            Assert.AreEqual ( "JasonQuerySecurityMetadata", JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(SqliteConnectionCredentialStorageVersionStore), nameof(SqliteConnectionCredentialStorageVersionStore.MetadataTableName)) );

            Assert.AreEqual ( "ConnectionCredentialStorageVersion", JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(SqliteConnectionCredentialStorageVersionStore), nameof(SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey)) );

            Assert.AreEqual ( "UX_JasonQuerySecurityMetadata_MetadataKey", JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(SqliteConnectionCredentialStorageVersionStore), nameof(SqliteConnectionCredentialStorageVersionStore.MetadataUniqueIndexName)) );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenLegacy_ReturnsLegacyVersion()
        {
            Assert.AreEqual
            (
                ConnectionCredentialStorageContract.LegacyVersion,
                SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("1")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenCurrent_ReturnsCurrentVersion()
        {
            Assert.AreEqual
            (
                ConnectionCredentialStorageContract.CurrentVersion,
                SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("2")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenEmpty_ThrowsInvalidDataException()
        {
            Assert.ThrowsExactly<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue(string.Empty)
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenNonNumeric_ThrowsInvalidDataException()
        {
            Assert.ThrowsExactly<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("not-a-version")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenWhitespacePadded_ThrowsInvalidDataException()
        {
            Assert.ThrowsExactly<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue(" 2 ")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenLeadingZero_ThrowsInvalidDataException()
        {
            Assert.ThrowsExactly<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("02")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenUnknownVersion_FailsClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("999")
            );
        }

        [TestMethod]
        public void ReadPersistedVersion_WhenMetadataTableIsMissing_ReturnsNull()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var store = new SqliteConnectionCredentialStorageVersionStore();

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        Assert.IsNull
                        (
                            store.ReadPersistedVersion
                            (
                                connection,
                                null
                            )
                        );
                    }
                }
            );
        }

        [TestMethod]
        public void WriteCurrentVersion_PersistsAcrossConnectionReopen()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var store = new SqliteConnectionCredentialStorageVersionStore();

                    using (var connection = OpenConnection(databaseFilePath))
                    using (var transaction = connection.BeginTransaction())
                    {
                        store.WriteCurrentVersion
                        (
                            connection,
                            transaction
                        );

                        transaction.Commit();
                    }

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        Assert.AreEqual
                        (
                            ConnectionCredentialStorageContract.CurrentVersion,
                            store.ReadPersistedVersion
                            (
                                connection,
                                null
                            )
                        );
                    }

                    AssertMigrationCompatibleMetadataSchema(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void WriteCurrentVersion_WhenTransactionRollsBack_DoesNotPersistMarker()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var store = new SqliteConnectionCredentialStorageVersionStore();

                    using (var connection = OpenConnection(databaseFilePath))
                    using (var transaction = connection.BeginTransaction())
                    {
                        store.WriteCurrentVersion
                        (
                            connection,
                            transaction
                        );

                        transaction.Rollback();
                    }

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        Assert.IsNull
                        (
                            store.ReadPersistedVersion
                            (
                                connection,
                                null
                            )
                        );
                    }
                }
            );
        }

        [TestMethod]
        public void WriteCurrentVersion_WhenLegacyMarkerExists_UpdatesToCurrentVersion()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    SeedPersistedVersion
                    (
                        databaseFilePath,
                        ConnectionCredentialStorageContract.LegacyVersion.ToString()
                    );

                    var store = new SqliteConnectionCredentialStorageVersionStore();

                    using (var connection = OpenConnection(databaseFilePath))
                    using (var transaction = connection.BeginTransaction())
                    {
                        Assert.AreEqual
                        (
                            ConnectionCredentialStorageContract.LegacyVersion,
                            store.ReadPersistedVersion
                            (
                                connection,
                                transaction
                            )
                        );

                        store.WriteCurrentVersion
                        (
                            connection,
                            transaction
                        );

                        transaction.Commit();
                    }

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        Assert.AreEqual
                        (
                            ConnectionCredentialStorageContract.CurrentVersion,
                            store.ReadPersistedVersion
                            (
                                connection,
                                null
                            )
                        );
                    }

                    AssertMigrationCompatibleMetadataSchema(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void WriteCurrentVersion_WhenInlineUniqueSchemaExists_RebuildsToExplicitUniqueIndex()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    using (var connection = OpenConnection(databaseFilePath))
                    using (var transaction = connection.BeginTransaction())
                    {
                        using
                        (
                            var command = new SQLiteCommand
                            (
                                "CREATE TABLE [JasonQuerySecurityMetadata] " +
                                "([PID] INTEGER PRIMARY KEY, [MetadataKey] TEXT NOT NULL UNIQUE, " +
                                "[MetadataValue] TEXT NOT NULL)",
                                connection,
                                transaction
                            )
                        )
                        {
                            command.ExecuteNonQuery();
                        }

                        using
                        (
                            var command = new SQLiteCommand
                            (
                                "INSERT INTO [JasonQuerySecurityMetadata] " +
                                "([MetadataKey], [MetadataValue]) VALUES (@MetadataKey, @MetadataValue)",
                                connection,
                                transaction
                            )
                        )
                        {
                            command.Parameters.AddWithValue
                            (
                                "@MetadataKey",
                                SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey
                            );

                            command.Parameters.AddWithValue
                            (
                                "@MetadataValue",
                                ConnectionCredentialStorageContract.CurrentVersion.ToString()
                            );

                            command.ExecuteNonQuery();
                        }

                        var store = new SqliteConnectionCredentialStorageVersionStore();

                        store.WriteCurrentVersion
                        (
                            connection,
                            transaction
                        );

                        transaction.Commit();
                    }

                    AssertMigrationCompatibleMetadataSchema(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void WriteCurrentVersion_WhenMetadataSchemaIsUnexpected_FailsClosed()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    using (var connection = OpenConnection(databaseFilePath))
                    using
                    (
                        var command = new SQLiteCommand
                        (
                            "CREATE TABLE [JasonQuerySecurityMetadata] " +
                            "([MetadataKey] TEXT NOT NULL, [MetadataValue] TEXT NOT NULL, [Extra] TEXT)",
                            connection
                        )
                    )
                    {
                        command.ExecuteNonQuery();
                    }

                    var store = new SqliteConnectionCredentialStorageVersionStore();

                    using (var connection = OpenConnection(databaseFilePath))
                    using (var transaction = connection.BeginTransaction())
                    {
                        Assert.ThrowsExactly<InvalidDataException>
                        (
                            () => store.WriteCurrentVersion
                            (
                                connection,
                                transaction
                            )
                        );

                        transaction.Rollback();
                    }
                }
            );
        }

        [TestMethod]
        public void ReadPersistedVersion_WhenPersistedVersionIsUnknown_FailsClosed()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    SeedPersistedVersion
                    (
                        databaseFilePath,
                        "999"
                    );

                    var store = new SqliteConnectionCredentialStorageVersionStore();

                    using (var connection = OpenConnection(databaseFilePath))
                    {
                        Assert.ThrowsExactly<NotSupportedException>
                        (
                            () => store.ReadPersistedVersion
                            (
                                connection,
                                null
                            )
                        );
                    }
                }
            );
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
                    var rows = 0;

                    while (reader.Read())
                    {
                        var name = Convert.ToString(reader["name"]);
                        var declaredType = Convert.ToString(reader["type"]);
                        var notNull = Convert.ToInt32(reader["notnull"]) != 0;
                        var primaryKeyOrdinal = Convert.ToInt32(reader["pk"]);

                        switch (rows)
                        {
                            case 0:
                                {
                                    Assert.AreEqual("PID", name);
                                    Assert.AreEqual("INTEGER", declaredType.ToUpperInvariant());
                                    Assert.AreEqual(1, primaryKeyOrdinal);
                                    break;
                                }
                            case 1:
                                {
                                    Assert.AreEqual("MetadataKey", name);
                                    Assert.AreEqual("TEXT", declaredType.ToUpperInvariant());
                                    Assert.IsTrue(notNull);
                                    Assert.AreEqual(0, primaryKeyOrdinal);
                                    break;
                                }
                            case 2:
                                {
                                    Assert.AreEqual("MetadataValue", name);
                                    Assert.AreEqual("TEXT", declaredType.ToUpperInvariant());
                                    Assert.IsTrue(notNull);
                                    Assert.AreEqual(0, primaryKeyOrdinal);
                                    break;
                                }
                            default:
                                {
                                    Assert.Fail("Unexpected JasonQuerySecurityMetadata column.");
                                    break;
                                }
                        }

                        rows++;
                    }

                    Assert.AreEqual(3, rows);
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
                    Assert.AreEqual(0, Convert.ToInt32(reader["partial"]));
                    Assert.IsFalse(reader.Read());
                }

                using
                (
                    var command = new SQLiteCommand
                    (
                        "PRAGMA index_info('" +
                        SqliteConnectionCredentialStorageVersionStore.MetadataUniqueIndexName +
                        "')",
                        connection
                    )
                )
                using (var reader = command.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual("MetadataKey", Convert.ToString(reader["name"]));
                    Assert.IsFalse(reader.Read());
                }
            }
        }

        private static void SeedPersistedVersion(string databaseFilePath, string persistedVersion)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = new SQLiteCommand("CREATE TABLE [JasonQuerySecurityMetadata] " + "(" + "[MetadataKey] TEXT NOT NULL PRIMARY KEY, " + "[MetadataValue] TEXT NOT NULL" + ")", connection, transaction))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand("INSERT INTO [JasonQuerySecurityMetadata] " + "([MetadataKey], [MetadataValue]) " + "VALUES (@MetadataKey, @MetadataValue)", connection, transaction))
                {
                    command.Parameters.AddWithValue
                    (
                        "@MetadataKey",
                        SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey
                    );

                    command.Parameters.AddWithValue
                    (
                        "@MetadataValue",
                        persistedVersion
                    );

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static SQLiteConnection OpenConnection(string databaseFilePath)
        {
            var connection = new SQLiteConnection
            {
                ConnectionString = $"Data Source={databaseFilePath};" + "Version=3;New=False;"
            };

            connection.Open();
            return connection;
        }

        private static void WithTemporaryDatabase(Action<string> testAction)
        {
            var databaseFilePath =Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-CredentialStorage-" +
                Guid.NewGuid().ToString("N") +
                ".db"
            );

            try
            {
                SQLiteConnection.CreateFile(databaseFilePath);

                testAction(databaseFilePath);
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
    }
}
