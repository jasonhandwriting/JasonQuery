using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.ConnectionCredentials
{
    [TestClass]
    public class ConnectionCredentialRuntimeCutoverGuardTests
    {
        [TestMethod]
        public void RuntimeCredentialReadCallSites_UseV2ContractAndDoNotUseLegacyCredentialSecurity()
        {
            var repositoryRoot = FindRepositoryRoot();

            var dbInfoGridSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionForm.DbInfoGrid.cs"
            );

            var exportSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionExportForm.cs"
            );

            Assert.Contains("ConnectionCredentialStorageContract.FromV2StoredValue", dbInfoGridSource);
            Assert.Contains("ConnectionCredentialStorageContract.FromV2StoredValue", exportSource);
            Assert.DoesNotContain("LegacyConnectionCredentialSecurity.", dbInfoGridSource, "ConnectionForm runtime credential reads must not use legacy per-field protection.");
            Assert.DoesNotContain("LegacyConnectionCredentialSecurity.", exportSource, "Connection export must read DBInfo.Password using the V2 logical-value contract.");
        }

        [TestMethod]
        public void RuntimeCredentialWriteCallSites_UseV2ContractAndParameterizedPassword()
        {
            var repositoryRoot = FindRepositoryRoot();

            var saveSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionForm.Save.cs"
            );

            var importSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionImportForm.cs"
            );

            Assert.Contains("ConnectionCredentialStorageContract.ToV2StoredValue", saveSource);
            Assert.Contains("Password = @Password", saveSource);
            Assert.Contains("new JasonQueryDatabaseParameter(\"@Password\", storedPassword)", saveSource);
            Assert.Contains("ConnectionCredentialStorageContract.ToV2StoredValue", importSource);
            Assert.Contains("passwordParameterName = $\"@Password{count}\"", importSource);
            Assert.Contains("passwordParameters.ToArray()", importSource);
            Assert.DoesNotContain("LegacyConnectionCredentialSecurity.", saveSource, "ConnectionForm runtime credential writes must not use legacy per-field protection.");
            Assert.DoesNotContain("LegacyConnectionCredentialSecurity.", importSource, "Connection import must persist DBInfo.Password using the V2 logical-value contract.");
        }

        [TestMethod]
        public void StartupCredentialGate_RunsBeforeDatabaseRuntimeIsMarkedV2()
        {
            var repositoryRoot = FindRepositoryRoot();

            var databaseSecuritySource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            var mainFormSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.cs"
            );

            var ensureReadyIndex = databaseSecuritySource.IndexOf
            (
                "credentialStorageStartupGate.EnsureReady(connection)",
                StringComparison.Ordinal
            );

            var setV2Index = databaseSecuritySource.IndexOf
            (
                "JasonQueryDbSecurityRuntime.SetV2(metadata.Mode)",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, ensureReadyIndex, "The credential storage startup gate call was not found.");
            Assert.IsGreaterThanOrEqualTo(0, setV2Index, "The Database Security V2 runtime marker call was not found.");
            Assert.IsLessThan(setV2Index, ensureReadyIndex, "DBInfo.Password migration readiness must be established before JasonQueryDbSecurityRuntime is marked V2.");

            var initializeDatabaseSecurityIndex = mainFormSource.IndexOf
            (
                "InitializeDatabaseSecurity(dbFilePath);",
                StringComparison.Ordinal
            );

            var loadGlobalSettingIndex = mainFormSource.IndexOf
            (
                "LoadGlobalSetting();",
                StringComparison.Ordinal
            );

            var loadConnectionFormIndex = mainFormSource.IndexOf
            (
                "LoadConnectionForm();",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, initializeDatabaseSecurityIndex, "InitializeDatabaseSecurity startup call was not found.");
            Assert.IsGreaterThanOrEqualTo(0, loadGlobalSettingIndex, "LoadGlobalSetting startup call was not found.");
            Assert.IsGreaterThanOrEqualTo(0, loadConnectionFormIndex, "LoadConnectionForm startup call was not found.");
            Assert.IsLessThan(loadGlobalSettingIndex, initializeDatabaseSecurityIndex, "Database security and credential migration must finish before global settings access JasonQuery.db.");
            Assert.IsLessThan(loadConnectionFormIndex, loadGlobalSettingIndex, "ConnectionForm must remain downstream from the database security startup gate.");
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
