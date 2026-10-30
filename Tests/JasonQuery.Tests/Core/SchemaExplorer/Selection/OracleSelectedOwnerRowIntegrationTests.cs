using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.SchemaExplorer.Selection
{
    [TestClass]
    public sealed class OracleSelectedOwnerRowIntegrationTests
    {
        private sealed class FakeRowReader : ISchemaExplorerRowReader
        {
            private readonly DataTable _table;

            internal FakeRowReader(DataTable table)
            {
                _table = table;
            }

            public bool IsGroupRow(int index) { return false; }
            public int GetGroupLevel(int index) { return -1; }
            public string GetGroupedText(int index) { return string.Empty; }
            public string GetSchemaTypeFromGroupStartIndex(int index) { return string.Empty; }
            public string FindPreviousGroupText(int index, int level) { return string.Empty; }
            public DataRow GetDataRow(int index) { return index >= 0 && index < _table.Rows.Count ? _table.Rows[index] : null; }
            public DataTable GetSchemaDataTable() { return _table; }
            public void SetCurrentRow(int index) { }
        }

        private static DataTable Table(string firstOwner, string secondOwner = null)
        {
            var t = new DataTable();

            t.Columns.Add("SchemaType", typeof(string));
            t.Columns.Add("SchemaName", typeof(string));
            t.Columns.Add("Schema_Browser", typeof(string));
            t.Columns.Add("OracleMetadataOwner", typeof(string));
            t.Rows.Add("Tables", "AABBCC", "AABBCC", firstOwner ?? string.Empty);

            if (secondOwner != null)
            {
                t.Rows.Add("Tables", "AABBCC", "AABBCC", secondOwner);
            }

            return t;
        }

        private static SchemaExplorerSelectionResolveRequest Request(string sessionUser, string suppliedOwner = "")
        {
            return new SchemaExplorerSelectionResolveRequest
            {
                SourceType = DataSourceType.Oracle,
                DisplayRowIndex = 0,
                RequireOracleOwnerIdentity = true,
                OracleVerifiedSessionUser = sessionUser,
                OracleMetadataOwner = suppliedOwner
            };
        }

        private static string ResolveGroupOwner(DataTable table)
        {
            //The C1 group-specific helper is tested directly to avoid binding a commercial UI control.
            var method = typeof(SchemaExplorerSelectionResolver).GetMethod
            (
                "ResolveOracleMetadataOwnerFromRow",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method, "Oracle Group Row provenance resolver is missing.");

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    Request("SYS"),
                    new SchemaExplorerSelectionInfo
                    {
                        SourceType = DataSourceType.Oracle,
                        SchemaType = "Tables",
                        SchemaName = "AABBCC",
                        IsGroupRow = true
                    },
                    new FakeRowReader(table)
                }
            );
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G01_NonOracleLegacySelection_Unaffected()
        {
            var request = Request("SYS");

            request.SourceType = DataSourceType.PostgreSql;
            request.OracleMetadataOwner = string.Empty;

            var result = SchemaExplorerSelectionResolver.Resolve(request, new FakeRowReader(Table("SYS")));

            Assert.AreEqual(SchemaExplorerSelectionKind.SchemaObject, result.Kind);
            Assert.IsFalse(result.HasQualifiedOracleSelectedOwner);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G02_DataRow_SysOwnerReadFromMetadata()
        {
            var result = SchemaExplorerSelectionResolver.Resolve(Request("SYS"), new FakeRowReader(Table("SYS")));

            Assert.AreEqual(SchemaExplorerSelectionKind.SchemaObject, result.Kind);
            Assert.AreEqual("SYS", result.OracleSelectedOwner);
            Assert.IsTrue(result.HasQualifiedOracleSelectedOwner);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G03_DataRow_QuotedMixedCasePreserved()
        {
            var result = SchemaExplorerSelectionResolver.Resolve(Request("MiXeD"), new FakeRowReader(Table("MiXeD")));

            Assert.AreEqual("MiXeD", result.OracleSelectedOwner);
            Assert.AreEqual("MiXeD", result.SchemaNode);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G04_InternalRequestOwnerCannotSpoofActualRow()
        {
            var result = SchemaExplorerSelectionResolver.Resolve(Request("SYS", "SYS"), new FakeRowReader(Table("SYSTEM")));

            Assert.AreEqual(SchemaExplorerSelectionKind.Invalid, result.Kind);
            Assert.IsFalse(result.HasQualifiedOracleSelectedOwner);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G05_GroupRow_ConsistentMetadataOwnerRecognized()
        {
            Assert.AreEqual("SYS", ResolveGroupOwner(Table("SYS", "SYS")));
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G06_GroupRow_ConflictingOwnersFailClosed()
        {
            Assert.AreEqual(string.Empty, ResolveGroupOwner(Table("SYS", "SYSTEM")));
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G07_GroupRow_MissingOwnerFailsClosed()
        {
            Assert.AreEqual(string.Empty, ResolveGroupOwner(Table("")));
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G08_SnapshotCarriesVerifiedConnectionId()
        {
            Assert.IsNotNull(typeof(SharedSchemaMetadataSnapshot).GetProperty("OracleConnectionId"));
            Assert.IsNotNull(typeof(SharedSchemaMetadataSnapshot).GetProperty("OracleVerifiedSessionUser"));
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G2")]
        public void F02B2G2_G09_ReaderExposesVerifiedConnectionId()
        {
            var property = typeof(OracleReader).GetProperty("VerifiedConnectionId");

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(Guid), property.PropertyType);
        }
    }
}
