using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.Forms;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class OracleSharedColumnDdlBridgeGoldenTests
    {
        private static readonly Guid ConnectionId = Guid.Parse("B8D03620-7693-413F-9249-91594A3A71A7");

        private static SchemaExplorerSelectionInfo Column(string caption = "DISPLAY, VARCHAR2(20)")
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = DataSourceType.Oracle,
                Kind = SchemaExplorerSelectionKind.ColumnInfo,
                SchemaType = "Tables",
                SchemaName = caption,
                IsColumnInfoRow = true,
                DisplayRowIndex = 7
            };
        }

        private static DataRow Raw(string owner = "SYS", string tableName = "ABC", string columnName = "RAW_COL", bool includeRaw = true)
        {
            var table = new DataTable();

            table.Columns.Add("OracleMetadataOwner", typeof(string));

            if (includeRaw)
            {
                table.Columns.Add("OracleMetadataTableName", typeof(string));
                table.Columns.Add("OracleMetadataColumnName", typeof(string));

                return table.Rows.Add(owner, tableName, columnName);
            }

            return table.Rows.Add(owner);
        }

        private static Type Bridge()
        {
            var type = typeof(EditColumnForm).Assembly.GetType("JasonQuery.UI.LeftPanel.OracleSharedColumnDdlBridgePolicy");

            Assert.IsNotNull(type, "Missing B3-B2-C2 OracleSharedColumnDdlBridgePolicy");
            return type;
        }

        private static bool Qualify(SchemaExplorerSelectionInfo s, DataRow row, string user = "SYS")
        {
            var method = Bridge().GetMethod("TryQualifySelection", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);
            return (bool)method.Invoke(null, new object[] { s, row, user });
        }

        private static string RawIdentity(SchemaExplorerSelectionInfo s, string property)
        {
            var p = typeof(SchemaExplorerSelectionInfo).GetProperty(property);
            Assert.IsNotNull(p, "Missing B3-B2-C2 selection property: " + property);
            return (string)p.GetValue(s);
        }

        private static bool Create(SchemaExplorerSelectionInfo s, string snapshotUser, Guid snapshotId, string currentUser, Guid currentId, out OracleDdlTargetIdentity identity)
        {
            var method = Bridge().GetMethod("TryCreate", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);

            var args = new object[] { s, snapshotUser, snapshotId, currentUser, currentId, null };
            var result = (bool)method.Invoke(null, args);

            identity = (OracleDdlTargetIdentity)args[5];
            return result;
        }

        private static bool Same(SchemaExplorerSelectionInfo before, SchemaExplorerSelectionInfo after)
        {
            var method = Bridge().GetMethod("IsSameColumnDataRow", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);
            return (bool)method.Invoke(null, new object[] { before, after });
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G01_UnqualifiedColumnCommandsStayDisabled()
        {
            Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, Column(), SharedSchemaExplorerMenuCommand.DropColumn));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G02_NonOracleColumnCommandsStayDisabled()
        {
            var s = Column();

            s.SourceType = DataSourceType.PostgreSql;

            Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G03_ExistingQualifiedTableCommandsStillOffered()
        {
            var s = new SchemaExplorerSelectionInfo
            {
                SourceType = DataSourceType.Oracle,
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SchemaType = "Tables",
                SchemaName = "ABC",
                HasQualifiedOracleSelectedOwner = true,
                OracleSelectedOwner = "SYS"
            };

            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G04_GroupColumnCommandsStayDisabled()
        {
            var s = Column();

            s.IsGroupRow = true;

            Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.DropColumn));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G05_QualifiedRawOracleColumnIsExact()
        {
            var s = Column("DISPLAY, WRONG");

            Assert.IsTrue(Qualify(s, Raw("SYS", "Table, One", "A, B\"C")));
            Assert.AreEqual("Table, One", RawIdentity(s, "OracleMetadataTableName"));
            Assert.AreEqual("A, B\"C", RawIdentity(s, "OracleMetadataColumnName"));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G06_MixedCaseOwnerAndIdentifiersStayExact()
        {
            var s = Column();

            Assert.IsTrue(Qualify(s, Raw("MiXeD", "MixedTable", "MiXeD_Column"), "MiXeD"));
            Assert.AreEqual("MixedTable", RawIdentity(s, "OracleMetadataTableName"));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G07_MetadataOwnerDoesNotMatchVerifiedSession()
        {
            Assert.IsFalse(Qualify(Column(), Raw("SYSTEM", "ABC", "RAW_COL"), "SYS"));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G08_MissingRawMetadataFailsClosed()
        {
            Assert.IsFalse(Qualify(Column(), Raw(includeRaw: false)));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G09_ColumnCaptionIsNeverUsedAsRawIdentity()
        {
            var s = Column("FAKE, NUMBER(22)");

            Assert.IsTrue(Qualify(s, Raw("SYS", "ABC", "ACTUAL")));
            Assert.AreEqual("ACTUAL", RawIdentity(s, "OracleMetadataColumnName"));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G10_QualifiedColumnAddEnabled()
        {
            var s = Column();

            Assert.IsTrue(Qualify(s, Raw()));
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G11_QualifiedColumnCommentDropRenameEnabled()
        {
            var s = Column();

            Assert.IsTrue(Qualify(s, Raw()));
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.CommentColumn));
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.DropColumn));
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.RenameColumn));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G12_SameUserReconnectFailsClosed()
        {
            var s = Column();

            Assert.IsTrue(Qualify(s, Raw()));
            Assert.IsFalse(Create(s, "SYS", ConnectionId, "SYS", Guid.NewGuid(), out _));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G13_SessionCaseMismatchFailsClosed()
        {
            var s = Column();

            Assert.IsTrue(Qualify(s, Raw("MiXeD"), "MiXeD"));
            Assert.IsFalse(Create(s, "MiXeD", ConnectionId, "MIXED", ConnectionId, out _));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G14_SameRawRowAcceptedAtClickTime()
        {
            var a = Column();
            var b = Column();

            Assert.IsTrue(Qualify(a, Raw()));
            Assert.IsTrue(Qualify(b, Raw()));
            Assert.IsTrue(Same(a, b));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G15_ChangedRawColumnRejectedAtClickTime()
        {
            var a = Column();
            var b = Column();

            Assert.IsTrue(Qualify(a, Raw()));
            Assert.IsTrue(Qualify(b, Raw(columnName: "OTHER")));
            Assert.IsFalse(Same(a, b));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G16_ChangedRawTableRejectedAtClickTime()
        {
            var a = Column();
            var b = Column();

            Assert.IsTrue(Qualify(a, Raw()));
            Assert.IsTrue(Qualify(b, Raw(tableName: "OTHER")));
            Assert.IsFalse(Same(a, b));
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G17_MainFormHasClickTimeColumnHandler()
        {
            var method = typeof(MainForm).GetMethod("TryOpenSharedOracleColumnDdl", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(bool), method.ReturnType);
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G18_QualifiedColumnIdentityCreatesTrustedOwner()
        {
            var s = Column();

            Assert.IsTrue(Qualify(s, Raw()));
            Assert.IsTrue(Create(s, "SYS", ConnectionId, "SYS", ConnectionId, out var identity));
            Assert.AreEqual("SYS", identity.MetadataOwner);
        }

        [TestMethod, TestCategory("F02B3B2C2")]
        public void G19_UnknownRawColumnFailsClosed()
        {
            Assert.IsFalse(Qualify(Column(), Raw(columnName: string.Empty)));
        }
    }
}
