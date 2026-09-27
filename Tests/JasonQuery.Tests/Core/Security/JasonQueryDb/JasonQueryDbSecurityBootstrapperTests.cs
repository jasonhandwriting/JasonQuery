using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbSecurityBootstrapperTests
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

                Assert.AreEqual(JasonQueryDbSecurityStartupState.Legacy, result.State);
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

                var databaseKey = JasonQueryDbKeyGenerator.Generate();
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);

                store.Save(JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(databaseKey)));

                var bootstrapper = new JasonQueryDbSecurityBootstrapper(store, new PassThroughKeyProtector());
                var result = bootstrapper.Resolve(databasePath);

                Assert.AreEqual(JasonQueryDbSecurityStartupState.V2Ready, result.State);
                Assert.AreEqual(JasonQueryDbKeyGenerator.ToDatabasePassword(databaseKey), result.DatabasePassword);
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

                var salt = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);

                store.Save(JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000));

                var bootstrapper = new JasonQueryDbSecurityBootstrapper(store, new PassThroughKeyProtector());
                var result = bootstrapper.Resolve(databasePath);

                Assert.AreEqual(JasonQueryDbSecurityStartupState.V2CustomPasswordRequired, result.State);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void ResolveCustomPassword_ReturnsDerivedDatabasePassword()
        {
            var salt = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();
            var metadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000);
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var bootstrapper = new JasonQueryDbSecurityBootstrapper(new JasonQueryDbSecurityMetadataStore(metadataPath), new PassThroughKeyProtector());
                var result = bootstrapper.ResolveCustomPassword(metadata, "JasonQuery-Test-Password");
                var expected = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabasePassword("JasonQuery-Test-Password", salt, 1000);

                Assert.AreEqual(JasonQueryDbSecurityStartupState.V2Ready, result.State);
                Assert.AreEqual(expected, result.DatabasePassword);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Resolve_WindowsCurrentUserKeyUnavailable_ThrowsSpecificStartupError()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");

                File.WriteAllBytes(databasePath, new byte[] { 1 });

                var databaseKey = JasonQueryDbKeyGenerator.Generate();
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);

                store.Save
                (
                    JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                    (
                        Convert.ToBase64String(databaseKey)
                    )
                );

                var bootstrapper = new JasonQueryDbSecurityBootstrapper
                (
                    store,
                    new FailingKeyProtector()
                );

                var exception = Assert.ThrowsExactly<JasonQueryDbSecurityStartupException>
                (
                    () => bootstrapper.Resolve(databasePath)
                );

                Assert.AreEqual
                (
                    JasonQueryDbSecurityStartupErrorKind.WindowsCurrentUserKeyUnavailable,
                    exception.ErrorKind
                );

                Assert.IsInstanceOfType(exception.InnerException, typeof(CryptographicException));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Resolve_WindowsCurrentUserKeyUnavailable_WithRecovery_ReturnsRecoveryRequired()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");

                File.WriteAllBytes(databasePath, new byte[] { 1 });

                var databaseKey = JasonQueryDbKeyGenerator.Generate();
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);

                using (var recoveryKey = JasonQueryDbRecoveryKeyCodec.Generate())
                {
                    var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(databaseKey));

                    metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;
                    metadata.Recovery = JasonQueryDbRecoveryMetadata.Create
                    (
                        JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey)
                    );

                    store.Save(metadata);
                }

                var bootstrapper = new JasonQueryDbSecurityBootstrapper(store, new FailingKeyProtector());
                var result = bootstrapper.Resolve(databasePath);

                Assert.AreEqual(JasonQueryDbSecurityStartupState.V2RecoveryRequired, result.State);
                Assert.IsNotNull(result.Metadata);
                Assert.IsNotNull(result.Metadata.Recovery);
                Assert.IsNull(result.DatabasePassword);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static JasonQueryDbSecurityBootstrapper CreateBootstrapper(string directory, IJasonQueryDbKeyProtector keyProtector)
        {
            return new JasonQueryDbSecurityBootstrapper
            (
                new JasonQueryDbSecurityMetadataStore
                (
                    Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName)
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

        private sealed class PassThroughKeyProtector : IJasonQueryDbKeyProtector
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

        private sealed class FailingKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                return (byte[])databaseKey.Clone();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                throw new CryptographicException("Simulated DPAPI failure.");
            }
        }
    }
}
