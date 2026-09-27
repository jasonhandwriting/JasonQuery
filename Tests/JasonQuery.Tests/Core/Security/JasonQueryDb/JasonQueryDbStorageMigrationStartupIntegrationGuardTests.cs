using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationStartupIntegrationGuardTests
    {
        [TestMethod]
        public void MainFormDatabaseSecurity_PhysicalRecoveryRunsBeforeLegacySQLiteSecurityRuntime()
        {
            var source = ReadSource
            (
                FindRepositoryRoot(),
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            var recoveryIndex = source.IndexOf
            (
                "RecoverDatabaseStorageMigrationIfNeeded",
                StringComparison.Ordinal
            );

            var legacyRuntimeIndex = source.IndexOf
            (
                "new SqliteDatabaseSecurityMigrationDatabase()",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, recoveryIndex, "Physical storage startup recovery call was not found.");
            Assert.IsGreaterThanOrEqualTo(0, legacyRuntimeIndex, "Legacy SQLite security runtime creation was not found.");
            Assert.IsLessThan(legacyRuntimeIndex, recoveryIndex, "Physical storage recovery must run before the legacy SQLite security runtime is created.");
        }

        [TestMethod]
        public void MainFormDatabaseSecurity_RuntimeRouteRunsAfterRecoveryAndBeforeLegacySQLiteSecurityRuntime()
        {
            var source = ReadSource
            (
                FindRepositoryRoot(),
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            var recoveryIndex = source.IndexOf
            (
                "RecoverDatabaseStorageMigrationIfNeeded",
                StringComparison.Ordinal
            );

            var runtimeRouteIndex = source.IndexOf
            (
                "JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadataStore)",
                StringComparison.Ordinal
            );

            var legacyRuntimeIndex = source.IndexOf
            (
                "new SqliteDatabaseSecurityMigrationDatabase()",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, recoveryIndex);
            Assert.IsGreaterThanOrEqualTo(0, runtimeRouteIndex);
            Assert.IsGreaterThanOrEqualTo(0, legacyRuntimeIndex);
            Assert.IsLessThan(runtimeRouteIndex, recoveryIndex);
            Assert.IsLessThan(legacyRuntimeIndex, runtimeRouteIndex);
            Assert.DoesNotContain("JasonQueryDbStorageRuntimeStartupGate.EnsureNormalRuntimeReady", source);
        }

        [TestMethod]
        public void JasonQueryDbPasswordDialog_HasDedicatedPhysicalMigrationCustomPasswordValidationMode()
        {
            var source = ReadSource
            (
                FindRepositoryRoot(),
                "JasonQuery",
                "UI",
                "Forms",
                "JasonQueryDbPasswordDialog.cs"
            );

            Assert.Contains("Func<string, bool> storageMigrationCustomPasswordValidator", source);
            Assert.Contains("ResolveStorageMigrationCustomPassword", source);
            Assert.Contains("_storageMigrationCustomPasswordValidator(txtCustomPassword.Text)", source);
        }

        [TestMethod]
        public void MainFormStartup_DatabaseSecurityRemainsBeforeGlobalDatabaseAccess()
        {
            var source = ReadSource
            (
                FindRepositoryRoot(),
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.cs"
            );

            var securityIndex = source.IndexOf
            (
                "InitializeDatabaseSecurity(dbFilePath);",
                StringComparison.Ordinal
            );

            var globalSettingIndex = source.IndexOf
            (
                "LoadGlobalSetting();",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, securityIndex, "Database security startup call was not found.");
            Assert.IsGreaterThanOrEqualTo(0, globalSettingIndex, "Global settings startup call was not found.");
            Assert.IsLessThan(globalSettingIndex, securityIndex, "Physical migration recovery must remain upstream from normal JasonQuery.db access.");
        }

        private static string FindRepositoryRoot()
        {
            var currentDirectoryRoot = FindRepositoryRootFrom
            (
                Directory.GetCurrentDirectory()
            );

            if (!string.IsNullOrEmpty(currentDirectoryRoot))
            {
                return currentDirectoryRoot;
            }

            var baseDirectoryRoot = FindRepositoryRootFrom
            (
                AppDomain.CurrentDomain.BaseDirectory
            );

            if (!string.IsNullOrEmpty(baseDirectoryRoot))
            {
                return baseDirectoryRoot;
            }

            Assert.Fail
            (
                "Could not locate the JasonQuery repository root for R4E startup integration guard tests."
            );

            return string.Empty;
        }

        private static string FindRepositoryRootFrom(string startPath)
        {
            if (string.IsNullOrWhiteSpace(startPath))
            {
                return null;
            }

            var directory = new DirectoryInfo
            (
                Path.GetFullPath(startPath)
            );

            while (directory != null)
            {
                var projectFilePath = Path.Combine
                (
                    directory.FullName,
                    "JasonQuery",
                    "JasonQuery.csproj"
                );

                var testsDirectoryPath = Path.Combine
                (
                    directory.FullName,
                    "Tests",
                    "JasonQuery.Tests"
                );

                if (File.Exists(projectFilePath) && Directory.Exists(testsDirectoryPath))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string ReadSource(string repositoryRoot, params string[] pathParts)
        {
            var sourcePath = repositoryRoot;

            foreach (var pathPart in pathParts)
            {
                sourcePath = Path.Combine(sourcePath, pathPart);
            }

            Assert.IsTrue
            (
                File.Exists(sourcePath),
                "Expected source file was not found: " + sourcePath
            );

            return File.ReadAllText(sourcePath);
        }
    }
}
