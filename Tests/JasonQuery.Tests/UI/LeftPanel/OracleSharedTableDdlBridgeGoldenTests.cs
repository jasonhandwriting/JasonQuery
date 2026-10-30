using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.Forms;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class OracleSharedTableDdlBridgeGoldenTests
    {
        private static readonly Guid SessionId = new Guid("AB6F6383-09C2-4DF0-9950-06BD4BED0E21");

        private static SchemaExplorerSelectionInfo Selection(string owner = "SYS", string table = "AABBCC")
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = DataSourceType.Oracle,
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SchemaType = "Tables",
                SchemaName = table,
                DisplayRowIndex = 6,
                HasQualifiedOracleSelectedOwner = true,
                OracleSelectedOwner = owner
            };
        }

        private static bool Offered(SchemaExplorerSelectionInfo s, SharedSchemaExplorerMenuCommand cmd)
        {
            return SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, s, cmd);
        }

        private static Type Bridge()
        {
            var type = typeof(EditTableForm).Assembly.GetType("JasonQuery.UI.LeftPanel.OracleSharedTableDdlBridgePolicy");

            Assert.IsNotNull(type, "B3-B2-T bridge is not implemented yet");
            return type;
        }

        private static bool Create(SchemaExplorerSelectionInfo s, string snapshotUser, Guid snapshotId, string currentUser, Guid currentId, out OracleDdlTargetIdentity identity)
        {
            var method = Bridge().GetMethod("TryCreate", BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(method);

            var args = new object[] { s, snapshotUser, snapshotId, currentUser, currentId, null };
            var ok = (bool)method.Invoke(null, args);

            identity = (OracleDdlTargetIdentity)args[5];
            return ok;
        }

        private static bool Same(SchemaExplorerSelectionInfo previous, SchemaExplorerSelectionInfo current)
        {
            var method = Bridge().GetMethod("IsSameTableDataRow", BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(method);
            return (bool)method.Invoke(null, new object[] { previous, current });
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G01_LegacyUnqualifiedTableRemainsDisabled()
        {
            var s = Selection();

            s.HasQualifiedOracleSelectedOwner = false;

            Assert.IsFalse(Offered(s, SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G02_NonOracleTableCommandsRemainDisabled()
        {
            var s = Selection();

            s.SourceType = DataSourceType.PostgreSql;

            Assert.IsFalse(Offered(s, SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G03_ColumnCommandsRemainDisabled()
        {
            Assert.IsFalse(Offered(Selection(), SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G04_GroupRowsRemainDisabled()
        {
            var s = Selection();

            s.IsGroupRow = true;

            Assert.IsFalse(Offered(s, SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G05_QualifiedTableCommentIsOffered()
        {
            Assert.IsTrue(Offered(Selection(), SharedSchemaExplorerMenuCommand.CommentTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G06_QualifiedTableDropIsOffered()
        {
            Assert.IsTrue(Offered(Selection(), SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G07_QualifiedTableRenameIsOffered()
        {
            Assert.IsTrue(Offered(Selection(), SharedSchemaExplorerMenuCommand.RenameTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G08_QualifiedTableTruncateIsOffered()
        {
            Assert.IsTrue(Offered(Selection(), SharedSchemaExplorerMenuCommand.TruncateTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G09_MixedCaseSchemaQualifiedTableIsOffered()
        {
            Assert.IsTrue(Offered(Selection("MiXeD"), SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G10_TrustedSysBridgeUsesExactOwner()
        {
            Assert.IsTrue(Create(Selection(), "SYS", SessionId, "SYS", SessionId, out var identity));
            Assert.AreEqual("SYS", identity.MetadataOwner);
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G11_MixedCaseBridgePreservesOwner()
        {
            Assert.IsTrue(Create(Selection("MiXeD"), "MiXeD", SessionId, "MiXeD", SessionId, out var identity));
            Assert.AreEqual("MiXeD", identity.SelectedOwner);
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G12_SessionCaseMismatchIsRejected()
        {
            Assert.IsFalse(Create(Selection("MiXeD"), "MiXeD", SessionId, "MIXED", SessionId, out _));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G13_SameUserReconnectIsRejected()
        {
            Assert.IsFalse(Create(Selection(), "SYS", SessionId, "SYS", Guid.NewGuid(), out _));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G14_SelectedOwnerMismatchIsRejected()
        {
            Assert.IsFalse(Create(Selection("SYSTEM"), "SYS", SessionId, "SYS", SessionId, out _));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G15_UnqualifiedSelectionIsRejected()
        {
            var s = Selection();

            s.HasQualifiedOracleSelectedOwner = false;

            Assert.IsFalse(Create(s, "SYS", SessionId, "SYS", SessionId, out _));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G16_ChangedTableRowIsRejectedAtClickTime()
        {
            Assert.IsFalse(Same(Selection(), Selection(table: "OTHER")));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G17_UnchangedTableRowIsAcceptedAtClickTime()
        {
            Assert.IsTrue(Same(Selection(), Selection()));
        }

        [TestMethod, TestCategory("F02B3B2T")]
        public void G18_MainFormHasProtectedTableCommandHandler()
        {
            var method = typeof(MainForm).GetMethod("TryOpenSharedOracleTableDdl", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(bool), method.ReturnType);
        }

        //Post-GUI correction: Oracle F7 uses the TABLE OBJECT group row when the grid groups columns by SchemaName.
        //The group level is discovered through the production policy, not guessed from a fixed C1 row index.
        private static int OracleTableObjectGroupLevel()
        {
            var policy = typeof(SchemaExplorerSelectionInfo).Assembly.GetType("JasonQuery.Core.SchemaExplorer.Selection.SchemaExplorerGroupLevelPolicy");

            Assert.IsNotNull(policy);

            var method = policy.GetMethod("IsObjectGroupLevel", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(method);

            for (var level = 0; level <= 12; level++)
            {
                if ((bool)method.Invoke(null, new object[] { DataSourceType.Oracle, level }))
                {
                    return level;
                }
            }

            Assert.Fail("No Oracle object group level found");
            return -1;
        }

        private static SchemaExplorerSelectionInfo TableObjectGroup(string owner = "SYS", string table = "AABBCC")
        {
            var selection = Selection(owner, table);

            selection.IsGroupRow = true;
            selection.GroupLevel = OracleTableObjectGroupLevel();

            return selection;
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G19_QualifiedTableObjectGroupOffersComment()
        {
            Assert.IsTrue(Offered(TableObjectGroup(), SharedSchemaExplorerMenuCommand.CommentTable));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G20_QualifiedTableObjectGroupOffersDrop()
        {
            Assert.IsTrue(Offered(TableObjectGroup(), SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G21_QualifiedTableObjectGroupOffersRename()
        {
            Assert.IsTrue(Offered(TableObjectGroup(), SharedSchemaExplorerMenuCommand.RenameTable));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G22_QualifiedTableObjectGroupOffersTruncate()
        {
            Assert.IsTrue(Offered(TableObjectGroup(), SharedSchemaExplorerMenuCommand.TruncateTable));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G23_QualifiedGroupBuildsVerifiedSysOwner()
        {
            Assert.IsTrue(Create(TableObjectGroup(), "SYS", SessionId, "SYS", SessionId, out var identity));
            Assert.AreEqual("SYS", identity.MetadataOwner);
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G24_QualifiedGroupPreservesQuotedMixedCaseOwner()
        {
            Assert.IsTrue(Create(TableObjectGroup("MiXeD"), "MiXeD", SessionId, "MiXeD", SessionId, out var identity));
            Assert.AreEqual("MiXeD", identity.SelectedOwner);
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G25_UnchangedTableObjectGroupAcceptedAtClickTime()
        {
            Assert.IsTrue(Same(TableObjectGroup(), TableObjectGroup()));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G26_ChangedGroupObjectNameRejectedAtClickTime()
        {
            Assert.IsFalse(Same(TableObjectGroup(), TableObjectGroup(table: "OTHER")));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G27_ChangedGroupOwnerRejectedAtClickTime()
        {
            Assert.IsFalse(Same(TableObjectGroup(), TableObjectGroup(owner: "SYSTEM")));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G28_WrongGroupLevelCannotAuthorizeTableDdl()
        {
            var group = TableObjectGroup();

            group.GroupLevel = -1;

            Assert.IsFalse(Offered(group, SharedSchemaExplorerMenuCommand.DropTable));
            Assert.IsFalse(Create(group, "SYS", SessionId, "SYS", SessionId, out _));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G29_UnqualifiedGroupCannotAuthorizeTableDdl()
        {
            var group = TableObjectGroup();

            group.HasQualifiedOracleSelectedOwner = false;

            Assert.IsFalse(Offered(group, SharedSchemaExplorerMenuCommand.DropTable));
            Assert.IsFalse(Create(group, "SYS", SessionId, "SYS", SessionId, out _));
        }

        [TestMethod, TestCategory("F02B3B2TGUI")]
        public void G30_CategoryGroupNodeCannotAuthorizeTableDdl()
        {
            var group = TableObjectGroup();

            group.Kind = SchemaExplorerSelectionKind.GroupNode;

            Assert.IsFalse(Offered(group, SharedSchemaExplorerMenuCommand.DropTable));
            Assert.IsFalse(Create(group, "SYS", SessionId, "SYS", SessionId, out _));
        }
    }
}
