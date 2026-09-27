using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationLogicalStreamContractTests
    {
        [TestMethod]
        public void FrameKinds_AreFrozen()
        {
            Assert.AreEqual(3, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginDatabase))));
            Assert.AreEqual(4, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginSchema))));
            Assert.AreEqual(5, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginTable))));
            Assert.AreEqual(6, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.Column))));
            Assert.AreEqual(7, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndTable))));
            Assert.AreEqual(8, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndSchema))));
            Assert.AreEqual(9, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginRows))));
            Assert.AreEqual(10, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginRow))));
            Assert.AreEqual(11, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.NullValue))));
            Assert.AreEqual(12, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.Int64Value))));
            Assert.AreEqual(13, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.DoubleValue))));
            Assert.AreEqual(14, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginTextUtf8))));
            Assert.AreEqual(15, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.TextUtf8Chunk))));
            Assert.AreEqual(16, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndTextUtf8))));
            Assert.AreEqual(17, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginBlob))));
            Assert.AreEqual(18, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BlobChunk))));
            Assert.AreEqual(19, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndBlob))));
            Assert.AreEqual(20, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndRow))));
            Assert.AreEqual(21, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndRows))));
            Assert.AreEqual(22, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginSequenceState))));
            Assert.AreEqual(23, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.SequenceEntry))));
            Assert.AreEqual(24, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndSequenceState))));
            Assert.AreEqual(25, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.BeginSecondarySchema))));
            Assert.AreEqual(26, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.SchemaObject))));
            Assert.AreEqual(27, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndSecondarySchema))));
            Assert.AreEqual(28, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndDatabase))));
            Assert.AreEqual(29, Convert.ToInt32(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalFrameKind), nameof(JasonQueryDbStorageMigrationLogicalFrameKind.EndStream))));
        }

        [TestMethod]
        public void Limits_AreFrozen()
        {
            Assert.AreEqual(10000, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxSchemaObjects)));
            Assert.AreEqual(4096, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)));
            Assert.AreEqual(4096, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxColumnsPerTable)));
            Assert.AreEqual(4096, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxFieldsPerRow)));
            Assert.AreEqual(32768, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes)));
            Assert.AreEqual(4 * 1024 * 1024, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes)));
            Assert.AreEqual(1000000000L, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxTextUtf8Bytes)));
            Assert.AreEqual(1000000000L, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxBlobBytes)));
            Assert.AreEqual(64 * 1024, RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.MaxValueChunkBytes)));
            Assert.IsFalse((bool)RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.HasHardRowCountLimit)));
        }

        [TestMethod]
        public void LogicalStream_UsesExistingMagicAndProtocolVersion()
        {
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.Magic)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.Magic)) );
            Assert.AreEqual(RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationWireProtocol), nameof(JasonQueryDbStorageMigrationWireProtocol.CurrentVersion)), RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageMigrationLogicalStreamContract), nameof(JasonQueryDbStorageMigrationLogicalStreamContract.CurrentVersion)) );
        }

        [TestMethod]
        public void SourceEncoding_AllowsUtf8Only()
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSourceEncoding("UTF-8");
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSourceEncoding("utf-8");

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSourceEncoding("UTF-16")
            );
        }

        [TestMethod]
        public void UnknownKinds_FailClosed()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedFrameKind
                (
                    (JasonQueryDbStorageMigrationLogicalFrameKind)255
                )
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedSchemaObjectKind
                (
                    (JasonQueryDbStorageMigrationSchemaObjectKind)255
                )
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedValueKind
                (
                    (JasonQueryDbStorageMigrationValueKind)255
                )
            );
        }

        [TestMethod]
        public void SecondarySchema_RejectsTableKind()
        {
            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedSecondarySchemaObjectKind
                (
                    JasonQueryDbStorageMigrationSchemaObjectKind.Table
                )
            );
        }

        [TestMethod]
        public void SecondarySchemaReplayOrder_IsIndexViewTrigger()
        {
            Assert.AreEqual
            (
                1,
                JasonQueryDbStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder
                (
                    JasonQueryDbStorageMigrationSchemaObjectKind.Index
                )
            );

            Assert.AreEqual
            (
                2,
                JasonQueryDbStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder
                (
                    JasonQueryDbStorageMigrationSchemaObjectKind.View
                )
            );

            Assert.AreEqual
            (
                3,
                JasonQueryDbStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder
                (
                    JasonQueryDbStorageMigrationSchemaObjectKind.Trigger
                )
            );
        }

        [TestMethod]
        public void RowFieldCount_MustMatchColumnCount()
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRowFieldCount(18, 18);

            Assert.ThrowsExactly<InvalidDataException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRowFieldCount(11, 18)
            );
        }

        [TestMethod]
        public void UnsupportedHistoricalTableFeatures_FailClosed()
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
            (
                false,
                false,
                false,
                0,
                10
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    true,
                    false,
                    false,
                    0,
                    10
                )
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    false,
                    true,
                    false,
                    0,
                    10
                )
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
                (
                    false,
                    false,
                    true,
                    0,
                    10
                )
            );

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
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
            var propertyNames = typeof(JasonQueryDbStorageMigrationDatabaseMetadata).GetProperties(BindingFlags.Public | BindingFlags.Instance)
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
            var table = new JasonQueryDbStorageMigrationTableDefinition
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
            Assert.Contains("AUTOINCREMENT", table.CreateSql);
        }

        [TestMethod]
        public void SequenceEntry_IsSeparateFromSchemaObjectContract()
        {
            var entry = new JasonQueryDbStorageMigrationSequenceEntry("SQLHistory", 49821);

            Assert.AreEqual("SQLHistory", entry.TableName);
            Assert.AreEqual(49821L, entry.SequenceValue);

            Assert.IsFalse
            (
                Enum.GetNames(typeof(JasonQueryDbStorageMigrationSchemaObjectKind))
                    .Any(name => string.Equals(name, "Sequence", StringComparison.Ordinal))
            );
        }
    }
}
