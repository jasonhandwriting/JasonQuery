using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageFormatContractTests
    {
        [TestMethod]
        public void LegacyVersion_IsPersistedValueOne()
        {
            Assert.AreEqual ( 1, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.LegacyVersion)) );
        }

        [TestMethod]
        public void ModernVersion_IsPersistedValueTwo()
        {
            Assert.AreEqual ( 2, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernVersion)) );
        }

        [TestMethod]
        public void CurrentVersion_RemainsLegacyUntilRuntimeCutover()
        {
            Assert.AreEqual ( JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.LegacyVersion)), JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.CurrentVersion)) );
        }

        [TestMethod]
        public void ModernCompatibilityProfile_MatchesQualifiedSqlCipher4Format()
        {
            Assert.AreEqual(4, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernSqlCipherCompatibility)));
            Assert.AreEqual(4096, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernCipherPageSize)));
            Assert.AreEqual(256000, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernKdfIterations)));
            Assert.AreEqual("PBKDF2_HMAC_SHA512", JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernKdfAlgorithm)));
            Assert.AreEqual("HMAC_SHA512", JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernHmacAlgorithm)));
            Assert.AreEqual(1, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernUseHmac)));
            Assert.AreEqual(0, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernPlaintextHeaderSize)));
        }

        [TestMethod]
        public void ResolveVersion_MissingMarker_ReturnsLegacy()
        {
            var version = JasonQueryDbStorageFormatContract.ResolveVersion(null);

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                version
            );
        }

        [TestMethod]
        public void ResolveVersion_ExplicitLegacyMarker_ReturnsLegacy()
        {
            var version = JasonQueryDbStorageFormatContract.ResolveVersion
            (
                JasonQueryDbStorageFormatContract.LegacyVersion
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                version
            );
        }

        [TestMethod]
        public void ResolveVersion_ExplicitModernMarker_ReturnsModern()
        {
            var version = JasonQueryDbStorageFormatContract.ResolveVersion
            (
                JasonQueryDbStorageFormatContract.ModernVersion
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4,
                version
            );
        }

        [TestMethod]
        public void RequiresMigration_MissingMarker_ReturnsFalseWhileLegacyIsCurrent()
        {
            Assert.IsFalse
            (
                JasonQueryDbStorageFormatContract.RequiresMigration(null)
            );
        }

        [TestMethod]
        public void RequiresMigration_ModernMarker_ReturnsTrueUntilRuntimeCutover()
        {
            Assert.IsTrue
            (
                JasonQueryDbStorageFormatContract.RequiresMigration
                (
                    JasonQueryDbStorageFormatContract.ModernVersion
                )
            );
        }

        [TestMethod]
        public void EnsureCurrentReady_MissingMarker_AllowsCurrentLegacyFormat()
        {
            JasonQueryDbStorageFormatContract.EnsureCurrentReady(null);
        }

        [TestMethod]
        public void EnsureCurrentReady_ModernMarker_RejectsBeforeRuntimeCutover()
        {
            Assert.ThrowsExactly<InvalidOperationException>
            (
                () => JasonQueryDbStorageFormatContract.EnsureCurrentReady
                (
                    JasonQueryDbStorageFormatContract.ModernVersion
                )
            );
        }

        [TestMethod]
        public void ResolveVersion_UnknownVersion_ThrowsNotSupportedException()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageFormatContract.ResolveVersion(3)
            );
        }
    }
}
