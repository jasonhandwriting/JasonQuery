using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationContractTests
    {
        [TestMethod]
        public void StagePersistedValues_AreStable()
        {
            Assert.AreEqual(1, (int)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationStage), nameof(JasonQueryDbStorageMigrationStage.PreparingCandidate)));
            Assert.AreEqual(2, (int)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationStage), nameof(JasonQueryDbStorageMigrationStage.CandidateReady)));
            Assert.AreEqual(3, (int)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationStage), nameof(JasonQueryDbStorageMigrationStage.ReplacePrepared)));
            Assert.AreEqual(4, (int)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationStage), nameof(JasonQueryDbStorageMigrationStage.DatabaseReplaced)));
            Assert.AreEqual(5, (int)JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationStage), nameof(JasonQueryDbStorageMigrationStage.MetadataCommitted)));
        }

        [TestMethod]
        public void ValueKindPersistedValues_AreStable()
        {
            Assert.AreEqual(0, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationValueKind), nameof(JasonQueryDbStorageMigrationValueKind.Null))));
            Assert.AreEqual(1, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationValueKind), nameof(JasonQueryDbStorageMigrationValueKind.Int64))));
            Assert.AreEqual(2, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationValueKind), nameof(JasonQueryDbStorageMigrationValueKind.Double))));
            Assert.AreEqual(3, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationValueKind), nameof(JasonQueryDbStorageMigrationValueKind.TextUtf8))));
            Assert.AreEqual(4, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationValueKind), nameof(JasonQueryDbStorageMigrationValueKind.Blob))));
        }

        [TestMethod]
        public void SchemaObjectKindPersistedValues_AreStable()
        {
            Assert.AreEqual(1, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationSchemaObjectKind), nameof(JasonQueryDbStorageMigrationSchemaObjectKind.Table))));
            Assert.AreEqual(2, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationSchemaObjectKind), nameof(JasonQueryDbStorageMigrationSchemaObjectKind.Index))));
            Assert.AreEqual(3, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationSchemaObjectKind), nameof(JasonQueryDbStorageMigrationSchemaObjectKind.View))));
            Assert.AreEqual(4, System.Convert.ToInt32(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationSchemaObjectKind), nameof(JasonQueryDbStorageMigrationSchemaObjectKind.Trigger))));
        }

        [TestMethod]
        public void ProtocolContract_DefinesLegacyToModernRoute()
        {
            Assert.AreEqual("JQSM", JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.Magic)));
            Assert.AreEqual(1, JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.CurrentVersion)));
            Assert.AreEqual(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.LegacyVersion)), JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.SourceStorageFormatVersion)));
            Assert.AreEqual(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.ModernVersion)), JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationProtocolContract), nameof(JasonQueryDbStorageMigrationProtocolContract.TargetStorageFormatVersion)));

            JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedVersion(1);
            JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedRoute(1, 2);
        }

        [TestMethod]
        public void ProtocolContract_UnknownVersion_FailsClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedVersion(2)
            );
        }

        [TestMethod]
        public void ProtocolContract_UnsupportedRoute_FailsClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedRoute(2, 1)
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedRoute(1, 3)
            );
        }

        [TestMethod]
        public void Journal_ValidLegacyToModernMigration_PassesValidation()
        {
            CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion).Validate();
        }

        [TestMethod]
        public void Journal_MissingSourceMarker_IsHistoricalLegacy()
        {
            CreateValidJournal(null).Validate();
        }

        [TestMethod]
        public void Journal_TargetMetadataMustExplicitlyIdentifyModernStorage()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.TargetMetadata.StorageFormatVersion = null;

            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_SourceMetadataMustResolveToDeclaredLegacyStorage()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.SourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;

            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_PhysicalMigrationCannotChangeSecurityIdentity()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.TargetMetadata.ProtectedDatabaseKey = Convert.ToBase64String(new byte[] { 9, 8, 7, 6 });

            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_UnknownStage_FailsClosed()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.Stage = (JasonQueryDbStorageMigrationStage)99;

            Assert.ThrowsExactly<NotSupportedException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_UnsupportedProtocolVersion_FailsClosed()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.ProtocolVersion = 2;

            Assert.ThrowsExactly<NotSupportedException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_OperationIdMustUseCanonicalNFormat()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.OperationId = Guid.NewGuid().ToString("D");

            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_UnsupportedCandidateWriterProtocolVersion_FailsClosed()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.CandidateWriterProtocolVersion = JasonQueryDbStorageV2CandidateWriterProtocol.CurrentVersion + 1;

            Assert.ThrowsExactly<NotSupportedException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_PreparingCandidate_MustNotPersistCandidateHash()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.CandidateDatabaseSha256 = new string('D', 64);

            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_CandidateReady_RequiresCandidateHash()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.Stage = JasonQueryDbStorageMigrationStage.CandidateReady;

            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_CandidateReady_WithCanonicalCandidateHash_PassesValidation()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.Stage = JasonQueryDbStorageMigrationStage.CandidateReady;
            journal.CandidateDatabaseSha256 = new string('D', 64);

            journal.Validate();
        }

        [TestMethod]
        public void Journal_Sha256FieldsRequireCanonicalUppercaseHex()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.SourceDatabaseSha256 = new string('a', 64);
            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_OperationIdMustUseCanonicalLowercaseNFormat()
        {
            var journal = CreateValidJournal(JasonQueryDbStorageFormatContract.LegacyVersion);

            journal.OperationId = Guid.NewGuid().ToString("N").ToUpperInvariant();
            Assert.ThrowsExactly<InvalidOperationException>(() => journal.Validate());
        }
        [TestMethod]
        public void Journal_TopLevelContractContainsNoPasswordOrDatabaseKeyField()
        {
            var propertyNames = typeof(JasonQueryDbStorageMigrationJournal).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => property.Name).ToArray();

            Assert.IsFalse
            (
                propertyNames.Any(name => name.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0)
            );

            Assert.IsFalse
            (
                propertyNames.Any(name => name.IndexOf("DatabaseKey", StringComparison.OrdinalIgnoreCase) >= 0)
            );
        }

        private static JasonQueryDbStorageMigrationJournal CreateValidJournal(int? sourceStorageFormatVersion)
        {
            var protectedDatabaseKey = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var sourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(protectedDatabaseKey);

            sourceMetadata.StorageFormatVersion = sourceStorageFormatVersion;

            var targetMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(protectedDatabaseKey);

            targetMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;

            return new JasonQueryDbStorageMigrationJournal
            {
                JournalVersion = JasonQueryDbStorageMigrationJournal.CurrentVersion,
                OperationId = Guid.NewGuid().ToString("N"),
                Stage = JasonQueryDbStorageMigrationStage.PreparingCandidate,
                ProtocolVersion = JasonQueryDbStorageMigrationProtocolContract.CurrentVersion,
                CandidateWriterProtocolVersion = JasonQueryDbStorageV2CandidateWriterProtocol.CurrentVersion,
                SourceStorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion,
                TargetStorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion,
                SourceDatabaseSha256 = new string('A', 64),
                SourceMetadataSha256 = new string('B', 64),
                TargetMetadataSha256 = new string('C', 64),
                SourceMetadata = sourceMetadata,
                TargetMetadata = targetMetadata
            };
        }
    }
}
