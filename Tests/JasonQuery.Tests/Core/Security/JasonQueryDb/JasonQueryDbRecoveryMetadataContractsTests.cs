using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbRecoveryMetadataContractsTests
    {
        [TestMethod]
        public void Factories_CreateCurrentMetadataVersionTwo()
        {
            var windows = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser("AQIDBA==");
            var salt = CreatePasswordSalt();

            try
            {
                var custom = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000);

                Assert.AreEqual(JasonQueryDbSecurityConstants.MetadataVersion, windows.MetadataVersion);
                Assert.AreEqual(JasonQueryDbSecurityConstants.MetadataVersion, custom.MetadataVersion);
                Assert.AreEqual(2, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbSecurityConstants), nameof(JasonQueryDbSecurityConstants.MetadataVersion)));
                Assert.AreEqual(1, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbSecurityConstants), nameof(JasonQueryDbSecurityConstants.LegacyMetadataVersion)));
            }
            finally
            {
                Array.Clear(salt, 0, salt.Length);
            }
        }

        [TestMethod]
        public void LegacyVersionOne_WindowsWithoutRecovery_Validates()
        {
            var metadata = CreateWindowsMetadata(JasonQueryDbSecurityConstants.LegacyMetadataVersion, false);

            metadata.Validate();
        }

        [TestMethod]
        public void LegacyVersionOne_CustomPasswordWithoutRecovery_Validates()
        {
            var metadata = CreateCustomMetadata(JasonQueryDbSecurityConstants.LegacyMetadataVersion);

            metadata.Validate();
        }

        [TestMethod]
        public void LegacyVersionOne_WithRecovery_Rejects()
        {
            var metadata = CreateWindowsMetadata(JasonQueryDbSecurityConstants.LegacyMetadataVersion, true);

            Assert.ThrowsExactly<InvalidDataException>(() => metadata.Validate());
        }

        [TestMethod]
        public void VersionTwo_WindowsWithoutRecovery_Validates()
        {
            var metadata = CreateWindowsMetadata(JasonQueryDbSecurityConstants.MetadataVersion, false);

            metadata.Validate();
        }

        [TestMethod]
        public void VersionTwo_WindowsWithRecovery_Validates()
        {
            var metadata = CreateWindowsMetadata(JasonQueryDbSecurityConstants.MetadataVersion, true);

            metadata.Validate();
        }

        [TestMethod]
        public void VersionTwo_CustomPasswordWithRecovery_Rejects()
        {
            var metadata = CreateCustomMetadata(JasonQueryDbSecurityConstants.MetadataVersion);

            metadata.Recovery = CreateValidRecoveryMetadata();

            Assert.ThrowsExactly<InvalidDataException>(() => metadata.Validate());
        }

        [TestMethod]
        public void UnknownMetadataVersion_Rejects()
        {
            var metadata = CreateWindowsMetadata(JasonQueryDbSecurityConstants.MetadataVersion, false);

            metadata.MetadataVersion = JasonQueryDbSecurityConstants.MetadataVersion + 1;

            Assert.ThrowsExactly<NotSupportedException>(() => metadata.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_CreateToEnvelope_RoundTrips()
        {
            var recovery = CreateValidRecoveryMetadata();
            var envelope = recovery.ToEnvelope();

            Assert.AreEqual(recovery.Version, envelope.Version);
            Assert.AreEqual(recovery.KeyId, envelope.KeyId);
            Assert.AreEqual(recovery.Kdf, envelope.Kdf);
            Assert.AreEqual(recovery.Cipher, envelope.Cipher);
            Assert.AreEqual(recovery.Mac, envelope.Mac);
        }

        [TestMethod]
        public void RecoveryMetadata_Clone_CreatesEquivalentIndependentObject()
        {
            var source = CreateValidRecoveryMetadata();
            var clone = JasonQueryDbRecoveryMetadata.Clone(source);

            Assert.AreNotSame(source, clone);
            Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(source, clone));

            var originalTag = clone.AuthenticationTag;

            source.AuthenticationTag = Convert.ToBase64String(new byte[JasonQueryDbRecoveryConstants.AuthenticationTagSizeBytes]);

            Assert.AreEqual(originalTag, clone.AuthenticationTag);
            Assert.IsFalse(JasonQueryDbRecoveryMetadata.MetadataEquals(source, clone));
        }

        [TestMethod]
        public void RecoveryMetadata_Equals_DetectsAuthenticationTagDifference()
        {
            var left = CreateValidRecoveryMetadata();
            var right = JasonQueryDbRecoveryMetadata.Clone(left);
            var tag = Convert.FromBase64String(right.AuthenticationTag);

            try
            {
                tag[0] ^= 0x01;
                right.AuthenticationTag = Convert.ToBase64String(tag);
            }
            finally
            {
                Array.Clear(tag, 0, tag.Length);
            }

            Assert.IsFalse(JasonQueryDbRecoveryMetadata.MetadataEquals(left, right));
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsUnknownEnvelopeVersion()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.Version++;

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsUnknownKdf()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.Kdf = "UNKNOWN-KDF";

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsUnknownCipher()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.Cipher = "UNKNOWN-CIPHER";

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsUnknownMac()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.Mac = "UNKNOWN-MAC";

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsInvalidKeyId()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.KeyId = "bad";

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsInvalidSaltLength()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.Salt = Convert.ToBase64String(new byte[JasonQueryDbRecoveryConstants.HkdfSaltSizeBytes - 1]);

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsInvalidIvLength()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.Iv = Convert.ToBase64String(new byte[JasonQueryDbRecoveryConstants.AesIvSizeBytes - 1]);

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsInvalidWrappedDatabaseKeyLength()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.WrappedDatabaseKey = Convert.ToBase64String(new byte[JasonQueryDbRecoveryConstants.WrappedDatabaseKeySizeBytes - 1]);

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void RecoveryMetadata_RejectsInvalidAuthenticationTagLength()
        {
            var recovery = CreateValidRecoveryMetadata();

            recovery.AuthenticationTag = Convert.ToBase64String(new byte[JasonQueryDbRecoveryConstants.AuthenticationTagSizeBytes - 1]);

            Assert.ThrowsExactly<InvalidDataException>(() => recovery.Validate());
        }

        [TestMethod]
        public void MetadataStore_RoundTripsVersionTwoRecoveryMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);
                var metadata = CreateWindowsMetadata(JasonQueryDbSecurityConstants.MetadataVersion, true);

                store.Save(metadata);

                var loaded = store.Load();

                Assert.AreEqual(JasonQueryDbSecurityConstants.MetadataVersion, loaded.MetadataVersion);
                Assert.IsNotNull(loaded.Recovery);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(metadata.Recovery, loaded.Recovery));
                Assert.HasCount(1, Directory.GetFiles(directory, "*.json"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void StorageMigrationTarget_PreservesRecoveryEnvelopeAndOnlyChangesStorageVersion()
        {
            var source = CreateWindowsMetadata(JasonQueryDbSecurityConstants.MetadataVersion, true);

            source.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

            var target = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(source);

            Assert.AreEqual(source.MetadataVersion, target.MetadataVersion);
            Assert.AreEqual(source.EncryptionVersion, target.EncryptionVersion);
            Assert.AreEqual(source.Mode, target.Mode);
            Assert.AreEqual(source.Protection, target.Protection);
            Assert.AreEqual(source.ProtectedDatabaseKey, target.ProtectedDatabaseKey);
            Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, target.StorageFormatVersion);
            Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(source.Recovery, target.Recovery));
        }

        [TestMethod]
        public void StorageMigrationTarget_DeepClonesRecoveryMetadata()
        {
            var source = CreateWindowsMetadata(JasonQueryDbSecurityConstants.MetadataVersion, true);

            source.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

            var target = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(source);
            var targetTag = target.Recovery.AuthenticationTag;

            Assert.AreNotSame(source.Recovery, target.Recovery);

            source.Recovery.AuthenticationTag = Convert.ToBase64String(new byte[JasonQueryDbRecoveryConstants.AuthenticationTagSizeBytes]);

            Assert.AreEqual(targetTag, target.Recovery.AuthenticationTag);
            Assert.IsFalse(JasonQueryDbRecoveryMetadata.MetadataEquals(source.Recovery, target.Recovery));
        }

        private static JasonQueryDbSecurityMetadata CreateWindowsMetadata(int metadataVersion, bool includeRecovery)
        {
            var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser("AQIDBA==");

            metadata.MetadataVersion = metadataVersion;
            metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

            if (includeRecovery)
            {
                metadata.Recovery = CreateValidRecoveryMetadata();
            }

            return metadata;
        }

        private static JasonQueryDbSecurityMetadata CreateCustomMetadata(int metadataVersion)
        {
            var salt = CreatePasswordSalt();

            try
            {
                var metadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000);

                metadata.MetadataVersion = metadataVersion;
                metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;
                return metadata;
            }
            finally
            {
                Array.Clear(salt, 0, salt.Length);
            }
        }

        private static JasonQueryDbRecoveryMetadata CreateValidRecoveryMetadata()
        {
            var databaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];
            var recoverySecret = new byte[JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes];

            for (var index = 0; index < databaseKey.Length; index++)
            {
                databaseKey[index] = (byte)(index + 1);
                recoverySecret[index] = (byte)(255 - index);
            }

            try
            {
                using (var recoveryKey = new JasonQueryDbRecoveryKeyMaterial("A1B2C3D4", recoverySecret))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);

                    return JasonQueryDbRecoveryMetadata.Create(envelope);
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
                Array.Clear(recoverySecret, 0, recoverySecret.Length);
            }
        }

        private static byte[] CreatePasswordSalt()
        {
            var salt = new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes];

            for (var index = 0; index < salt.Length; index++)
            {
                salt[index] = (byte)(index + 1);
            }

            return salt;
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-R4F2E5-B2-" + Guid.NewGuid().ToString("N")
            );

            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
