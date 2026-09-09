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
        public void CurrentVersion_RemainsLegacyUntilModernFormatIsDefined()
        {
            Assert.AreEqual
            (
                DatabaseStorageFormatContract.LegacyVersion,
                DatabaseStorageFormatContract.CurrentVersion
            );
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
            var version = DatabaseStorageFormatContract.ResolveVersion(DatabaseStorageFormatContract.LegacyVersion);

            Assert.AreEqual
            (
                DatabaseStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
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
        public void EnsureCurrentReady_MissingMarker_AllowsCurrentLegacyFormat()
        {
            DatabaseStorageFormatContract.EnsureCurrentReady(null);
        }

        [TestMethod]
        public void ResolveVersion_UnknownVersion_ThrowsNotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageFormatContract.ResolveVersion(2)
            );
        }
    }
}
