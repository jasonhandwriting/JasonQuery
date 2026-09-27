using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Internal.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Runtime
{
    [TestClass]
    [DoNotParallelize]
    public class JasonQueryDatabaseRuntimeBoundaryTests
    {
        [TestMethod]
        public void DatabaseParameter_PreservesExactNameAndValue()
        {
            var value = "  O'Brien-資料庫\t2026!  ";
            var parameter = new JasonQueryDatabaseParameter("@Password", value);

            Assert.AreEqual("@Password", parameter.Name);
            Assert.AreSame(value, parameter.Value);
        }

        [TestMethod]
        public void DatabaseParameter_RejectsMissingName()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDatabaseParameter(" ", "value")
            );
        }

        [TestMethod]
        public void RuntimeContract_DoesNotReferenceConcreteSQLiteProviders()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "IJasonQueryDatabaseRuntime.cs"
            );

            AssertNoConcreteProviderReference(source, "runtime contract");
        }

        [TestMethod]
        public void RepositoryFacade_DoesNotReferenceConcreteSQLiteProviderTypes()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Repositories",
                "JasonQueryRepository.cs"
            );

            AssertNoConcreteProviderReference(source, "repository facade");
            Assert.Contains("IJasonQueryDatabaseRuntime", source);
            Assert.Contains("JasonQueryDatabaseParameter[] parameters", source);
        }

        [TestMethod]
        public void LegacyRuntime_IsTheOnlyR4F2ARepositoryRuntimeThatOwnsSystemDataSQLite()
        {
            var legacySource = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "LegacySystemDataSQLiteDatabaseRuntime.cs"
            );

            Assert.Contains("using System.Data.SQLite;", legacySource);
            Assert.Contains("SQLiteConnection", legacySource);
            Assert.Contains("SQLiteCommand", legacySource);
            Assert.Contains("SQLiteDataAdapter", legacySource);
            Assert.DoesNotContain("using SQLitePCL;", legacySource);
            Assert.DoesNotContain("sqlcipher.dll", legacySource);
        }

        [TestMethod]
        public void ConnectionFormSave_UsesProviderNeutralDatabaseParameter()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionForm.Save.cs"
            );

            Assert.Contains("new JasonQueryDatabaseParameter(\"@Password\", storedPassword)", source);
            Assert.DoesNotContain("using System.Data.SQLite;", source);
            Assert.DoesNotContain("new SQLiteParameter(", source);
        }

        [TestMethod]
        public void ConnectionImport_UsesProviderNeutralDatabaseParameter()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionImportForm.cs"
            );

            Assert.Contains("new List<JasonQueryDatabaseParameter>()", source);
            Assert.Contains("new JasonQueryDatabaseParameter(passwordParameterName, storedPassword)", source);
            Assert.DoesNotContain("using System.Data.SQLite;", source);
            Assert.DoesNotContain("SQLiteParameter", source);
        }

        [TestMethod]
        public void ValidatedConnectionCompatibilitySeam_FailsClosedOutsideLegacyRuntime()
        {
            try
            {
                JasonQueryRepository.ConfigureRuntime(new NonLegacyRuntime());

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => JasonQueryRepository.OpenValidatedCurrentDatabaseConnection()
                );
            }
            finally
            {
                JasonQueryRepository.ResetRuntimeToLegacy();
            }
        }

        [TestMethod]
        public void ProductionStartup_ConfiguresModernRuntimeThroughProviderNeutralRepositoryInR4F2C()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            var validationIndex = source.IndexOf
            (
                "ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready",
                StringComparison.Ordinal
            );

            var configureIndex = source.IndexOf
            (
                "JasonQueryRepository.ConfigureRuntime(runtime);",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, validationIndex);
            Assert.IsGreaterThan(validationIndex, configureIndex);
            Assert.Contains("new ModernSqlCipherDatabaseRuntime()", source);
            Assert.DoesNotContain("using SQLitePCL;", source);
            Assert.DoesNotContain("raw.sqlite3_", source);
        }

        [TestMethod]
        public void StorageFormatCurrentVersion_RemainsLegacyInR4F2A()
        {
            Assert.AreEqual ( JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.LegacyVersion)), JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.CurrentVersion)) );
        }

        private static void AssertNoConcreteProviderReference(string source, string description)
        {
            foreach (var forbidden in new[]
            {
                "using System.Data.SQLite;",
                "SQLiteConnection",
                "SQLiteCommand",
                "SQLiteDataAdapter",
                "SQLiteParameter",
                "using SQLitePCL;",
                "SQLitePCL.",
                "sqlcipher.dll"
            })
            {
                Assert.DoesNotContain(
forbidden,
                    source, $"{description} must not reference concrete SQLite provider token '{forbidden}'."
                );
            }
        }

        private static string ReadRepositorySource(params string[] pathParts)
        {
            var root = FindRepositoryRoot();
            var path = root;

            foreach (var pathPart in pathParts)
            {
                path = Path.Combine(path, pathPart);
            }

            Assert.IsTrue(File.Exists(path), $"Required source file was not found: {path}");
            return File.ReadAllText(path);
        }

        private static string FindRepositoryRoot()
        {
            var root = FindRepositoryRootFrom(Directory.GetCurrentDirectory());

            if (!string.IsNullOrEmpty(root))
            {
                return root;
            }

            root = FindRepositoryRootFrom(AppDomain.CurrentDomain.BaseDirectory);

            if (!string.IsNullOrEmpty(root))
            {
                return root;
            }

            Assert.Fail("Could not locate the JasonQuery repository root for source guard tests.");
            return string.Empty;
        }

        private static string FindRepositoryRootFrom(string startPath)
        {
            if (string.IsNullOrWhiteSpace(startPath))
            {
                return null;
            }

            var directory = new DirectoryInfo(Path.GetFullPath(startPath));

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "JasonQuery", "JasonQuery.csproj"))
                    && Directory.Exists(Path.Combine(directory.FullName, "Tests", "JasonQuery.Tests")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private sealed class NonLegacyRuntime : IJasonQueryDatabaseRuntime
        {
            public DataTable ExecuteQuery(string connectionString, string databasePassword, string sql)
            {
                throw new NotSupportedException();
            }

            public void ExecuteNonQuery(string connectionString, string databasePassword, string sql, IReadOnlyList<JasonQueryDatabaseParameter> parameters)
            {
                throw new NotSupportedException();
            }

            public void ExecuteBatchNonQuery(string connectionString, string databasePassword, IReadOnlyList<string> sqlStatements)
            {
                throw new NotSupportedException();
            }

            public bool CanOpenDatabase(string connectionString, string databasePassword)
            {
                throw new NotSupportedException();
            }

            public void ChangePassword(string connectionString, string currentDatabasePassword, string newDatabasePassword)
            {
                throw new NotSupportedException();
            }
        }
    }
}
