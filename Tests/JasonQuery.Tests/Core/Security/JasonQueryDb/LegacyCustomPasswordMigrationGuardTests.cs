using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class LegacyCustomPasswordMigrationGuardTests
    {
        [TestMethod]
        public void MainForm_LegacyCustomPasswordPath_PreservesCustomPasswordMode()
        {
            var repositoryRoot = FindRepositoryRoot();

            var source = NormalizeWhitespace
            (
                ReadSource
                (
                    repositoryRoot,
                    "JasonQuery",
                    "UI",
                    "Forms",
                    "MainForm.DatabaseSecurity.cs"
                )
            );

            Assert.Contains("var legacyMigrationBackupFilePath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databaseFilePath);", source);
            Assert.Contains("if (!metadataStore.Exists && File.Exists(legacyMigrationBackupFilePath))", source);
            Assert.Contains("if (interruptedMigrator.CanRecoverInterruptedMigrationWithDefaultPassword(databaseFilePath))", source);
            Assert.Contains("if (!TryMigrateLegacyCustomPassword(databaseFilePath, interruptedMigrator))", source);
            Assert.Contains("if (migrator.CanOpenWithDefaultPassword(databaseFilePath))", source);
            Assert.Contains("migrator.MigrateToWindowsCurrentUser(databaseFilePath)", source);
            Assert.Contains("if (!TryMigrateLegacyCustomPassword(databaseFilePath, migrator))", source);
            Assert.Contains("new JasonQueryDbPasswordDialog(migrator, databaseFilePath)", source);
            Assert.Contains("migrator.MigrateToCustomPassword", source);

            var backupPathIndex = source.IndexOf
            (
                "var legacyMigrationBackupFilePath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databaseFilePath);",
                StringComparison.Ordinal
            );

            var interruptedRecoveryIndex = source.IndexOf
            (
                "if (interruptedMigrator.CanRecoverInterruptedMigrationWithDefaultPassword(databaseFilePath))",
                backupPathIndex,
                StringComparison.Ordinal
            );

            var interruptedCustomMigrationIndex = source.IndexOf
            (
                "if (!TryMigrateLegacyCustomPassword(databaseFilePath, interruptedMigrator))",
                interruptedRecoveryIndex,
                StringComparison.Ordinal
            );

            var directFreshIndex = source.IndexOf
            (
                "if (!File.Exists(databaseFilePath) && !metadataStore.Exists)",
                interruptedCustomMigrationIndex,
                StringComparison.Ordinal
            );

            var defaultCheckIndex = source.IndexOf
            (
                "if (migrator.CanOpenWithDefaultPassword(databaseFilePath))",
                directFreshIndex,
                StringComparison.Ordinal
            );

            var defaultMigrationIndex = source.IndexOf
            (
                "migrator.MigrateToWindowsCurrentUser(databaseFilePath)",
                defaultCheckIndex,
                StringComparison.Ordinal
            );

            var customMigrationIndex = source.IndexOf
            (
                "if (!TryMigrateLegacyCustomPassword(databaseFilePath, migrator))",
                defaultMigrationIndex,
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, backupPathIndex);
            Assert.IsGreaterThan(backupPathIndex, interruptedRecoveryIndex);
            Assert.IsGreaterThan(interruptedRecoveryIndex, interruptedCustomMigrationIndex);
            Assert.IsGreaterThan(interruptedCustomMigrationIndex, directFreshIndex);
            Assert.IsGreaterThan(directFreshIndex, defaultCheckIndex);
            Assert.IsGreaterThan(defaultCheckIndex, defaultMigrationIndex);
            Assert.IsGreaterThan(defaultMigrationIndex, customMigrationIndex);
        }

        [TestMethod]
        public void JasonQueryDbPasswordDialog_LegacyMode_ValidatesPasswordButDoesNotPerformMigration()
        {
            var repositoryRoot = FindRepositoryRoot();

            var source = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "JasonQueryDbPasswordDialog.cs"
            );

            Assert.Contains("JasonQueryDbPasswordDialog(JasonQueryDbLegacySecurityMigrator legacyMigrator, string databaseFilePath)", source);
            Assert.Contains("_legacyMigrator.IsCustomPasswordValid(_databaseFilePath, txtCustomPassword.Text)", source);
            Assert.Contains("CustomPassword = txtCustomPassword.Text", source);
            Assert.DoesNotContain("MigrateToCustomPassword(", source, "The password dialog should validate and return the legacy custom password; core migration must remain in MainForm/core security flow.");
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
                "JasonQueryDb",
                "JasonQueryDbLegacySecurityMigrator.cs"
            );

            Assert.Contains("public JasonQueryDbSecurityMigrationResult MigrateToCustomPassword", source);
            Assert.Contains("JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(customPassword)", source);
            Assert.Contains("JasonQueryDbCustomPasswordKeyDeriver.CreateSalt()", source);
            Assert.Contains("JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabasePassword", source);
            Assert.Contains("JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, iterations)", source);
            Assert.Contains("RecoverInterruptedMigrationIfNeeded(databaseFilePath, legacyDatabasePassword)", source);
        }

        private static string NormalizeWhitespace(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var builder = new StringBuilder(value.Length);
            var pendingSpace = false;

            foreach (var character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString();
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
