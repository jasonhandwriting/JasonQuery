using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class LegacyCustomPasswordMigrationGuardTests
    {
        [TestMethod]
        public void MainForm_LegacyCustomPasswordPath_PreservesCustomPasswordMode()
        {
            var repositoryRoot = FindRepositoryRoot();

            var source = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            StringAssert.Contains(source, "!metadataStore.Exists && migrator.HasInterruptedMigrationBackup(databaseFilePath)");
            StringAssert.Contains(source, "migrator.CanRecoverInterruptedMigrationWithDefaultPassword(databaseFilePath)");
            StringAssert.Contains(source, "migrator.CanOpenWithDefaultPassword(databaseFilePath)");
            StringAssert.Contains(source, "new DatabasePasswordDialog(migrator, databaseFilePath)");
            StringAssert.Contains(source, "migrator.MigrateToCustomPassword");

            var defaultCheckIndex = source.IndexOf
            (
                "if (migrator.CanOpenWithDefaultPassword(databaseFilePath))",
                StringComparison.Ordinal
            );

            var defaultMigrationIndex = source.IndexOf
            (
                "migrator.MigrateToWindowsCurrentUser(databaseFilePath)",
                StringComparison.Ordinal
            );

            var customMigrationIndex = source.IndexOf
            (
                "migrator.MigrateToCustomPassword",
                StringComparison.Ordinal
            );

            Assert.IsTrue(defaultCheckIndex >= 0);
            Assert.IsTrue(defaultMigrationIndex > defaultCheckIndex);
            Assert.IsTrue(customMigrationIndex > defaultMigrationIndex);
        }

        [TestMethod]
        public void DatabasePasswordDialog_LegacyMode_ValidatesPasswordButDoesNotPerformMigration()
        {
            var repositoryRoot = FindRepositoryRoot();

            var source = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "DatabasePasswordDialog.cs"
            );

            StringAssert.Contains
            (
                source,
                "DatabasePasswordDialog(LegacyDatabaseSecurityMigrator legacyMigrator, string databaseFilePath)"
            );

            StringAssert.Contains
            (
                source,
                "_legacyMigrator.IsCustomPasswordValid(_databaseFilePath, txtCustomPassword.Text)"
            );

            StringAssert.Contains(source, "CustomPassword = txtCustomPassword.Text");

            Assert.IsFalse
            (
                source.Contains("MigrateToCustomPassword("),
                "The password dialog should validate and return the legacy custom password; core migration must remain in MainForm/core security flow."
            );
        }

        [TestMethod]
        public void LegacyMigrator_CustomMigration_CreatesV2CustomPasswordMetadataAndUsesSameUserPassword()
        {
            var repositoryRoot = FindRepositoryRoot();

            var source = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "Core",
                "Security",
                "Database",
                "LegacyDatabaseSecurityMigrator.cs"
            );

            StringAssert.Contains(source, "public DatabaseSecurityMigrationResult MigrateToCustomPassword");
            StringAssert.Contains(source, "LegacyDatabaseSecurity.CreateCustomDatabasePassword(customPassword)");
            StringAssert.Contains(source, "CustomPasswordDatabaseKeyDeriver.CreateSalt()");
            StringAssert.Contains(source, "CustomPasswordDatabaseKeyDeriver.DeriveDatabasePassword");
            StringAssert.Contains(source, "DatabaseSecurityMetadata.CreateCustomPassword(salt, iterations)");
            StringAssert.Contains(source, "RecoverInterruptedMigrationIfNeeded(databaseFilePath, legacyDatabasePassword)");
        }

        private static string FindRepositoryRoot()
        {
            var currentDirectoryRoot = FindRepositoryRootFrom(Directory.GetCurrentDirectory());

            if (!string.IsNullOrEmpty(currentDirectoryRoot))
            {
                return currentDirectoryRoot;
            }

            var baseDirectoryRoot = FindRepositoryRootFrom(AppDomain.CurrentDomain.BaseDirectory);

            if (!string.IsNullOrEmpty(baseDirectoryRoot))
            {
                return baseDirectoryRoot;
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
                var projectFilePath = Path.Combine(directory.FullName, "JasonQuery", "JasonQuery.csproj");
                var testsDirectoryPath = Path.Combine(directory.FullName, "Tests", "JasonQuery.Tests");

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

            Assert.IsTrue(File.Exists(sourcePath), $"Required source file was not found: {sourcePath}");
            return File.ReadAllText(sourcePath);
        }
    }
}
