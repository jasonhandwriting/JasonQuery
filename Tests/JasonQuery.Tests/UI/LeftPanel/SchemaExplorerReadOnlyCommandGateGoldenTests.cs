using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U4B-C1: Golden-first contract for non-mutating F7 object entry points.
    //B01/B06 preserve existing behavior; B02-B05 are intentionally RED before implementation.
    //No WinForms instantiation, database access, connection, DDL or Git mutations.
    [TestClass]
    [TestCategory("F7U4BC1")]
    public sealed class SchemaExplorerReadOnlyCommandGateGoldenTests
    {
        private static SchemaExplorerSelectionInfo Object(DataSourceType source, string kind = "Tables")
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = source,
                DisplayRowIndex = 3,
                SchemaType = kind,
                SchemaName = "JQ_TEST",
                SchemaNode = source == DataSourceType.Oracle ? "JQ_OWNER" : "sampledb",
                SchemaDbo = "dbo",
                ObjectId = "123",
                OracleSelectedOwner = source == DataSourceType.Oracle ? "JQ_OWNER" : string.Empty,
                HasQualifiedOracleSelectedOwner = source == DataSourceType.Oracle
            };
        }

        private static bool Offer(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command, bool cachedSnapshotReadable)
        {
            var method = typeof(SharedSchemaExplorerObjectCommandPolicy).GetMethod
            (
                "CanOfferReadOnlyAction",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(LeftPanelTargetKind), typeof(SchemaExplorerSelectionInfo), typeof(SharedSchemaExplorerMenuCommand), typeof(bool) },
                null
            );

            Assert.IsNotNull(method, "EXPECTED RED: add a pure read-only action candidate gate; do not widen DDL CanOpenGenerator.");
            return (bool)method.Invoke(null, new object[] { target, selection, command, cachedSnapshotReadable });
        }

        private static string Source(params string[] parts)
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && directory != null; depth++)
                {
                    var path = Path.Combine(directory.FullName, Path.Combine(parts));

                    if (File.Exists(path))
                    {
                        return File.ReadAllText(path);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("Source file not found: " + Path.Combine(parts));
            return string.Empty;
        }

        private static string MethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing method: " + signature);

            var open = source.IndexOf('{', start + signature.Length);

            Assert.IsTrue(open >= 0, "Missing method opening brace: " + signature);

            var depth = 0;

            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            Assert.Fail("Missing method closing brace: " + signature);
            return string.Empty;
        }

        [TestMethod]
        public void B01_ExistingMenuTargetContractsRemainUnchanged_GREEN()
        {
            var row = Object(DataSourceType.Oracle);
            var query = SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.QueryForm, row).ToArray();
            var browser = SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.SchemaBrowserForm, row).ToArray();
            var other = SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.OtherForm, row).ToArray();

            CollectionAssert.Contains(query, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.Contains(query, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser);
            CollectionAssert.Contains(query, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
            CollectionAssert.DoesNotContain(browser, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.DoesNotContain(browser, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser);
            CollectionAssert.Contains(browser, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
            CollectionAssert.Contains(other, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.DoesNotContain(other, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
        }

        [TestMethod]
        public void B02_CachedQualifiedReadOnlyObjectsAreCandidates_RED()
        {
            foreach (var provider in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                var row = Object(provider);

                Assert.IsTrue(Offer(LeftPanelTargetKind.QueryForm, row, SharedSchemaExplorerMenuCommand.ShowDdl, true), provider.ToString());
                Assert.IsTrue(Offer(LeftPanelTargetKind.QueryForm, row, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser, true), provider.ToString());
                Assert.IsTrue(Offer(LeftPanelTargetKind.QueryForm, row, SharedSchemaExplorerMenuCommand.GenerateSqlStatement, true), provider.ToString());
            }
        }

        [TestMethod]
        public void B03_CachedReadOnlyGateRejectsStaleInvalidAndStructuralCommands_RED()
        {
            var table = Object(DataSourceType.PostgreSql);

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, table, SharedSchemaExplorerMenuCommand.ShowDdl, false));
            Assert.IsFalse(Offer(LeftPanelTargetKind.OtherForm, table, SharedSchemaExplorerMenuCommand.GenerateSqlStatement, true));
            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, table, SharedSchemaExplorerMenuCommand.DropTable, true));

            table.Kind = SchemaExplorerSelectionKind.Invalid;

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, table, SharedSchemaExplorerMenuCommand.ShowDdl, true));
        }

        [TestMethod]
        public void B04_MenuAvailabilityUsesSeparateReadOnlyGate_RED()
        {
            var source = Source("JasonQuery", "UI", "Forms", "MainForm.SharedSchemaExplorerEvents.cs");
            var method = MethodBody(source, "private void SharedSchemaExplorerGridMouseUp(object sender, MouseEventArgs e)");

            Assert.Contains("CanOfferReadOnlyAction", method, "EXPECTED RED: distinguish read-only object entries from structural DDL form gates.");
            Assert.Contains("SharedSchemaDdlInteractionPolicy.CanOpenGenerator", method);
            Assert.Contains("SharedSchemaExplorerObjectCommandPolicy.CanExecute", method);
        }

        [TestMethod]
        public void B05_ReadOnlyClicksMustReresolveTheCurrentGridRow_RED()
        {
            var source = Source("JasonQuery", "UI", "Forms", "MainForm.SharedSchemaExplorerObjectCommands.cs");
            var method = MethodBody(source, "private void SharedSchemaObjectMenuClick(object sender, EventArgs e)");

            Assert.Contains("ResolveSharedSchemaExplorerInput", method, "EXPECTED RED: do not dispatch Show DDL / Browser / Generate SQL from a stale captured row.");
            Assert.Contains("CanOfferReadOnlyAction", method, "EXPECTED RED: click-time read-only gate must be checked independently.");
            Assert.Contains("SharedSchemaExplorerObjectCommandPolicy.CanExecute", method);
        }

        [TestMethod]
        public void B06_ExistingDdlAndProviderDispatchRemainIntact_GREEN()
        {
            var source = Source("JasonQuery", "UI", "Forms", "MainForm.SharedSchemaExplorerObjectCommands.cs");
            var method = MethodBody(source, "private void SharedSchemaObjectMenuClick(object sender, EventArgs e)");

            Assert.Contains("SharedSchemaDdlInteractionPolicy.CanOpenGenerator", method);
            Assert.Contains("CanHandleSharedSchemaExplorerSessionAction()", method);
            Assert.Contains("TryOpenSharedOracleTableDdl", method);
            Assert.Contains("TryOpenSharedNonOracleTableDdl", method);
            Assert.Contains("TryOpenSharedOracleColumnDdl", method);
            Assert.Contains("TryOpenSharedNonOracleColumnDdl", method);
            Assert.Contains("ShowSharedSchemaDdl(selection)", method);
            Assert.Contains("OpenSharedSchemaBrowser(selection)", method);
            Assert.Contains("OpenSharedGenerateSql(selection, activeTab.AccessibleDescription)", method);
        }

        //GUI regression: C1 Table/View object headers are real group rows, not the non-group SchemaObject rows covered by B02.
        [TestMethod]
        public void B07_QualifiedC1ObjectGroupsOfferReadOnlyCommands_GREEN()
        {
            foreach (var provider in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                foreach (var objectType in new[] { "Tables", "Views" })
                {
                    var row = Object(provider, objectType);

                    row.IsGroupRow = true;
                    row.GroupLevel = SchemaExplorerGroupLevelPolicy.GetMinimumObjectGroupLevel(provider);

                    Assert.IsTrue(Offer(LeftPanelTargetKind.QueryForm, row, SharedSchemaExplorerMenuCommand.ShowDdl, true), provider + "/" + objectType + "/ShowDdl");
                    Assert.IsTrue(Offer(LeftPanelTargetKind.QueryForm, row, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser, true), provider + "/" + objectType + "/OpenSchemaBrowser");
                    Assert.IsTrue(Offer(LeftPanelTargetKind.QueryForm, row, SharedSchemaExplorerMenuCommand.GenerateSqlStatement, true), provider + "/" + objectType + "/GenerateSqlStatement");
                }
            }
        }

        [TestMethod]
        public void B08_CategoryGroupsAndUnverifiedOracleOwnersRemainDenied_GREEN()
        {
            var oracle = Object(DataSourceType.Oracle);

            oracle.IsGroupRow = true;
            oracle.GroupLevel = SchemaExplorerGroupLevelPolicy.GetMinimumObjectGroupLevel(DataSourceType.Oracle);
            oracle.HasQualifiedOracleSelectedOwner = false;

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, oracle, SharedSchemaExplorerMenuCommand.ShowDdl, true));

            oracle.HasQualifiedOracleSelectedOwner = true;
            oracle.OracleSelectedOwner = string.Empty;

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, oracle, SharedSchemaExplorerMenuCommand.ShowDdl, true));

            var group = Object(DataSourceType.PostgreSql);

            group.IsGroupRow = true;
            group.GroupLevel = SchemaExplorerGroupLevelPolicy.GetMinimumObjectGroupLevel(DataSourceType.PostgreSql) - 1;

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, group, SharedSchemaExplorerMenuCommand.ShowDdl, true));

            group.GroupLevel = SchemaExplorerGroupLevelPolicy.GetMinimumObjectGroupLevel(DataSourceType.PostgreSql);
            group.Kind = SchemaExplorerSelectionKind.GroupNode;

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, group, SharedSchemaExplorerMenuCommand.ShowDdl, true));

            group.Kind = SchemaExplorerSelectionKind.SchemaObject;

            Assert.IsFalse(Offer(LeftPanelTargetKind.QueryForm, group, SharedSchemaExplorerMenuCommand.DropTable, true));
        }

    }
}
