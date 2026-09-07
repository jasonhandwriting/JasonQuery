using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Core.Security.Legacy;
using JasonQuery.Database.Internal.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Security
{
    [TestClass]
    public class SqliteLegacyConnectionCredentialMigratorTests
    {
        [TestMethod]
        public void MigrateIfRequired_WhenMarkerMissing_MigratesCredentialAndPersistsCurrentVersion()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\LegacyUserOne";
                    var logicalValue = "migration-value-one";
                    var protectedValue = LegacyConnectionCredentialSecurity.Protect(logicalValue, domainUser);

                    InsertDbInfo
                    (
                        databaseFilePath,
                        1,
                        domainUser,
                        protectedValue
                    );

                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        logicalValue,
                        ReadPassword(databaseFilePath, 1)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenExplicitLegacyMarker_MigratesAndUpdatesVersion()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\LegacyUserTwo";
                    var logicalValue = "migration-value-two";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        2,
                        domainUser,
                        LegacyConnectionCredentialSecurity.Protect
                        (
                            logicalValue,
                            domainUser
                        )
                    );

                    SeedPersistedVersion
                    (
                        databaseFilePath,
                        ConnectionCredentialStorageContract.LegacyVersion
                            .ToString(CultureInfo.InvariantCulture)
                    );

                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        logicalValue,
                        ReadPassword(databaseFilePath, 2)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenAlreadyCurrent_IsNoOpWithoutContentHeuristics()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\CurrentMarkerUser";
                    var legacyLookingValue = LegacyConnectionCredentialSecurity.Protect("must-not-be-decoded", domainUser);

                    InsertDbInfo
                    (
                        databaseFilePath,
                        3,
                        domainUser,
                        legacyLookingValue
                    );

                    SeedPersistedVersion
                    (
                        databaseFilePath,
                        ConnectionCredentialStorageContract.CurrentVersion.ToString(CultureInfo.InvariantCulture)
                    );

                    Assert.IsFalse
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        legacyLookingValue,
                        ReadPassword(databaseFilePath, 3)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_MigratesAllRowsUsingEachStoredDomainUser()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var firstDomainUser = @"DOMAIN_A\LegacyUserA";
                    var secondDomainUser = @"DOMAIN_B\LegacyUserB";
                    var firstLogicalValue = "first-logical-value";
                    var secondLogicalValue = "second-logical-value";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        10,
                        firstDomainUser,
                        LegacyConnectionCredentialSecurity.Protect
                        (
                            firstLogicalValue,
                            firstDomainUser
                        )
                    );

                    InsertDbInfo
                    (
                        databaseFilePath,
                        11,
                        secondDomainUser,
                        LegacyConnectionCredentialSecurity.Protect
                        (
                            secondLogicalValue,
                            secondDomainUser
                        )
                    );

                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        firstLogicalValue,
                        ReadPassword(databaseFilePath, 10)
                    );

                    Assert.AreEqual
                    (
                        secondLogicalValue,
                        ReadPassword(databaseFilePath, 11)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_PreservesExactLogicalPasswordCharacters()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\ExactValueUser";
                    var logicalValue = "  O'Brien-資料庫\t2026!  ";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        20,
                        domainUser,
                        LegacyConnectionCredentialSecurity.Protect
                        (
                            logicalValue,
                            domainUser
                        )
                    );

                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        logicalValue,
                        ReadPassword(databaseFilePath, 20)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenPasswordIsEmptyOrNull_CanonicalizesToEmptyAndMarksCurrent()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    InsertDbInfo
                    (
                        databaseFilePath,
                        30,
                        @"TESTDOMAIN\EmptyUser",
                        string.Empty
                    );

                    InsertDbInfo
                    (
                        databaseFilePath,
                        31,
                        @"TESTDOMAIN\NullUser",
                        null
                    );

                    var emptyPasswordDomainUser = @"TESTDOMAIN\ProtectedEmptyUser";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        32,
                        emptyPasswordDomainUser,
                        LegacyConnectionCredentialSecurity.Protect
                        (
                            string.Empty,
                            emptyPasswordDomainUser
                        )
                    );

                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        string.Empty,
                        ReadPassword(databaseFilePath, 30)
                    );

                    Assert.AreEqual
                    (
                        string.Empty,
                        ReadPassword(databaseFilePath, 31)
                    );

                    Assert.AreEqual
                    (
                        string.Empty,
                        ReadPassword(databaseFilePath, 32)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenNoDbInfoRows_MarksDatabaseCurrent()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenLaterCredentialIsCorrupt_RollsBackEarlierUpdatesAndMarker()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\RollbackUser";
                    var firstProtectedValue = LegacyConnectionCredentialSecurity.Protect("first-value-must-roll-back", domainUser);
                    var corruptValue = "not-valid-legacy-ciphertext";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        40,
                        domainUser,
                        firstProtectedValue
                    );

                    InsertDbInfo
                    (
                        databaseFilePath,
                        41,
                        domainUser,
                        corruptValue
                    );

                    Assert.ThrowsException<InvalidDataException>
                    (
                        () => ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        firstProtectedValue,
                        ReadPassword(databaseFilePath, 40)
                    );

                    Assert.AreEqual
                    (
                        corruptValue,
                        ReadPassword(databaseFilePath, 41)
                    );

                    Assert.IsNull
                    (
                        ReadPersistedVersion(databaseFilePath)
                    );
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenSavedCredentialHasNoDomainUser_FailsClosedAndRollsBack()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var protectedValue = LegacyConnectionCredentialSecurity.Protect
                    (
                        "missing-domain-user-value",
                        @"TESTDOMAIN\OriginalUser"
                    );

                    InsertDbInfo
                    (
                        databaseFilePath,
                        50,
                        string.Empty,
                        protectedValue
                    );

                    Assert.ThrowsException<InvalidDataException>
                    (
                        () => ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        protectedValue,
                        ReadPassword(databaseFilePath, 50)
                    );

                    Assert.IsNull
                    (
                        ReadPersistedVersion(databaseFilePath)
                    );
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenVersionIsUnknown_FailsClosedWithoutChangingCredential()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\UnknownVersionUser";

                    var protectedValue = LegacyConnectionCredentialSecurity.Protect
                    (
                        "unknown-version-value",
                        domainUser
                    );

                    InsertDbInfo
                    (
                        databaseFilePath,
                        60,
                        domainUser,
                        protectedValue
                    );

                    SeedPersistedVersion
                    (
                        databaseFilePath,
                        "999"
                    );

                    Assert.ThrowsException<NotSupportedException>
                    (
                        () => ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        protectedValue,
                        ReadPassword(databaseFilePath, 60)
                    );

                    Assert.AreEqual
                    (
                        "999",
                        ReadRawPersistedVersionValue(databaseFilePath)
                    );
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_SecondCallIsNoOpAndLeavesLogicalValueUnchanged()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var domainUser = @"TESTDOMAIN\IdempotentUser";
                    var logicalValue = "idempotent-logical-value";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        70,
                        domainUser,
                        LegacyConnectionCredentialSecurity.Protect
                        (
                            logicalValue,
                            domainUser
                        )
                    );

                    Assert.IsTrue
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.IsFalse
                    (
                        ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        logicalValue,
                        ReadPassword(databaseFilePath, 70)
                    );

                    AssertCurrentVersion(databaseFilePath);
                }
            );
        }

        [TestMethod]
        public void MigrateIfRequired_WhenExplicitLegacyMigrationFails_PreservesLegacyMarker()
        {
            WithTemporaryDatabase
            (
                databaseFilePath =>
                {
                    var corruptValue = "not-valid-legacy-ciphertext";

                    InsertDbInfo
                    (
                        databaseFilePath,
                        80,
                        @"TESTDOMAIN\ExplicitLegacyUser",
                        corruptValue
                    );

                    SeedPersistedVersion
                    (
                        databaseFilePath,
                        ConnectionCredentialStorageContract.LegacyVersion.ToString(CultureInfo.InvariantCulture)
                    );

                    Assert.ThrowsException<InvalidDataException>
                    (
                        () => ExecuteMigration(databaseFilePath)
                    );

                    Assert.AreEqual
                    (
                        corruptValue,
                        ReadPassword(databaseFilePath, 80)
                    );

                    Assert.AreEqual
                    (
                        ConnectionCredentialStorageContract.LegacyVersion,
                        ReadPersistedVersion(databaseFilePath)
                    );
                }
            );
        }

        private static bool ExecuteMigration(string databaseFilePath)
        {
            using (var connection = OpenConnection(databaseFilePath))
            {
                var migrator = new SqliteLegacyConnectionCredentialMigrator();

                return migrator.MigrateIfRequired(connection);
            }
        }

        private static void InsertDbInfo(string databaseFilePath, long pid, string domainUser, string password)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using
            (
                var command = new SQLiteCommand
                (
                    "INSERT INTO [DBInfo] " +
                    "([PID], [DomainUser], [Password]) " +
                    "VALUES (@Pid, @DomainUser, @Password)",
                    connection
                )
            )
            {
                command.Parameters.AddWithValue
                (
                    "@Pid",
                    pid
                );

                command.Parameters.AddWithValue
                (
                    "@DomainUser",
                    (object)domainUser ?? DBNull.Value
                );

                command.Parameters.AddWithValue
                (
                    "@Password",
                    (object)password ?? DBNull.Value
                );

                command.ExecuteNonQuery();
            }
        }

        private static string ReadPassword(string databaseFilePath, long pid)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using
            (
                var command = new SQLiteCommand
                (
                    "SELECT [Password] " +
                    "FROM [DBInfo] " +
                    "WHERE [PID] = @Pid",
                    connection
                )
            )
            {
                command.Parameters.AddWithValue
                (
                    "@Pid",
                    pid
                );

                var value = command.ExecuteScalar();

                if (value == null || value == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToString
                (
                    value,
                    CultureInfo.InvariantCulture
                );
            }
        }

        private static int? ReadPersistedVersion(string databaseFilePath)
        {
            using (var connection = OpenConnection(databaseFilePath))
            {
                var store = new SqliteConnectionCredentialStorageVersionStore();

                return store.ReadPersistedVersion
                (
                    connection,
                    null
                );
            }
        }

        private static string ReadRawPersistedVersionValue(string databaseFilePath)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using
            (
                var command = new SQLiteCommand
                (
                    "SELECT [MetadataValue] " +
                    "FROM [JasonQuerySecurityMetadata] " +
                    "WHERE [MetadataKey] = @MetadataKey",
                    connection
                )
            )
            {
                command.Parameters.AddWithValue
                (
                    "@MetadataKey",
                    SqliteConnectionCredentialStorageVersionStore.VersionMetadataKey
                );

                var value = command.ExecuteScalar();

                if (value == null || value == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToString
                (
                    value,
                    CultureInfo.InvariantCulture
                );
            }
        }

        private static void AssertCurrentVersion(string databaseFilePath)
        {
            Assert.AreEqual
            (
                ConnectionCredentialStorageContract.CurrentVersion,
                ReadPersistedVersion(databaseFilePath)
            );
        }

        private static void SeedPersistedVersion(string databaseFilePath, string persistedVersion)
        {
            using (var connection = OpenConnection(databaseFilePath))
            using (var transaction = connection.BeginTransaction())
            {
                using
                (
                    var command = new SQLiteCommand
                    (
                        "CREATE TABLE IF NOT EXISTS " +
                        "[JasonQuerySecurityMetadata] " +
                        "(" +
                        "[MetadataKey] TEXT NOT NULL PRIMARY KEY, " +
                        "[MetadataValue] TEXT NOT NULL" +
                        ")",
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
                        "([MetadataKey], [MetadataValue]) " +
                        "VALUES (@MetadataKey, @MetadataValue)",
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
                        persistedVersion
                    );

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private static SQLiteConnection OpenConnection(string databaseFilePath)
        {
            var connection =new SQLiteConnection
            {
                ConnectionString =
                    $"Data Source={databaseFilePath};" +
                    "Version=3;New=False;"
            };

            connection.Open();
            return connection;
        }

        private static void WithTemporaryDatabase(Action<string> testAction)
        {
            var databaseFilePath =Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-LegacyCredentialMigration-" +
                Guid.NewGuid().ToString("N") +
                ".db"
            );

            try
            {
                SQLiteConnection.CreateFile(databaseFilePath);

                using (var connection = OpenConnection(databaseFilePath))
                using
                (
                    var command = new SQLiteCommand
                    (
                        "CREATE TABLE [DBInfo] " +
                        "(" +
                        "[PID] INTEGER NOT NULL PRIMARY KEY, " +
                        "[DomainUser] TEXT NULL, " +
                        "[Password] TEXT NULL" +
                        ")",
                        connection
                    )
                )
                {
                    command.ExecuteNonQuery();
                }

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
