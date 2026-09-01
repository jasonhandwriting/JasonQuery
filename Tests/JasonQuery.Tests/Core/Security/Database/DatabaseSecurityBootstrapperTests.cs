using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseSecurityBootstrapperTests
    {
        [TestMethod]
        public void Resolve_MetadataMissing_ReturnsLegacy()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");

                File.WriteAllBytes(databasePath, new byte[] { 1 });

                var bootstrapper = CreateBootstrapper(directory, new PassThroughKeyProtector());
                var result = bootstrapper.Resolve(databasePath);

                Assert.AreEqual(DatabaseSecurityStartupState.Legacy, result.State);
                Assert.IsNull(result.DatabasePassword);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Resolve_WindowsCurrentUserMetadata_ReturnsV2Ready()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");

                File.WriteAllBytes(databasePath, new byte[] { 1 });

                var databaseKey = DatabaseKeyGenerator.Generate();
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);
                var store = new DatabaseSecurityMetadataStore(metadataPath);

                store.Save(DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(databaseKey)));

                var bootstrapper = new DatabaseSecurityBootstrapper(store, new PassThroughKeyProtector());
                var result = bootstrapper.Resolve(databasePath);

                Assert.AreEqual(DatabaseSecurityStartupState.V2Ready, result.State);
                Assert.AreEqual(DatabaseKeyGenerator.ToDatabasePassword(databaseKey), result.DatabasePassword);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Resolve_CustomPasswordMetadata_RequiresPassword()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");

                File.WriteAllBytes(databasePath, new byte[] { 1 });

                var salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);
                var store = new DatabaseSecurityMetadataStore(metadataPath);

                store.Save(DatabaseSecurityMetadata.CreateCustomPassword(salt, 1000));

                var bootstrapper = new DatabaseSecurityBootstrapper(store, new PassThroughKeyProtector());
                var result = bootstrapper.Resolve(databasePath);

                Assert.AreEqual(DatabaseSecurityStartupState.V2CustomPasswordRequired, result.State);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void ResolveCustomPassword_ReturnsDerivedDatabasePassword()
        {
            var salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();
            var metadata = DatabaseSecurityMetadata.CreateCustomPassword(salt, 1000);
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);
                var bootstrapper = new DatabaseSecurityBootstrapper(new DatabaseSecurityMetadataStore(metadataPath), new PassThroughKeyProtector());
                var result = bootstrapper.ResolveCustomPassword(metadata, "JasonQuery-Test-Password");
                var expected = CustomPasswordDatabaseKeyDeriver.DeriveDatabasePassword("JasonQuery-Test-Password", salt, 1000);

                Assert.AreEqual(DatabaseSecurityStartupState.V2Ready, result.State);
                Assert.AreEqual(expected, result.DatabasePassword);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static DatabaseSecurityBootstrapper CreateBootstrapper(string directory, IDatabaseKeyProtector keyProtector)
        {
            return new DatabaseSecurityBootstrapper
            (
                new DatabaseSecurityMetadataStore
                (
                    Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName)
                ),
                keyProtector
            );
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "JasonQuery.Tests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);
            return directory;
        }

        private sealed class PassThroughKeyProtector : IDatabaseKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                return (byte[])databaseKey.Clone();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                return (byte[])protectedDatabaseKey.Clone();
            }
        }
    }
}
