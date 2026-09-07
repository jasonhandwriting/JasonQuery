using JasonQuery.Database.Internal.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Repositories
{
    [TestClass]
    [DoNotParallelize]
    public class JasonQueryRepositoryParameterizedNonQueryTests
    {
        [TestMethod]
        public void ExecNonQuery_WithCredentialParameter_PreservesExactLogicalPassword()
        {
            WithTemporaryRepositoryDatabase
            (
                databaseFilePath =>
                {
                    const string logicalPassword = "  O'Brien-資料庫\t2026!  ";

                    JasonQueryRepository.ExecNonQuery
                    (
                        "INSERT INTO [CredentialWriteTest] ([Id], [Password]) VALUES (1, @Password)",
                        new[]
                        {
                            new SQLiteParameter("@Password", logicalPassword)
                        },
                        false
                    );

                    Assert.AreEqual(logicalPassword, ReadPassword(databaseFilePath, 1));
                }
            );
        }

        [TestMethod]
        public void ExecNonQuery_WithCredentialParameter_PersistsCanonicalEmptyString()
        {
            WithTemporaryRepositoryDatabase
            (
                databaseFilePath =>
                {
                    JasonQueryRepository.ExecNonQuery
                    (
                        "INSERT INTO [CredentialWriteTest] ([Id], [Password]) VALUES (2, @Password)",
                        new[]
                        {
                            new SQLiteParameter("@Password", string.Empty)
                        },
                        false
                    );

                    Assert.AreEqual(string.Empty, ReadPassword(databaseFilePath, 2));
                }
            );
        }

        [TestMethod]
        public void ExecNonQuery_WithMultipleCredentialParameters_PreservesEachValue()
        {
            WithTemporaryRepositoryDatabase
            (
                databaseFilePath =>
                {
                    const string firstPassword = "first-O'Brien-value";
                    const string secondPassword = "  second-資料庫\tvalue  ";

                    JasonQueryRepository.ExecNonQuery
                    (
                        "INSERT INTO [CredentialWriteTest] ([Id], [Password]) VALUES (10, @Password0); " +
                        "INSERT INTO [CredentialWriteTest] ([Id], [Password]) VALUES (11, @Password1);",
                        new[]
                        {
                            new SQLiteParameter("@Password0", firstPassword),
                            new SQLiteParameter("@Password1", secondPassword)
                        },
                        false
                    );

                    Assert.AreEqual(firstPassword, ReadPassword(databaseFilePath, 10));
                    Assert.AreEqual(secondPassword, ReadPassword(databaseFilePath, 11));
                }
            );
        }

        private static void WithTemporaryRepositoryDatabase(Action<string> action)
        {
            var originalConnectionString = JasonQueryRepository.DbConnectionString;
            var originalConnectionPassword = JasonQueryRepository.DbConnectionPassword;
            var databaseFilePath = Path.Combine(Path.GetTempPath(), $"JasonQuery-Step5-Repo-{Guid.NewGuid():N}.db");
            const string databasePassword = "Step5RepositoryTestPassword!2026";

            try
            {
                SQLiteConnection.CreateFile(databaseFilePath);

                using
                (
                    var connection = new SQLiteConnection
                    {
                        ConnectionString = $"Data Source={databaseFilePath};Version=3;New=False;"
                    }
                )
                {
                    connection.Open();

                    using
                    (
                        var command = new SQLiteCommand
                        (
                            "CREATE TABLE [CredentialWriteTest] " +
                            "([Id] INTEGER NOT NULL PRIMARY KEY, [Password] TEXT NOT NULL)",
                            connection
                        )
                    )
                    {
                        command.ExecuteNonQuery();
                    }

                    connection.ChangePassword(databasePassword);
                }

                SQLiteConnection.ClearAllPools();

                JasonQueryRepository.DbConnectionString = $"Data Source={databaseFilePath};Version=3;New=False;Compress=True;";
                JasonQueryRepository.DbConnectionPassword = databasePassword;

                action(databaseFilePath);
            }
            finally
            {
                JasonQueryRepository.DbConnectionString = originalConnectionString;
                JasonQueryRepository.DbConnectionPassword = originalConnectionPassword;

                SQLiteConnection.ClearAllPools();

                if (File.Exists(databaseFilePath))
                {
                    File.Delete(databaseFilePath);
                }
            }
        }

        private static string ReadPassword(string databaseFilePath, long id)
        {
            using
            (
                var connection = new SQLiteConnection
                {
                    ConnectionString = $"Data Source={databaseFilePath};Version=3;New=False;"
                }
            )
            {
                connection.SetPassword(JasonQueryRepository.DbConnectionPassword);
                connection.Open();

                using (var command = new SQLiteCommand("SELECT [Password] FROM [CredentialWriteTest] WHERE [Id] = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    var value = command.ExecuteScalar();
                    return value == null || value == DBNull.Value ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
