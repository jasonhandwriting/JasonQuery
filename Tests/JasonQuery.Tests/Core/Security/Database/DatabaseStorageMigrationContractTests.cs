using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseStorageMigrationContractTests
    {
        [TestMethod]
        public void StagePersistedValues_AreStable()
        {
            Assert.AreEqual(1, (int)DatabaseStorageMigrationStage.PreparingCandidate);
            Assert.AreEqual(2, (int)DatabaseStorageMigrationStage.CandidateReady);
            Assert.AreEqual(3, (int)DatabaseStorageMigrationStage.ReplacePrepared);
            Assert.AreEqual(4, (int)DatabaseStorageMigrationStage.DatabaseReplaced);
            Assert.AreEqual(5, (int)DatabaseStorageMigrationStage.MetadataCommitted);
        }

        [TestMethod]
        public void ValueKindPersistedValues_AreStable()
        {
            Assert.AreEqual(0, (int)DatabaseStorageMigrationValueKind.Null);
            Assert.AreEqual(1, (int)DatabaseStorageMigrationValueKind.Int64);
            Assert.AreEqual(2, (int)DatabaseStorageMigrationValueKind.Double);
            Assert.AreEqual(3, (int)DatabaseStorageMigrationValueKind.TextUtf8);
            Assert.AreEqual(4, (int)DatabaseStorageMigrationValueKind.Blob);
        }

        [TestMethod]
        public void SchemaObjectKindPersistedValues_AreStable()
        {
            Assert.AreEqual(1, (int)DatabaseStorageMigrationSchemaObjectKind.Table);
            Assert.AreEqual(2, (int)DatabaseStorageMigrationSchemaObjectKind.Index);
            Assert.AreEqual(3, (int)DatabaseStorageMigrationSchemaObjectKind.View);
            Assert.AreEqual(4, (int)DatabaseStorageMigrationSchemaObjectKind.Trigger);
        }

        [TestMethod]
        public void ProtocolContract_DefinesLegacyToModernRoute()
        {
            Assert.AreEqual("JQSM", DatabaseStorageMigrationProtocolContract.Magic);
            Assert.AreEqual(1, DatabaseStorageMigrationProtocolContract.CurrentVersion);
            Assert.AreEqual(DatabaseStorageFormatContract.LegacyVersion,
                            DatabaseStorageMigrationProtocolContract.SourceStorageFormatVersion);
            Assert.AreEqual(DatabaseStorageFormatContract.ModernVersion,
                            DatabaseStorageMigrationProtocolContract.TargetStorageFormatVersion);

            DatabaseStorageMigrationProtocolContract.EnsureSupportedVersion(1);
            DatabaseStorageMigrationProtocolContract.EnsureSupportedRoute(1, 2);
        }

        [TestMethod]
        public void ProtocolContract_UnknownVersion_FailsClosed()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationProtocolContract.EnsureSupportedVersion(2)
            );
        }

        [TestMethod]
        public void ProtocolContract_UnsupportedRoute_FailsClosed()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationProtocolContract.EnsureSupportedRoute(2, 1)
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationProtocolContract.EnsureSupportedRoute(1, 3)
            );
        }

        [TestMethod]
        public void Journal_ValidLegacyToModernMigration_PassesValidation()
        {
            CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion).Validate();
        }

        [TestMethod]
        public void Journal_MissingSourceMarker_IsHistoricalLegacy()
        {
            CreateValidJournal(null).Validate();
        }

        [TestMethod]
        public void Journal_TargetMetadataMustExplicitlyIdentifyModernStorage()
        {
            var journal = CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion);

            journal.TargetMetadata.StorageFormatVersion = null;

            Assert.ThrowsException<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_SourceMetadataMustResolveToDeclaredLegacyStorage()
        {
            var journal = CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion);

            journal.SourceMetadata.StorageFormatVersion = DatabaseStorageFormatContract.ModernVersion;

            Assert.ThrowsException<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_PhysicalMigrationCannotChangeSecurityIdentity()
        {
            var journal = CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion);

            journal.TargetMetadata.ProtectedDatabaseKey = Convert.ToBase64String(new byte[] { 9, 8, 7, 6 });

            Assert.ThrowsException<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_UnknownStage_FailsClosed()
        {
            var journal = CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion);

            journal.Stage = (DatabaseStorageMigrationStage)99;

            Assert.ThrowsException<NotSupportedException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_UnsupportedProtocolVersion_FailsClosed()
        {
            var journal = CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion);

            journal.ProtocolVersion = 2;

            Assert.ThrowsException<NotSupportedException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_OperationIdMustUseCanonicalNFormat()
        {
            var journal = CreateValidJournal(DatabaseStorageFormatContract.LegacyVersion);

            journal.OperationId = Guid.NewGuid().ToString("D");

            Assert.ThrowsException<InvalidOperationException>(() => journal.Validate());
        }

        [TestMethod]
        public void Journal_TopLevelContractContainsNoPasswordOrDatabaseKeyField()
        {
            var propertyNames = typeof(DatabaseStorageMigrationJournal).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => property.Name).ToArray();

            Assert.IsFalse
            (
                propertyNames.Any(name => name.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0)
            );

            Assert.IsFalse
            (
                propertyNames.Any(name => name.IndexOf("DatabaseKey", StringComparison.OrdinalIgnoreCase) >= 0)
            );
        }

        private static DatabaseStorageMigrationJournal CreateValidJournal(int? sourceStorageFormatVersion)
        {
            var protectedDatabaseKey = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var sourceMetadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser(protectedDatabaseKey);

            sourceMetadata.StorageFormatVersion = sourceStorageFormatVersion;

            var targetMetadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser(protectedDatabaseKey);

            targetMetadata.StorageFormatVersion = DatabaseStorageFormatContract.ModernVersion;

            return new DatabaseStorageMigrationJournal
            {
                JournalVersion = DatabaseStorageMigrationJournal.CurrentVersion,
                OperationId = Guid.NewGuid().ToString("N"),
                Stage = DatabaseStorageMigrationStage.PreparingCandidate,
                ProtocolVersion = DatabaseStorageMigrationProtocolContract.CurrentVersion,
                SourceStorageFormatVersion = DatabaseStorageFormatContract.LegacyVersion,
                TargetStorageFormatVersion = DatabaseStorageFormatContract.ModernVersion,
                SourceMetadata = sourceMetadata,
                TargetMetadata = targetMetadata
            };
        }
    }
}
