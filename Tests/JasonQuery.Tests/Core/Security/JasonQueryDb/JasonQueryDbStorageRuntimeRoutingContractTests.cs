using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageRuntimeRoutingContractTests
    {
        [TestMethod]
        public void RuntimeKind_Values_AreStable()
        {
            Assert.AreEqual(1, (int)RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageRuntimeKind), nameof(JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite)));
            Assert.AreEqual(2, (int)RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageRuntimeKind), nameof(JasonQueryDbStorageRuntimeKind.ModernSqlCipher)));
        }

        [TestMethod]
        public void Route_LegacyPair_IsValid()
        {
            var route = new JasonQueryDbStorageRuntimeRoute
            (
                JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                route.StorageFormatVersion
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite,
                route.RuntimeKind
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatContract.LegacyVersion,
                route.PersistedStorageFormatVersion
            );

            Assert.IsTrue(route.IsLegacy);
            Assert.IsFalse(route.IsModern);
        }

        [TestMethod]
        public void Route_ModernPair_IsValid()
        {
            var route = new JasonQueryDbStorageRuntimeRoute
            (
                JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4,
                JasonQueryDbStorageRuntimeKind.ModernSqlCipher
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4,
                route.StorageFormatVersion
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.ModernSqlCipher,
                route.RuntimeKind
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatContract.ModernVersion,
                route.PersistedStorageFormatVersion
            );

            Assert.IsFalse(route.IsLegacy);
            Assert.IsTrue(route.IsModern);
        }

        [TestMethod]
        public void Route_LegacyFormatWithModernRuntime_Throws()
        {
            Assert.ThrowsExactly<InvalidDataException>
            (
                () => new JasonQueryDbStorageRuntimeRoute
                (
                    JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                    JasonQueryDbStorageRuntimeKind.ModernSqlCipher
                )
            );
        }

        [TestMethod]
        public void Route_ModernFormatWithLegacyRuntime_Throws()
        {
            Assert.ThrowsExactly<InvalidDataException>
            (
                () => new JasonQueryDbStorageRuntimeRoute
                (
                    JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4,
                    JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite
                )
            );
        }

        [TestMethod]
        public void Resolve_MissingPersistedVersion_RoutesLegacy()
        {
            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve((int?)null);

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite,
                route.RuntimeKind
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatContract.LegacyVersion,
                route.PersistedStorageFormatVersion
            );
        }

        [TestMethod]
        public void Resolve_ExplicitLegacyVersion_RoutesLegacy()
        {
            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve
            (
                JasonQueryDbStorageFormatContract.LegacyVersion
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite,
                route.RuntimeKind
            );
        }

        [TestMethod]
        public void Resolve_ExplicitModernVersion_RoutesModern()
        {
            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve
            (
                JasonQueryDbStorageFormatContract.ModernVersion
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.ModernSqlCipher,
                route.RuntimeKind
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageFormatContract.ModernVersion,
                route.PersistedStorageFormatVersion
            );
        }

        [TestMethod]
        public void Resolve_ZeroVersion_FailsClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve(0)
            );
        }

        [TestMethod]
        public void Resolve_FutureVersion_FailsClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve(3)
            );
        }

        [TestMethod]
        public void Resolve_NegativeVersion_FailsClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve(-1)
            );
        }

        [TestMethod]
        public void Resolve_NullMetadataStore_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve
                (
                    (IJasonQueryDbSecurityMetadataStore)null
                )
            );
        }

        [TestMethod]
        public void Resolve_MissingMetadata_RoutesLegacyWithoutLoading()
        {
            var metadataStore = new StubMetadataStore
            {
                ExistsValue = false
            };

            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve
            (
                metadataStore
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite,
                route.RuntimeKind
            );

            Assert.AreEqual(0, metadataStore.LoadCallCount);
        }

        [TestMethod]
        public void Resolve_LegacyMetadata_RoutesLegacy()
        {
            var metadataStore = new StubMetadataStore
            {
                ExistsValue = true,
                Metadata = CreateMetadata
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion
                )
            };

            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve
            (
                metadataStore
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite,
                route.RuntimeKind
            );

            Assert.AreEqual(1, metadataStore.LoadCallCount);
        }

        [TestMethod]
        public void Resolve_ModernMetadata_RoutesModern()
        {
            var metadataStore = new StubMetadataStore
            {
                ExistsValue = true,
                Metadata = CreateMetadata
                (
                    JasonQueryDbStorageFormatContract.ModernVersion
                )
            };

            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve
            (
                metadataStore
            );

            Assert.AreEqual
            (
                JasonQueryDbStorageRuntimeKind.ModernSqlCipher,
                route.RuntimeKind
            );

            Assert.AreEqual(1, metadataStore.LoadCallCount);
        }

        [TestMethod]
        public void Resolve_UnknownMetadataVersion_FailsClosed()
        {
            var metadata = CreateMetadata
            (
                JasonQueryDbStorageFormatContract.LegacyVersion
            );

            metadata.StorageFormatVersion = 3;

            var metadataStore = new StubMetadataStore
            {
                ExistsValue = true,
                Metadata = metadata
            };

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve
                (
                    metadataStore
                )
            );

            Assert.AreEqual(1, metadataStore.LoadCallCount);
        }

        [TestMethod]
        public void Resolve_NullLoadedMetadata_FailsClosed()
        {
            var metadataStore = new StubMetadataStore
            {
                ExistsValue = true,
                Metadata = null
            };

            Assert.ThrowsExactly<InvalidDataException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve
                (
                    metadataStore
                )
            );

            Assert.AreEqual(1, metadataStore.LoadCallCount);
        }

        [TestMethod]
        public void Resolve_MetadataLoadFailure_PropagatesWithoutFallback()
        {
            var expected = new InvalidDataException
            (
                "Synthetic metadata failure."
            );

            var metadataStore = new StubMetadataStore
            {
                ExistsValue = true,
                LoadException = expected
            };

            var actual = Assert.ThrowsExactly<InvalidDataException>
            (
                () => JasonQueryDbStorageRuntimeRoutingContract.Resolve
                (
                    metadataStore
                )
            );

            Assert.AreSame(expected, actual);
            Assert.AreEqual(1, metadataStore.LoadCallCount);
        }

        private static JasonQueryDbSecurityMetadata CreateMetadata(int storageFormatVersion)
        {
            var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
            (
                Convert.ToBase64String(new byte[] { 1, 3, 5, 7 })
            );

            metadata.StorageFormatVersion = storageFormatVersion;

            return metadata;
        }

        private sealed class StubMetadataStore : IJasonQueryDbSecurityMetadataStore
        {
            public string MetadataFilePath => "synthetic";

            public bool ExistsValue { get; set; }

            public bool Exists => ExistsValue;

            public JasonQueryDbSecurityMetadata Metadata { get; set; }

            public Exception LoadException { get; set; }

            public int LoadCallCount { get; private set; }

            public JasonQueryDbSecurityMetadata Load()
            {
                LoadCallCount++;

                if (LoadException != null)
                {
                    throw LoadException;
                }

                return Metadata;
            }

            public void Save(JasonQueryDbSecurityMetadata metadata)
            {
                throw new NotSupportedException();
            }

            public void Delete()
            {
                throw new NotSupportedException();
            }
        }
    }
}
