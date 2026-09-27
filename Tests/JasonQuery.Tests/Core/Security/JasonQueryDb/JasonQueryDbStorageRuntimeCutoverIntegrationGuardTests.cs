using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageRuntimeCutoverIntegrationGuardTests
    {
        [TestMethod] public void Startup_RecoversJournalBeforeResolvingPersistedRoute() => AssertOrder("RecoverDatabaseStorageMigrationIfNeeded", "JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadataStore)");
        [TestMethod] public void Startup_ModernRoute_DoesNotConstructLegacyMigrationDatabaseFirst() => AssertOrder("if (storageRoute.RuntimeKind == JasonQueryDbStorageRuntimeKind.ModernSqlCipher)", "new SqliteDatabaseSecurityMigrationDatabase()");
        [TestMethod] public void Startup_UnknownRoute_FailsClosed() => AssertContains("storageRoute.RuntimeKind != JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite");
        [TestMethod] public void LegacyStartup_MigratesCredentialStorageBeforePhysicalMigration() => AssertOrder("credentialStorageStartupGate.EnsureReady(connection)", "cutover.MigrateLegacyStorageToModern");
        [TestMethod] public void PhysicalMigration_UsesSeparateLegacyAndModernHelpers() { AssertContains("LegacyStorageV1MigrationHelperRelativePath"); AssertContains("ModernStorageV2MigrationHelperRelativePath"); }
        [TestMethod] public void ModernStartup_UsesIsolatedRuntime() => AssertContains("new ModernSqlCipherDatabaseRuntime()");
        [TestMethod] public void ModernStartup_ValidatesCredentialMarkerThroughRuntime() => AssertContains("ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready");
        [TestMethod] public void PasswordDialog_NoLongerConstructsLegacyProviderForV2Validation() { var s = Read("JasonQuery", "UI", "Forms", "JasonQueryDbPasswordDialog.cs"); Assert.DoesNotContain("new SqliteDatabaseSecurityMigrationDatabase()", s); Assert.Contains("_currentDatabasePasswordValidator", s); var main = Read("JasonQuery", "UI", "Forms", "MainForm.DatabaseSecurity.cs"); Assert.Contains("JasonQueryRepository.CanOpenDatabase", main); Assert.Contains("ValidateModernStorageV2Password", main); }
        [TestMethod] public void MainProcess_RemainsFreeOfSQLitePCL() { var s = Read("JasonQuery", "JasonQuery.csproj"); Assert.DoesNotContain("SQLitePCLRaw", s); Assert.DoesNotContain("sqlcipher.dll", s); var migration = Read("Migration", "ModernSQLite", "JasonQuery.ModernDbMigration", "JasonQuery.ModernDbMigration.csproj"); var runtime = Read("Runtime", "ModernSQLite", "JasonQuery.ModernDbRuntime", "JasonQuery.ModernDbRuntime.csproj"); Assert.DoesNotContain("CopyToOutputDirectory=\"PreserveNewest\"", migration); Assert.DoesNotContain("CopyToOutputDirectory=\"PreserveNewest\"", runtime); Assert.Contains("CopyQualifiedModernSqlCipherNativeToOutput", migration); Assert.Contains("CopyQualifiedModernSqlCipherNativeToOutput", runtime); }
        [TestMethod]
        public void R4F2D_DoesNotCutCurrentVersionAndImplementsRekeyThroughIsolatedRuntime()
        {
            var formatContract = Read
            (
                "JasonQuery",
                "Core",
                "Security",
                "JasonQueryDb",
                "JasonQueryDbStorageFormatContract.cs"
            );

            Assert.Contains("public const int CurrentVersion = LegacyVersion;", formatContract);

            var facade = Read
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernSqlCipherDatabaseRuntime.cs"
            );

            var session = Read
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "ModernDbRuntimeSession.cs"
            );

            var sharedRuntime = Read
            (
                "Migration",
                "ModernSQLite",
                "JasonQuery.ModernDbMigration",
                "ModernSqlCipherRuntime.cs"
            );

            Assert.Contains("client.Rekey(newCredentialBytes)", facade);
            Assert.DoesNotContain("using SQLitePCL;", facade);
            Assert.DoesNotContain("raw.sqlite3_rekey", facade);
            Assert.Contains("ModernSqlCipherRuntime.Rekey(_database, newDatabasePasswordUtf8)", session);
            Assert.Contains("raw.sqlite3_rekey", sharedRuntime);
        }

        [TestMethod]
        public void D2_TransitionManager_ResolvesProviderFromMetadataAndJournalSource()
        {
            var source = Read
            (
                "JasonQuery",
                "Core",
                "Security",
                "JasonQueryDb",
                "JasonQueryDbSecurityTransitionManager.cs"
            );

            Assert.Contains("Func<JasonQueryDbSecurityMetadata, IJasonQueryDbMigrationDatabase>", source);
            Assert.Contains("ResolveDatabase(journal.SourceMetadata)", source);
            Assert.Contains("EnsureStableStorageFormat", source);
        }

        [TestMethod]
        public void D2_StorageAwareFactory_RoutesOnlyFromStorageFormatMarker()
        {
            var source = Read
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Security",
                "StorageAwareDatabaseSecurityMigrationDatabase.cs"
            );

            Assert.Contains("JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadata.StorageFormatVersion)", source);
            Assert.Contains("JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite", source);
            Assert.Contains("JasonQueryDbStorageRuntimeKind.ModernSqlCipher", source);
            Assert.DoesNotContain("storageFormatVersion ==", source);
        }

        [TestMethod]
        public void D2_ModernSecurityMigration_UsesIsolatedRuntimeWithoutSQLitePCL()
        {
            var source = Read
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Security",
                "StorageAwareDatabaseSecurityMigrationDatabase.cs"
            );

            Assert.Contains("new ModernSqlCipherDatabaseRuntime()", source);
            Assert.Contains("runtime.ChangePassword", source);
            Assert.Contains("PRAGMA wal_checkpoint(TRUNCATE);", source);
            Assert.DoesNotContain("using SQLitePCL;", source);
            Assert.DoesNotContain("System.Data.SQLite", source);
            Assert.DoesNotContain("raw.sqlite3_rekey", source);
        }

        [TestMethod]
        public void D2_Startup_RecoversSecurityTransitionBeforePersistedRoute()
        {
            var source = Read
            (
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            var recoveryIndex = source.IndexOf
            (
                "transitionManager.RecoverInterruptedChangeIfNeeded",
                StringComparison.Ordinal
            );

            var routeIndex = source.IndexOf
            (
                "JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadataStore)",
                StringComparison.Ordinal
            );

            Assert.IsTrue(recoveryIndex >= 0 && routeIndex > recoveryIndex);
            Assert.DoesNotContain("EnsureNoLegacySecurityTransitionJournal", source);
            Assert.Contains("StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create", source);
        }

        [TestMethod]
        public void D2_JasonQueryDbSecurityForm_UsesStorageAwareResolverAndReleasesRuntimeHandle()
        {
            var source = Read
            (
                "JasonQuery",
                "UI",
                "Forms",
                "JasonQueryDbSecurityForm.cs"
            );

            Assert.Contains("StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create", source);
            Assert.Contains("ReleaseRuntimeDatabaseHandleForSecurityTransition", source);
            Assert.DoesNotContain("new SqliteDatabaseSecurityMigrationDatabase()", source);
        }

        [TestMethod]
        public void D2_RepositoryRelease_ClosesModernCachedHandleWithoutLegacyFallback()
        {
            var repository = Read
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Repositories",
                "JasonQueryRepository.cs"
            );

            var runtime = Read
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernSqlCipherDatabaseRuntime.cs"
            );

            Assert.Contains("ReleaseRuntimeDatabaseHandleForSecurityTransition", repository);
            Assert.Contains("modernRuntime.ReleaseDatabaseHandle()", repository);
            Assert.Contains("internal void ReleaseDatabaseHandle()", runtime);
            Assert.Contains("DisposeClient();", runtime);
        }

        private static void AssertContains(string value) { Assert.Contains(value, Read("JasonQuery", "UI", "Forms", "MainForm.DatabaseSecurity.cs")); }
        private static void AssertOrder(string first, string second) { var s = Read("JasonQuery", "UI", "Forms", "MainForm.DatabaseSecurity.cs"); var a = s.IndexOf(first, StringComparison.Ordinal); var b = s.IndexOf(second, StringComparison.Ordinal); Assert.IsTrue(a >= 0 && b > a, first + " must precede " + second); }
        private static string Read(params string[] parts) { var root = FindRepositoryRoot(); foreach (var part in parts) root = Path.Combine(root, part); return File.ReadAllText(root); }
        private static string FindRepositoryRoot() { var d = new DirectoryInfo(Directory.GetCurrentDirectory()); while (d != null) { if (File.Exists(Path.Combine(d.FullName, "JasonQuery", "JasonQuery.csproj")) && Directory.Exists(Path.Combine(d.FullName, "Tests", "JasonQuery.Tests"))) return d.FullName; d = d.Parent; } d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); while (d != null) { if (File.Exists(Path.Combine(d.FullName, "JasonQuery", "JasonQuery.csproj")) && Directory.Exists(Path.Combine(d.FullName, "Tests", "JasonQuery.Tests"))) return d.FullName; d = d.Parent; } Assert.Fail("Repository root not found."); return string.Empty; }
    }
}
