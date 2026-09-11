using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseStorageFormatContractTests
    {
        [TestMethod]
        public void LegacyVersion_IsPersistedValueOne()
        {
            Assert.AreEqual
            (
                1,
                DatabaseStorageFormatContract.LegacyVersion
            );
        }

        [TestMethod]
        public void ModernVersion_IsPersistedValueTwo()
        {
            Assert.AreEqual
            (
                2,
                DatabaseStorageFormatContract.ModernVersion
            );
        }

        [TestMethod]
        public void CurrentVersion_RemainsLegacyUntilRuntimeCutover()
        {
            Assert.AreEqual
            (
                DatabaseStorageFormatContract.LegacyVersion,
                DatabaseStorageFormatContract.CurrentVersion
            );
        }

        [TestMethod]
        public void ModernCompatibilityProfile_MatchesQualifiedSqlCipher4Format()
        {
            Assert.AreEqual(4, DatabaseStorageFormatContract.ModernSqlCipherCompatibility);
            Assert.AreEqual(4096, DatabaseStorageFormatContract.ModernCipherPageSize);
            Assert.AreEqual(256000, DatabaseStorageFormatContract.ModernKdfIterations);
            Assert.AreEqual("PBKDF2_HMAC_SHA512", DatabaseStorageFormatContract.ModernKdfAlgorithm);
            Assert.AreEqual("HMAC_SHA512", DatabaseStorageFormatContract.ModernHmacAlgorithm);
            Assert.AreEqual(1, DatabaseStorageFormatContract.ModernUseHmac);
            Assert.AreEqual(0, DatabaseStorageFormatContract.ModernPlaintextHeaderSize);
        }

        [TestMethod]
        public void ResolveVersion_MissingMarker_ReturnsLegacy()
        {
            var version = DatabaseStorageFormatContract.ResolveVersion(null);

            Assert.AreEqual
            (
                DatabaseStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                version
            );
        }

        [TestMethod]
        public void ResolveVersion_ExplicitLegacyMarker_ReturnsLegacy()
        {
            var version = DatabaseStorageFormatContract.ResolveVersion
            (
                DatabaseStorageFormatContract.LegacyVersion
            );

            Assert.AreEqual
            (
                DatabaseStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                version
            );
        }

        [TestMethod]
        public void ResolveVersion_ExplicitModernMarker_ReturnsModern()
        {
            var version = DatabaseStorageFormatContract.ResolveVersion
            (
                DatabaseStorageFormatContract.ModernVersion
            );

            Assert.AreEqual
            (
                DatabaseStorageFormatVersion.SqlCipherCompatibility4,
                version
            );
        }

        [TestMethod]
        public void RequiresMigration_MissingMarker_ReturnsFalseWhileLegacyIsCurrent()
        {
            Assert.IsFalse
            (
                DatabaseStorageFormatContract.RequiresMigration(null)
            );
        }

        [TestMethod]
        public void RequiresMigration_ModernMarker_ReturnsTrueUntilRuntimeCutover()
        {
            Assert.IsTrue
            (
                DatabaseStorageFormatContract.RequiresMigration
                (
                    DatabaseStorageFormatContract.ModernVersion
                )
            );
        }

        [TestMethod]
        public void EnsureCurrentReady_MissingMarker_AllowsCurrentLegacyFormat()
        {
            DatabaseStorageFormatContract.EnsureCurrentReady(null);
        }

        [TestMethod]
        public void EnsureCurrentReady_ModernMarker_RejectsBeforeRuntimeCutover()
        {
            Assert.ThrowsException<InvalidOperationException>
            (
                () => DatabaseStorageFormatContract.EnsureCurrentReady
                (
                    DatabaseStorageFormatContract.ModernVersion
                )
            );
        }

        [TestMethod]
        public void ResolveVersion_UnknownVersion_ThrowsNotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageFormatContract.ResolveVersion(3)
            );
        }
    }
}
