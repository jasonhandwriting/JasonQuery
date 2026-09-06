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
            Assert.AreEqual
            (
                "JasonQuerySecurityMetadata",
                SqliteConnectionCredentialStorageVersionStore.MetadataTableName
            );

            Assert.AreEqual
            (
                "ConnectionCredentialStorageVersion",
                SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey
            );
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
            Assert.ThrowsException<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue(string.Empty)
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenNonNumeric_ThrowsInvalidDataException()
        {
            Assert.ThrowsException<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("not-a-version")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenWhitespacePadded_ThrowsInvalidDataException()
        {
            Assert.ThrowsException<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue(" 2 ")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenLeadingZero_ThrowsInvalidDataException()
        {
            Assert.ThrowsException<InvalidDataException>
            (
                () => SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue("02")
            );
        }

        [TestMethod]
        public void ParsePersistedVersionValue_WhenUnknownVersion_FailsClosed()
        {
            Assert.ThrowsException<NotSupportedException>
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
                        Assert.ThrowsException<NotSupportedException>
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
