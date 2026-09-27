using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageRuntimeCutoverCoordinatorTests
    {
        [TestMethod]
        public void Constructor_EmptyDatabasePath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new JasonQueryDbStorageRuntimeCutoverCoordinator(" ", "metadata.json", new FakeMetadataStore()));
        }

        [TestMethod]
        public void Constructor_NullMetadataStore_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new JasonQueryDbStorageRuntimeCutoverCoordinator("database.db", "metadata.json", null));
        }

        [TestMethod]
        public void ResolvePersistedRoute_MissingMetadata_UsesLegacyRoute()
        {
            var scope = new FakeMetadataStore();
            var coordinator = Create(scope, null);

            Assert.AreEqual(JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite, coordinator.ResolvePersistedRoute().RuntimeKind);
        }

        [TestMethod]
        public void ResolvePersistedRoute_ModernMetadata_UsesModernRoute()
        {
            var scope = FakeMetadataStore.Create(JasonQueryDbStorageFormatContract.ModernVersion);
            var coordinator = Create(scope, null);

            Assert.AreEqual(JasonQueryDbStorageRuntimeKind.ModernSqlCipher, coordinator.ResolvePersistedRoute().RuntimeKind);
        }

        [TestMethod]
        public void MigrateLegacyStorageToModern_AlreadyModern_FailsClosed()
        {
            using (var files = new HelperFiles())
            {
                var scope = FakeMetadataStore.Create(JasonQueryDbStorageFormatContract.ModernVersion);
                var coordinator = Create(scope, (a, b, c, d) => null);

                Assert.ThrowsExactly<InvalidOperationException>(() => coordinator.MigrateLegacyStorageToModern(files.Legacy, files.Modern, "credential"));
            }
        }

        [TestMethod]
        public void MigrateLegacyStorageToModern_MigrationMustCommitModernRoute()
        {
            using (var files = new HelperFiles())
            {
                var scope = FakeMetadataStore.Create(JasonQueryDbStorageFormatContract.LegacyVersion);
                var coordinator = Create(scope, (a, b, c, d) => new JasonQueryDbStorageMigrationCoordinatorResult("op", 1, "DB", "META"));

                Assert.ThrowsExactly<InvalidDataException>(() => coordinator.MigrateLegacyStorageToModern(files.Legacy, files.Modern, "credential"));
            }
        }

        [TestMethod]
        public void MigrateLegacyStorageToModern_PassesSameResolvedCredentialToBothSides()
        {
            using (var files = new HelperFiles())
            {
                var scope = FakeMetadataStore.Create(JasonQueryDbStorageFormatContract.LegacyVersion);
                byte[] source = null;
                byte[] target = null;

                var coordinator = Create(scope, (a, b, c, d) =>
                {
                    source = (byte[])c.Clone();
                    target = (byte[])d.Clone();
                    scope.Metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;

                    return new JasonQueryDbStorageMigrationCoordinatorResult("op", 1, "DB", "META");
                });

                coordinator.MigrateLegacyStorageToModern(files.Legacy, files.Modern, "credential");
                CollectionAssert.AreEqual(source, target);
                CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("credential"), source);
                Array.Clear(source, 0, source.Length);
                Array.Clear(target, 0, target.Length);
            }
        }

        [TestMethod]
        public void MigrateLegacyStorageToModern_ReReadsPersistedRouteAfterMigration()
        {
            using (var files = new HelperFiles())
            {
                var scope = FakeMetadataStore.Create(JasonQueryDbStorageFormatContract.LegacyVersion);

                var coordinator = Create(scope, (a, b, c, d) =>
                {
                    scope.Metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;
                    return new JasonQueryDbStorageMigrationCoordinatorResult("op", 1, "DB", "META");
                });

                coordinator.MigrateLegacyStorageToModern(files.Legacy, files.Modern, "credential");
                Assert.AreEqual(JasonQueryDbStorageRuntimeKind.ModernSqlCipher, coordinator.ResolvePersistedRoute().RuntimeKind);
            }
        }

        private static JasonQueryDbStorageRuntimeCutoverCoordinator Create(FakeMetadataStore store, Func<string, string, byte[], byte[], JasonQueryDbStorageMigrationCoordinatorResult> migrate)
        {
            return new JasonQueryDbStorageRuntimeCutoverCoordinator(Path.Combine(Path.GetTempPath(), "JasonQuery.db"), Path.Combine(Path.GetTempPath(), "JasonQuery.security.json"), store, migrate ?? ((a, b, c, d) => null));
        }

        private sealed class FakeMetadataStore : IJasonQueryDbSecurityMetadataStore
        {
            public string MetadataFilePath => Path.Combine(Path.GetTempPath(), "JasonQuery.security.json");
            public bool Exists => Metadata != null;
            public JasonQueryDbSecurityMetadata Metadata { get; private set; }
            public JasonQueryDbSecurityMetadata Load() => Metadata;
            public void Save(JasonQueryDbSecurityMetadata metadata) => Metadata = metadata;
            public void Delete() => Metadata = null;
            public static FakeMetadataStore Create(int version)
            {
                var store = new FakeMetadataStore();
                var metadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(new byte[16], JasonQueryDbSecurityConstants.DefaultPbkdf2Iterations);

                metadata.StorageFormatVersion = version;
                store.Metadata = metadata;

                return store;
            }
        }

        private sealed class HelperFiles : IDisposable
        {
            public HelperFiles()
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery-R4F2C-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);
                Legacy = Path.Combine(DirectoryPath, "legacy.exe");
                Modern = Path.Combine(DirectoryPath, "modern.exe");
                File.WriteAllBytes(Legacy, new byte[] { 1 });
                File.WriteAllBytes(Modern, new byte[] { 2 });
            }
            public string DirectoryPath { get; }
            public string Legacy { get; }
            public string Modern { get; }
            public void Dispose() { if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true); }
        }
    }
}
