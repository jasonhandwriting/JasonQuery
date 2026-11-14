using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.Forms;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C2B3A: GUI bridge contract only. No database, DDL dispatch, or WinForms dialog is opened by these tests.
    [TestClass]
    [TestCategory("F7U3C2B3A")]
    public sealed class SchemaExplorerColumnDdlGuiBridgeGoldenTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private static SchemaExplorerSelectionInfo Column(string rawColumn)
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = DataSourceType.PostgreSql,
                Kind = SchemaExplorerSelectionKind.ColumnInfo,
                IsColumnInfoRow = true,
                IsGroupRow = false,
                SchemaType = "Tables",
                SchemaNode = "ExactSchema",
                SchemaName = "X, integer",
                DisplayRowIndex = 9,
                NonOracleRawParentTableName = "Orders",
                NonOracleRawColumnCaption = "X, integer",
                HasQualifiedNonOracleColumnParent = true,
                NonOracleRawColumnName = rawColumn,
                HasQualifiedNonOracleColumnIdentity = true
            };
        }

        [TestMethod]
        public void B601_MainFormHasNonOracleExistingColumnBridge()
        {
            var method = typeof(MainForm).GetMethod
            (
                "TryOpenSharedNonOracleColumnDdl",
                PrivateInstance,
                null,
                new[] { typeof(SchemaExplorerSelectionInfo), typeof(SharedSchemaExplorerMenuCommand) },
                null
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(bool), method.ReturnType);
        }

        [TestMethod]
        public void B602_EditColumnFormHasDistinctExistingColumnGate()
        {
            var property = typeof(EditColumnForm).GetProperty("RequireNonOracleF7ExistingColumnGate", PrivateInstance);

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(bool), property.PropertyType);
            Assert.IsTrue(property.GetGetMethod(true).IsAssembly);
        }

        [TestMethod]
        public void B603_ExistingColumnFormCapturesOriginalColumnName()
        {
            var property = typeof(EditColumnForm).GetProperty("NonOracleF7ExpectedColumn", PrivateInstance);

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [TestMethod]
        public void B604_OriginalColumnNameMayContainCommaWithoutParsingCaption()
        {
            var selection = Column("X, Y");

            Assert.IsTrue(NonOracleSharedColumnDdlBridgePolicy.IsQualifiedColumnDataRow(selection));
            Assert.AreEqual("X, Y", selection.NonOracleRawColumnName);
        }

        [TestMethod]
        public void B605_ChangedRawColumnRejectsCachedSelection()
        {
            Assert.IsTrue(NonOracleSharedColumnDdlBridgePolicy.IsSameColumnDataRow(Column("X, Y"), Column("X, Y")));
            Assert.IsFalse(NonOracleSharedColumnDdlBridgePolicy.IsSameColumnDataRow(Column("X, Y"), Column("X")));
        }

        [TestMethod]
        public void B606_ExistingAddColumnEntryRemainsSeparate()
        {
            var method = typeof(MainForm).GetMethod
            (
                "TryOpenSharedNonOracleAddColumnPreview",
                PrivateInstance,
                null,
                new[] { typeof(SchemaExplorerSelectionInfo) },
                null
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(bool), method.ReturnType);
        }
    }
}
