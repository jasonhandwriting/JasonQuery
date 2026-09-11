using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseStorageMigrationLogicalStreamContractTests
    {
        [TestMethod]
        public void FrameKinds_AreFrozen()
        {
            Assert.AreEqual(3, (int)DatabaseStorageMigrationLogicalFrameKind.BeginDatabase);
            Assert.AreEqual(4, (int)DatabaseStorageMigrationLogicalFrameKind.BeginSchema);
            Assert.AreEqual(5, (int)DatabaseStorageMigrationLogicalFrameKind.BeginTable);
            Assert.AreEqual(6, (int)DatabaseStorageMigrationLogicalFrameKind.Column);
            Assert.AreEqual(7, (int)DatabaseStorageMigrationLogicalFrameKind.EndTable);
            Assert.AreEqual(8, (int)DatabaseStorageMigrationLogicalFrameKind.EndSchema);
            Assert.AreEqual(9, (int)DatabaseStorageMigrationLogicalFrameKind.BeginRows);
            Assert.AreEqual(10, (int)DatabaseStorageMigrationLogicalFrameKind.BeginRow);
            Assert.AreEqual(11, (int)DatabaseStorageMigrationLogicalFrameKind.NullValue);
            Assert.AreEqual(12, (int)DatabaseStorageMigrationLogicalFrameKind.Int64Value);
            Assert.AreEqual(13, (int)DatabaseStorageMigrationLogicalFrameKind.DoubleValue);
            Assert.AreEqual(14, (int)DatabaseStorageMigrationLogicalFrameKind.BeginTextUtf8);
            Assert.AreEqual(15, (int)DatabaseStorageMigrationLogicalFrameKind.TextUtf8Chunk);
            Assert.AreEqual(16, (int)DatabaseStorageMigrationLogicalFrameKind.EndTextUtf8);
            Assert.AreEqual(17, (int)DatabaseStorageMigrationLogicalFrameKind.BeginBlob);
            Assert.AreEqual(18, (int)DatabaseStorageMigrationLogicalFrameKind.BlobChunk);
            Assert.AreEqual(19, (int)DatabaseStorageMigrationLogicalFrameKind.EndBlob);
            Assert.AreEqual(20, (int)DatabaseStorageMigrationLogicalFrameKind.EndRow);
            Assert.AreEqual(21, (int)DatabaseStorageMigrationLogicalFrameKind.EndRows);
            Assert.AreEqual(22, (int)DatabaseStorageMigrationLogicalFrameKind.BeginSequenceState);
            Assert.AreEqual(23, (int)DatabaseStorageMigrationLogicalFrameKind.SequenceEntry);
            Assert.AreEqual(24, (int)DatabaseStorageMigrationLogicalFrameKind.EndSequenceState);
            Assert.AreEqual(25, (int)DatabaseStorageMigrationLogicalFrameKind.BeginSecondarySchema);
            Assert.AreEqual(26, (int)DatabaseStorageMigrationLogicalFrameKind.SchemaObject);
            Assert.AreEqual(27, (int)DatabaseStorageMigrationLogicalFrameKind.EndSecondarySchema);
            Assert.AreEqual(28, (int)DatabaseStorageMigrationLogicalFrameKind.EndDatabase);
            Assert.AreEqual(29, (int)DatabaseStorageMigrationLogicalFrameKind.EndStream);
        }

        [TestMethod]
        public void Limits_AreFrozen()
        {
            Assert.AreEqual(10000, DatabaseStorageMigrationLogicalStreamContract.MaxSchemaObjects);
            Assert.AreEqual(4096, DatabaseStorageMigrationLogicalStreamContract.MaxTables);
            Assert.AreEqual(4096, DatabaseStorageMigrationLogicalStreamContract.MaxColumnsPerTable);
            Assert.AreEqual(4096, DatabaseStorageMigrationLogicalStreamContract.MaxFieldsPerRow);
            Assert.AreEqual(32768, DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes);
            Assert.AreEqual(4 * 1024 * 1024, DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes);
            Assert.AreEqual(1000000000L, DatabaseStorageMigrationLogicalStreamContract.MaxTextUtf8Bytes);
            Assert.AreEqual(1000000000L, DatabaseStorageMigrationLogicalStreamContract.MaxBlobBytes);
            Assert.AreEqual(64 * 1024, DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes);
            Assert.IsFalse(DatabaseStorageMigrationLogicalStreamContract.HasHardRowCountLimit);
        }

        [TestMethod]
        public void LogicalStream_UsesExistingMagicAndProtocolVersion()
        {
            Assert.AreEqual
            (
                DatabaseStorageMigrationWireProtocol.Magic,
                DatabaseStorageMigrationLogicalStreamContract.Magic
            );

            Assert.AreEqual
            (
                DatabaseStorageMigrationWireProtocol.CurrentVersion,
                DatabaseStorageMigrationLogicalStreamContract.CurrentVersion
            );
        }

        [TestMethod]
        public void SourceEncoding_AllowsUtf8Only()
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureSourceEncoding("UTF-8");
            DatabaseStorageMigrationLogicalStreamContract.EnsureSourceEncoding("utf-8");

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSourceEncoding("UTF-16")
            );
        }

        [TestMethod]
        public void UnknownKinds_FailClosed()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedFrameKind
                (
                    (DatabaseStorageMigrationLogicalFrameKind)255
                )
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedSchemaObjectKind
                (
                    (DatabaseStorageMigrationSchemaObjectKind)255
                )
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedValueKind
                (
                    (DatabaseStorageMigrationValueKind)255
                )
            );
        }

        [TestMethod]
        public void SecondarySchema_RejectsTableKind()
        {
            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedSecondarySchemaObjectKind
                (
                    DatabaseStorageMigrationSchemaObjectKind.Table
                )
            );
        }

        [TestMethod]
        public void SecondarySchemaReplayOrder_IsIndexViewTrigger()
        {
            Assert.AreEqual
            (
                1,
                DatabaseStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder
                (
                    DatabaseStorageMigrationSchemaObjectKind.Index
                )
            );
            Assert.AreEqual
            (
                2,
                DatabaseStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder
                (
                    DatabaseStorageMigrationSchemaObjectKind.View
                )
            );
            Assert.AreEqual
            (
                3,
                DatabaseStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder
                (
                    DatabaseStorageMigrationSchemaObjectKind.Trigger
                )
            );
        }

        [TestMethod]
        public void RowFieldCount_MustMatchColumnCount()
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureRowFieldCount(18, 18);

            Assert.ThrowsException<InvalidDataException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureRowFieldCount(11, 18)
            );
        }

        [TestMethod]
        public void UnsupportedHistoricalTableFeatures_FailClosed()
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
            (
                false,
                false,
                false,
                0,
                10
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    true,
                    false,
                    false,
                    0,
                    10
                )
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    false,
                    true,
                    false,
                    0,
                    10
                )
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    false,
                    false,
                    true,
                    0,
                    10
                )
            );

            Assert.ThrowsException<NotSupportedException>
            (
                () => DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    false,
                    false,
                    false,
                    -1,
                    10
                )
            );
        }

        [TestMethod]
        public void DatabaseMetadata_ContainsLogicalStateOnly()
        {
            var propertyNames = typeof(DatabaseStorageMigrationDatabaseMetadata)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name)
                .OrderBy(name => name)
                .ToArray();

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "ApplicationId",
                    "SourceEncodingName",
                    "UserVersion"
                },
                propertyNames
            );
        }

        [TestMethod]
        public void TableDefinition_PreservesOriginalSqlAndRowIdentity()
        {
            var table = new DatabaseStorageMigrationTableDefinition
            (
                0,
                "SQLHistory",
                "CREATE TABLE SQLHistory (PID INTEGER PRIMARY KEY AUTOINCREMENT, SQL TEXT)",
                2,
                0,
                true
            );

            Assert.AreEqual(0, table.TableId);
            Assert.AreEqual("SQLHistory", table.Name);
            Assert.AreEqual(2, table.ColumnCount);
            Assert.AreEqual(0, table.RowIdAliasColumnCid);
            Assert.IsTrue(table.HasAutoincrement);
            StringAssert.Contains(table.CreateSql, "AUTOINCREMENT");
        }

        [TestMethod]
        public void SequenceEntry_IsSeparateFromSchemaObjectContract()
        {
            var entry = new DatabaseStorageMigrationSequenceEntry("SQLHistory", 49821);

            Assert.AreEqual("SQLHistory", entry.TableName);
            Assert.AreEqual(49821L, entry.SequenceValue);

            Assert.IsFalse
            (
                Enum.GetNames(typeof(DatabaseStorageMigrationSchemaObjectKind))
                    .Any(name => string.Equals(name, "Sequence", StringComparison.Ordinal))
            );
        }
    }
}
