using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U4B-D: approve only the simplified shared-F7 paste menu before legacy retirement.
    //The three suffix-paste variants must not be offered or dispatched.
    //Pure policy tests: no WinForms, connection, database, SQL execution, or Git mutation.
    [TestClass]
    [TestCategory("F7U4BD")]
    public sealed class SchemaExplorerPasteSimplificationGoldenTests
    {
        private static readonly SharedSchemaExplorerMenuCommand[] RemovedCommands =
        {
            SharedSchemaExplorerMenuCommand.PasteComma,
            SharedSchemaExplorerMenuCommand.PasteCommaNewLine,
            SharedSchemaExplorerMenuCommand.PasteNewLine
        };

        private static SchemaExplorerSelectionInfo Object(DataSourceType provider, string objectType, bool isColumn)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = isColumn ? SchemaExplorerSelectionKind.ColumnInfo : SchemaExplorerSelectionKind.SchemaObject,
                SourceType = provider,
                SchemaType = objectType,
                SchemaName = isColumn ? "ID, INTEGER" : "ORDERS",
                SchemaNode = provider == DataSourceType.Oracle ? "JQ_OWNER" : "sampledb",
                SchemaDbo = "dbo",
                IsColumnInfoRow = isColumn,
                DisplayRowIndex = 3,
                GroupLevel = 3
            };
        }

        [TestMethod]
        public void B01_SharedMenuNoLongerOffersThreeSuffixPasteVariants_RED()
        {
            foreach (var provider in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                foreach (var target in new[] { LeftPanelTargetKind.QueryForm, LeftPanelTargetKind.SchemaBrowserForm })
                {
                    foreach (var isColumn in new[] { false, true })
                    {
                        var row = Object(provider, "Tables", isColumn);
                        var menu = SharedSchemaExplorerContextMenuPolicy.Plan(target, row);

                        foreach (var removed in RemovedCommands)
                        {
                            Assert.IsFalse(menu.Contains(removed), provider + "/" + target + "/column=" + isColumn + "/" + removed);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void B02_RemovedSuffixPasteCommandsAreNotExecutable_RED()
        {
            var row = Object(DataSourceType.PostgreSql, "Tables", true);

            foreach (var command in RemovedCommands)
            {
                Assert.IsFalse(SharedSchemaExplorerPastePolicy.IsPasteCommand(command), command.ToString());
                Assert.IsFalse(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, row, command, out _), command.ToString());
            }
        }

        [TestMethod]
        public void B03_CopyAndTheTwoRetainedPasteActionsAreUnchanged_GREEN()
        {
            var table = Object(DataSourceType.Oracle, "Tables", false);
            var column = Object(DataSourceType.Oracle, "Tables", true);
            var tableMenu = SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.QueryForm, table);
            var columnMenu = SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.QueryForm, column);

            CollectionAssert.Contains(tableMenu.ToArray(), SharedSchemaExplorerMenuCommand.CopyToClipboard);
            CollectionAssert.Contains(tableMenu.ToArray(), SharedSchemaExplorerMenuCommand.PasteSelectAll);
            CollectionAssert.Contains(tableMenu.ToArray(), SharedSchemaExplorerMenuCommand.PasteName);
            CollectionAssert.Contains(columnMenu.ToArray(), SharedSchemaExplorerMenuCommand.PasteName);
            CollectionAssert.DoesNotContain(columnMenu.ToArray(), SharedSchemaExplorerMenuCommand.PasteSelectAll);

            Assert.IsTrue(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, table, SharedSchemaExplorerMenuCommand.PasteSelectAll, out var sql));
            Assert.AreEqual("SELECT * FROM ORDERS", sql);
            Assert.IsTrue(SharedSchemaExplorerPastePolicy.TryBuildText(LeftPanelTargetKind.QueryForm, column, SharedSchemaExplorerMenuCommand.PasteName, out var name));
            Assert.AreEqual("ID", name);
        }
    }
}
