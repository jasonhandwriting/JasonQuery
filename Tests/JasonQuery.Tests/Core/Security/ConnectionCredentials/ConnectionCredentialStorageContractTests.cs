using JasonQuery.Core.Security.ConnectionCredentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.ConnectionCredentials
{
    [TestClass]
    public class ConnectionCredentialStorageContractTests
    {
        [TestMethod]
        public void ResolveVersion_WhenPersistedVersionIsMissing_ReturnsLegacy()
        {
            var result = ConnectionCredentialStorageContract.ResolveVersion(null);

            Assert.AreEqual
            (
                ConnectionCredentialStorageVersion.LegacyDomainUserProtected,
                result
            );
        }

        [TestMethod]
        public void ResolveVersion_WhenPersistedVersionIsLegacy_ReturnsLegacy()
        {
            var result = ConnectionCredentialStorageContract.ResolveVersion(ConnectionCredentialStorageContract.LegacyVersion);

            Assert.AreEqual
            (
                ConnectionCredentialStorageVersion.LegacyDomainUserProtected,
                result
            );
        }

        [TestMethod]
        public void ResolveVersion_WhenPersistedVersionIsCurrent_ReturnsCurrent()
        {
            var result = ConnectionCredentialStorageContract.ResolveVersion(ConnectionCredentialStorageContract.CurrentVersion);

            Assert.AreEqual
            (
                ConnectionCredentialStorageVersion.DatabaseProtectedLogicalValue,
                result
            );
        }

        [TestMethod]
        public void ResolveVersion_WhenPersistedVersionIsUnknown_ThrowsNotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => ConnectionCredentialStorageContract.ResolveVersion(999)
            );
        }

        [TestMethod]
        public void RequiresMigration_ReturnsExpectedResultForKnownStates()
        {
            Assert.IsTrue
            (
                ConnectionCredentialStorageContract.RequiresMigration(null)
            );

            Assert.IsTrue
            (
                ConnectionCredentialStorageContract.RequiresMigration
                (
                    ConnectionCredentialStorageContract.LegacyVersion
                )
            );

            Assert.IsFalse
            (
                ConnectionCredentialStorageContract.RequiresMigration
                (
                    ConnectionCredentialStorageContract.CurrentVersion
                )
            );
        }

        [TestMethod]
        public void RequiresMigration_WhenPersistedVersionIsUnknown_ThrowsNotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => ConnectionCredentialStorageContract.RequiresMigration(999)
            );
        }

        [TestMethod]
        public void EnsureV2Ready_WhenPersistedVersionIsUnknown_ThrowsNotSupportedException()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => ConnectionCredentialStorageContract.EnsureV2Ready(999)
            );
        }

        [TestMethod]
        public void EnsureV2Ready_WhenPersistedVersionIsMissing_ThrowsInvalidOperationException()
        {
            Assert.ThrowsException<InvalidOperationException>
            (
                () => ConnectionCredentialStorageContract.EnsureV2Ready(null)
            );
        }

        [TestMethod]
        public void EnsureV2Ready_WhenPersistedVersionIsLegacy_ThrowsInvalidOperationException()
        {
            Assert.ThrowsException<InvalidOperationException>
            (
                () => ConnectionCredentialStorageContract.EnsureV2Ready
                (
                    ConnectionCredentialStorageContract.LegacyVersion
                )
            );
        }

        [TestMethod]
        public void EnsureV2Ready_WhenPersistedVersionIsCurrent_DoesNotThrow()
        {
            ConnectionCredentialStorageContract.EnsureV2Ready
            (
                ConnectionCredentialStorageContract.CurrentVersion
            );
        }

        [TestMethod]
        public void V2Storage_RoundTripPreservesLogicalPasswordExactly()
        {
            const string logicalPassword ="  P@ss word-資料庫\t2026!  ";

            var storedValue = ConnectionCredentialStorageContract.ToV2StoredValue(logicalPassword);
            var result = ConnectionCredentialStorageContract.FromV2StoredValue(storedValue);

            Assert.AreEqual(logicalPassword, storedValue);
            Assert.AreEqual(logicalPassword, result);
        }

        [TestMethod]
        public void V2Storage_NullRepresentsNoSavedCredential()
        {
            Assert.AreEqual
            (
                string.Empty,
                ConnectionCredentialStorageContract.ToV2StoredValue(null)
            );

            Assert.AreEqual
            (
                string.Empty,
                ConnectionCredentialStorageContract.FromV2StoredValue(null)
            );
        }
    }
}
