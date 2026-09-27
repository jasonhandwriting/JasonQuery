using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class DirectStorageV2FreshInstallStartupGuardTests
    {
        [TestMethod]
        public void Startup_RecoversInterruptedFreshCandidateBeforePersistedRuntimeRouting()
        {
            var source = ReadMainFormDatabaseSecuritySource();

            var recoveryCall = source.IndexOf
            (
                "RecoverInterruptedFreshDatabaseInitializationIfNeeded",
                StringComparison.Ordinal
            );

            var routeResolution = source.IndexOf
            (
                "var storageRoute = JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadataStore);",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, recoveryCall);
            Assert.IsGreaterThan(recoveryCall, routeResolution);
        }

        [TestMethod]
        public void FreshRecovery_RoutesProviderExclusivelyFromPersistedStorageFormatWhenMetadataExists()
        {
            var source = ExtractMethod
            (
                ReadMainFormDatabaseSecuritySource(),
                "private static IJasonQueryDbFreshInstallDatabase CreateFreshInstallRecoveryDatabase"
            );

            Assert.Contains("JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadata.StorageFormatVersion)", source);
            Assert.Contains("case JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite:", source);
            Assert.Contains("return new SqliteDatabaseSecurityFreshInstallDatabase();", source);
            Assert.Contains("case JasonQueryDbStorageRuntimeKind.ModernSqlCipher:", source);
            Assert.Contains("return new ModernSqlCipherDatabaseSecurityFreshInstallDatabase();", source);
            Assert.DoesNotContain("CanOpen(", source);
            Assert.DoesNotContain("CanOpenWithoutPassword(", source);
        }

        [TestMethod]
        public void FreshRecovery_NoMetadataDefaultsToDirectModernTargetWithoutProviderProbing()
        {
            var source = ExtractMethod
            (
                ReadMainFormDatabaseSecuritySource(),
                "private static IJasonQueryDbFreshInstallDatabase CreateFreshInstallRecoveryDatabase"
            );

            Assert.Contains("if (!metadataStore.Exists)", source);
            Assert.Contains("return new ModernSqlCipherDatabaseSecurityFreshInstallDatabase();", source);
            Assert.DoesNotContain("File.ReadAllBytes", source);
            Assert.DoesNotContain("SQLite format 3", source);
        }

        [TestMethod]
        public void CleanFreshInstall_CreatesStorageV2DirectlyAndFinalizesThroughModernRuntimeGate()
        {
            var source = ExtractMethod
            (
                ReadMainFormDatabaseSecuritySource(),
                "private static void InitializeFreshStorageV2Database"
            );

            Assert.Contains("new ModernSqlCipherDatabaseSecurityFreshInstallDatabase()", source);
            Assert.Contains("freshInitializer.InitializeWindowsCurrentUser(databaseFilePath, templateStream)", source);
            Assert.Contains("ConfigureModernStorageV2Runtime", source);
            Assert.DoesNotContain("new SqliteDatabaseSecurityFreshInstallDatabase()", source);
            Assert.DoesNotContain("CompleteLegacyStorageV1Startup", source);
            Assert.DoesNotContain("MigrateLegacyStorageToModern", source);
        }

        [TestMethod]
        public void CleanFreshInstall_PreservesLegacyMigrationRecoveryPriorityAndDefersLegacyProviderConstruction()
        {
            var source = ReadMainFormDatabaseSecuritySource();

            var backupPathCheck = source.IndexOf
            (
                "var legacyMigrationBackupFilePath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databaseFilePath);",
                StringComparison.Ordinal
            );

            var interruptedMigrationCondition = source.IndexOf
            (
                "if (!metadataStore.Exists && File.Exists(legacyMigrationBackupFilePath))",
                StringComparison.Ordinal
            );

            var interruptedMigratorCreation = source.IndexOf
            (
                "var interruptedMigrator = CreateLegacyDatabaseSecurityMigrator",
                interruptedMigrationCondition,
                StringComparison.Ordinal
            );

            var directFreshCondition = source.IndexOf
            (
                "if (!File.Exists(databaseFilePath) && !metadataStore.Exists)",
                StringComparison.Ordinal
            );

            var legacyRuntimeReset = source.IndexOf
            (
                "JasonQueryRepository.ResetRuntimeToLegacy();",
                directFreshCondition,
                StringComparison.Ordinal
            );

            var normalLegacyMigratorCreation = source.IndexOf
            (
                "var migrator = CreateLegacyDatabaseSecurityMigrator",
                legacyRuntimeReset,
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, backupPathCheck);
            Assert.IsGreaterThan(backupPathCheck, interruptedMigrationCondition);
            Assert.IsGreaterThan(interruptedMigrationCondition, interruptedMigratorCreation);
            Assert.IsGreaterThan(interruptedMigratorCreation, directFreshCondition);
            Assert.IsGreaterThan(directFreshCondition, legacyRuntimeReset);
            Assert.IsGreaterThan(legacyRuntimeReset, normalLegacyMigratorCreation);
            Assert.DoesNotContain("new SqliteDatabaseSecurityMigrationDatabase()", source.Substring(interruptedMigratorCreation, directFreshCondition - interruptedMigratorCreation));
        }

        [TestMethod]
        public void R4F2E_DoesNotCutOverGlobalCurrentVersionOrLeakSQLitePCLIntoMainStartup()
        {
            var source = ReadMainFormDatabaseSecuritySource();

            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.LegacyVersion)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.CurrentVersion)));
            Assert.DoesNotContain("using SQLitePCL;", source);
            Assert.DoesNotContain("raw.sqlite3_", source);
            Assert.DoesNotContain("DatabaseKeyDisasterRecovery", source);
        }

        private static string ReadMainFormDatabaseSecuritySource()
        {
            return ReadRepositorySource
            (
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );
        }

        private static string ExtractMethod(string source, string methodMarker)
        {
            var start = source.IndexOf(methodMarker, StringComparison.Ordinal);

            Assert.IsGreaterThanOrEqualTo(0, start, "Method marker was not found: " + methodMarker);

            var openBrace = source.IndexOf('{', start);

            Assert.IsGreaterThanOrEqualTo(0, openBrace, "Method opening brace was not found: " + methodMarker);

            var depth = 0;

            for (var index = openBrace; index < source.Length; index++)
            {
                switch (source[index])
                {
                    case '{':
                        {
                            depth++;
                            break;
                        }
                    case '}':
                        {
                            depth--;

                            if (depth == 0)
                            {
                                return source.Substring(start, index - start + 1);
                            }

                            break;
                        }
                }
            }

            Assert.Fail("Method closing brace was not found: " + methodMarker);
            return string.Empty;
        }

        private static string ReadRepositorySource(params string[] pathParts)
        {
            var path = FindRepositoryRoot();

            foreach (var pathPart in pathParts)
            {
                path = Path.Combine(path, pathPart);
            }

            Assert.IsTrue(File.Exists(path), "Required source file was not found: " + path);
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

            Assert.Fail("Could not locate the JasonQuery repository root for R4F2E tests.");
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
                if (File.Exists(Path.Combine(directory.FullName, "JasonQuery", "JasonQuery.csproj")) && Directory.Exists(Path.Combine(directory.FullName, "Tests", "JasonQuery.Tests")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
