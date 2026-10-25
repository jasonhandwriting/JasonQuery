using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedSchemaExplorerPastePolicyTests
    {
        private static SchemaExplorerSelectionInfo Selection(DataSourceType sourceType = DataSourceType.Oracle, string schemaType = "Tables", bool column = false)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = column ? SchemaExplorerSelectionKind.ColumnInfo : SchemaExplorerSelectionKind.SchemaObject,
                SourceType = sourceType,
                SchemaType = schemaType,
                SchemaName = column ? "ID, VARCHAR2" : "ORDERS",
                SchemaNode = "public",
                SchemaDbo = "dbo",
                IsColumnInfoRow = column,
                DisplayRowIndex = 7,
                GroupLevel = 3
            };
        }

        private static string Build(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command)
        {
            Assert.IsTrue(SharedSchemaExplorerPastePolicy.TryBuildText(target, selection, command, out var result));
            return result;
        }

        [TestMethod]
        public void PasteCommandAllowlist_HasOnlyFivePasteCommands()
        {
            foreach (SharedSchemaExplorerMenuCommand command in System.Enum.GetValues(typeof(SharedSchemaExplorerMenuCommand)))
            {
                var allowed = command == SharedSchemaExplorerMenuCommand.PasteSelectAll
                    || command == SharedSchemaExplorerMenuCommand.PasteName
                    || command == SharedSchemaExplorerMenuCommand.PasteComma
                    || command == SharedSchemaExplorerMenuCommand.PasteCommaNewLine
                    || command == SharedSchemaExplorerMenuCommand.PasteNewLine;

                Assert.AreEqual(allowed, SharedSchemaExplorerPastePolicy.IsPasteCommand(command), command.ToString());
            }
        }

        [TestMethod]
        public void ColumnNamePaste_RemovesDatatypeSuffix()
        {
            Assert.AreEqual("ID", Build(LeftPanelTargetKind.QueryForm, Selection(column: true), SharedSchemaExplorerMenuCommand.PasteName));
        }

        [TestMethod]
        public void ColumnCommaPaste_AppendsCommaSpace()
        {
            Assert.AreEqual("ID, ", Build(LeftPanelTargetKind.QueryForm, Selection(column: true), SharedSchemaExplorerMenuCommand.PasteComma));
        }

        [TestMethod]
        public void ColumnCommaNewlinePaste_UsesCrLf()
        {
            Assert.AreEqual("ID, \r\n", Build(LeftPanelTargetKind.QueryForm, Selection(column: true), SharedSchemaExplorerMenuCommand.PasteCommaNewLine));
        }

        [TestMethod]
        public void ColumnNewlinePaste_UsesCrLf()
        {
            Assert.AreEqual("ID\r\n", Build(LeftPanelTargetKind.QueryForm, Selection(column: true), SharedSchemaExplorerMenuCommand.PasteNewLine));
        }

        [TestMethod]
        public void TableNamePaste_IsUnchanged()
        {
            Assert.AreEqual("ORDERS", Build(LeftPanelTargetKind.QueryForm, Selection(), SharedSchemaExplorerMenuCommand.PasteName));
        }

        [TestMethod]
        public void OracleSelectAll_UsesLegacyUppercaseSql()
        {
            Assert.AreEqual("SELECT * FROM ORDERS", Build(LeftPanelTargetKind.QueryForm, Selection(), SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void PostgreSqlSelectAll_QualifiesSchema()
        {
            Assert.AreEqual("select * from public.ORDERS", Build(LeftPanelTargetKind.QueryForm, Selection(DataSourceType.PostgreSql), SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void SqlServerSelectAll_QualifiesSchema()
        {
            Assert.AreEqual("select * from dbo.ORDERS", Build(LeftPanelTargetKind.QueryForm, Selection(DataSourceType.SqlServer), SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void SqlServerSelectAll_DoesNotDuplicateSchema()
        {
            var selection = Selection(DataSourceType.SqlServer);

            selection.SchemaName = "dbo.ORDERS";

            Assert.AreEqual("select * from dbo.ORDERS", Build(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void MySqlSelectAll_UsesLegacyUnqualifiedName()
        {
            Assert.AreEqual("select * from ORDERS", Build(LeftPanelTargetKind.QueryForm, Selection(DataSourceType.MySql), SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void ViewSelectAll_IsAvailable()
        {
            Assert.AreEqual("SELECT * FROM ORDERS", Build(LeftPanelTargetKind.QueryForm, Selection(schemaType: "Views"), SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void ViewColumn_SelectAllIsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, Selection(schemaType: "Views", column: true), SharedSchemaExplorerMenuCommand.PasteSelectAll, out _));
        }

        [TestMethod]
        public void TableColumn_SelectAllIsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, Selection(column: true), SharedSchemaExplorerMenuCommand.PasteSelectAll, out _));
        }

        [TestMethod]
        public void SchemaBrowserColumnPaste_KeepsSuffixFormat()
        {
            Assert.AreEqual("ID, \r\n", Build(LeftPanelTargetKind.SchemaBrowserForm, Selection(column: true), SharedSchemaExplorerMenuCommand.PasteCommaNewLine));
        }

        [TestMethod]
        public void SchemaBrowserTable_SelectAllIsAvailable()
        {
            Assert.AreEqual("SELECT * FROM ORDERS", Build(LeftPanelTargetKind.SchemaBrowserForm, Selection(), SharedSchemaExplorerMenuCommand.PasteSelectAll));
        }

        [TestMethod]
        public void MissingPostgreSqlSchema_SelectAllIsDenied()
        {
            var selection = Selection(DataSourceType.PostgreSql);

            selection.SchemaNode = " ";

            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.PasteSelectAll, out _));
        }

        [TestMethod]
        public void MissingSqlServerSchema_SelectAllIsDenied()
        {
            var selection = Selection(DataSourceType.SqlServer);

            selection.SchemaDbo = "";

            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.PasteSelectAll, out _));
        }

        [TestMethod]
        public void UnknownTarget_IsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.OtherForm, Selection(), SharedSchemaExplorerMenuCommand.PasteName, out _));
        }

        [TestMethod]
        public void NullSelection_IsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, null, SharedSchemaExplorerMenuCommand.PasteName, out _));
        }

        [TestMethod]
        public void InvalidDatabase_IsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, Selection(DataSourceType.None), SharedSchemaExplorerMenuCommand.PasteName, out _));
        }

        [TestMethod]
        public void NonPasteCommands_AreDenied()
        {
            foreach (var command in new[]
            {
                SharedSchemaExplorerMenuCommand.CopyToClipboard,
                SharedSchemaExplorerMenuCommand.ShowDdl,
                SharedSchemaExplorerMenuCommand.GenerateSqlStatement,
                SharedSchemaExplorerMenuCommand.DropColumn,
                SharedSchemaExplorerMenuCommand.TruncateTable
            })
            {
                Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, Selection(), command, out _), command.ToString());
            }
        }

        [TestMethod]
        public void GroupColumn_IsDenied()
        {
            var selection = Selection(column: true);

            selection.IsGroupRow = true;

            Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.PasteName, out _));
        }
    }
}
