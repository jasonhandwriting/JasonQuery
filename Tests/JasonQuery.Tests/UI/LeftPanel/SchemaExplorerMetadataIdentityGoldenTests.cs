using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U2-A: policy/metadata contract only. These tests never connect to a database or execute DDL.
    //An offered menu command is not authorization to dispatch or commit a database change.
    [TestClass]
    public sealed class SchemaExplorerMetadataIdentityGoldenTests
    {
        private static readonly DataSourceType[] ThreeProviders =
        {
            DataSourceType.PostgreSql,
            DataSourceType.SqlServer,
            DataSourceType.MySql
        };

        private static readonly LeftPanelTargetKind[] TwoHosts =
        {
            LeftPanelTargetKind.QueryForm,
            LeftPanelTargetKind.SchemaBrowserForm
        };

        private static SchemaExplorerSelectionInfo Table(DataSourceType source)
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = source,
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                IsGroupRow = false,
                IsColumnInfoRow = false,
                SchemaType = "Tables",
                SchemaName = "ORDERS",
                SchemaNode = source == DataSourceType.PostgreSql ? "public" : "appdb",
                SchemaDbo = "dbo",
                ObjectId = "101",
                DisplayRowIndex = 4
            };
        }

        private static SchemaExplorerSelectionInfo Column(DataSourceType source)
        {
            var selection = Table(source);

            selection.Kind = SchemaExplorerSelectionKind.ColumnInfo;
            selection.IsColumnInfoRow = true;
            selection.SchemaName = "ID, INTEGER";

            return selection;
        }

        private static bool MenuOffers(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection)
        {
            return SharedSchemaExplorerContextMenuPolicy.Plan(target, selection).Contains(SharedSchemaExplorerMenuCommand.AddColumn);
        }

        private static bool CanOpen(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection)
        {
            //This is a UI command policy, not the future execute-time authorization gate.
            return SharedSchemaExplorerObjectCommandPolicy.CanExecute(target, selection, SharedSchemaExplorerMenuCommand.AddColumn);
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A01_PostgreSqlTableMenuOffersAddColumnInBothHosts()
        {
            foreach (var host in TwoHosts)
            {
                Assert.IsTrue(MenuOffers(host, Table(DataSourceType.PostgreSql)), host.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A02_SqlServerTableMenuOffersAddColumnInBothHosts()
        {
            foreach (var host in TwoHosts)
            {
                Assert.IsTrue(MenuOffers(host, Table(DataSourceType.SqlServer)), host.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A03_MySqlTableMenuOffersAddColumnInBothHosts()
        {
            foreach (var host in TwoHosts)
            {
                Assert.IsTrue(MenuOffers(host, Table(DataSourceType.MySql)), host.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A04_PostgreSqlTableCommandRequiresQualifiedSchemaCandidate()
        {
            foreach (var host in TwoHosts)
            {
                Assert.IsTrue(CanOpen(host, Table(DataSourceType.PostgreSql)), host.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A05_SqlServerTableCommandRequiresDatabaseSchemaAndObjectCandidate()
        {
            foreach (var host in TwoHosts)
            {
                Assert.IsTrue(CanOpen(host, Table(DataSourceType.SqlServer)), host.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A06_MySqlTableCommandRequiresDatabaseCandidate()
        {
            foreach (var host in TwoHosts)
            {
                Assert.IsTrue(CanOpen(host, Table(DataSourceType.MySql)), host.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A07_ThreeProviderColumnMenusStillOfferAddColumn()
        {
            //Existing Column caption is not the target Table identity; only menu presence is tested here.
            foreach (var source in ThreeProviders)
            {
                foreach (var host in TwoHosts)
                {
                    Assert.IsTrue(MenuOffers(host, Column(source)), source + "/" + host);
                }
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A08_ViewsNeverGainAddColumn()
        {
            foreach (var source in ThreeProviders)
            {
                var view = Table(source);

                view.SchemaType = "Views";

                Assert.IsFalse(MenuOffers(LeftPanelTargetKind.QueryForm, view));
                Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, view));
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A09_CategoryGroupCannotAuthorizeAddColumn()
        {
            foreach (var source in ThreeProviders)
            {
                var category = Table(source);

                category.Kind = SchemaExplorerSelectionKind.GroupNode;
                category.IsGroupRow = true;
                category.GroupLevel = 2;

                Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, category));
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A10_PostgreSqlBlankSchemaCannotAuthorizeAddColumn()
        {
            var selection = Table(DataSourceType.PostgreSql);

            selection.SchemaNode = " ";

            Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, selection));
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A11_SqlServerMissingDatabaseOrSchemaCannotAuthorizeAddColumn()
        {
            var selection = Table(DataSourceType.SqlServer);

            selection.SchemaNode = string.Empty;

            Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, selection));

            selection.SchemaNode = "appdb";
            selection.SchemaDbo = " ";

            Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, selection));
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A12_MySqlMissingDatabaseCannotAuthorizeAddColumn()
        {
            var selection = Table(DataSourceType.MySql);

            selection.SchemaNode = string.Empty;

            Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, selection));
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A13_PostgreSqlTableGroupMustUseSelectedParentSchemaNotFirstRow()
        {
            var reader = GroupReader( "public", "ORDERS");

            AddRow(reader.Data, "ORDERS", "sales", "", "");
            AddRow(reader.Data, "ORDERS", "public", "", "");

            var selection = ResolveGroup(DataSourceType.PostgreSql, reader);

            Assert.AreEqual("public", selection.SchemaNode, "Never choose another schema's same-name table.");
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A14_PostgreSqlQuotedCaseSensitiveTableGroupMustNotPickCaseInsensitiveFirstRow()
        {
            var reader = GroupReader( "ExactSchema", "Orders");

            AddRow(reader.Data, "orders", "other_schema", "", "");
            AddRow(reader.Data, "Orders", "ExactSchema", "", "");

            var selection = ResolveGroup(DataSourceType.PostgreSql, reader);

            Assert.AreEqual("Orders", selection.SchemaName);
            Assert.AreEqual("ExactSchema", selection.SchemaNode, "Quoted PostgreSQL identifiers are case-sensitive.");
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A15_SqlServerSameTableAcrossSchemasWithoutUniqueGroupIdentityIsDenied()
        {
            var reader = GroupReader( "appdb", "ORDERS");

            AddRow(reader.Data, "ORDERS", "appdb", "dbo", "101");
            AddRow(reader.Data, "ORDERS", "appdb", "sales", "202");

            var selection = ResolveGroup(DataSourceType.SqlServer, reader);

            Assert.IsTrue(selection.Kind == SchemaExplorerSelectionKind.Invalid || string.IsNullOrWhiteSpace(selection.SchemaDbo), "A table-object group without a unique schema must fail closed.");
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A16_SqlServerConflictingObjectIdsForSameTableAreDenied()
        {
            var reader = GroupReader( "appdb", "ORDERS");

            AddRow(reader.Data, "ORDERS", "appdb", "dbo", "101");
            AddRow(reader.Data, "ORDERS", "appdb", "dbo", "202");

            var selection = ResolveGroup(DataSourceType.SqlServer, reader);

            Assert.IsTrue(selection.Kind == SchemaExplorerSelectionKind.Invalid || string.IsNullOrWhiteSpace(selection.ObjectId), "Conflicting ObjectIDs must not be resolved by selecting the first row.");
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A17_MySqlTableGroupMustUseSelectedDatabaseNotFirstRow()
        {
            var reader = GroupReader( "active_db", "ORDERS");

            AddRow(reader.Data, "ORDERS", "archive_db", "", "");
            AddRow(reader.Data, "ORDERS", "active_db", "", "");

            var selection = ResolveGroup(DataSourceType.MySql, reader);

            Assert.AreEqual("active_db", selection.SchemaNode, "Never target a stale database's same-name table.");
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A18_WrongGroupLevelCannotAuthorizeAddColumn()
        {
            foreach (var source in ThreeProviders)
            {
                var selection = Table(source);

                selection.IsGroupRow = true;
                selection.GroupLevel = 2;

                Assert.IsFalse(CanOpen(LeftPanelTargetKind.QueryForm, selection));
            }
        }

        [TestMethod, TestCategory("F7U2A")]
        public void A19_OracleU1QualifiedTableAddColumnRemainsAllowed()
        {
            var oracle = Table(DataSourceType.Oracle);

            oracle.HasQualifiedOracleSelectedOwner = true;
            oracle.OracleSelectedOwner = "SYS";

            Assert.IsTrue(MenuOffers(LeftPanelTargetKind.QueryForm, oracle));
            Assert.IsTrue(CanOpen(LeftPanelTargetKind.QueryForm, oracle));
        }

        private static SchemaExplorerSelectionInfo ResolveGroup(DataSourceType source, FixtureReader reader)
        {
            return SchemaExplorerSelectionResolver.Resolve(new SchemaExplorerSelectionResolveRequest
            {
                SourceType = source,
                DisplayRowIndex = 3,
                IsExternalSelection = false,
                IsShowColumnInfo = false
            }, reader);
        }

        private static FixtureReader GroupReader(string parentScope, string table)
        {
            return new FixtureReader(parentScope, table);
        }

        private static void AddRow(DataTable table, string tableName, string schemaNode, string schemaDbo, string objectId)
        {
            var row = table.NewRow();

            row["SchemaType"] = "Tables";
            row["SchemaName"] = tableName;
            row["Schema_Browser"] = tableName;
            row["SchemaNode"] = schemaNode;
            row["SchemaDbo"] = schemaDbo;
            row["ObjectID"] = objectId;
            row["SchemaObject"] = "TABLE";

            table.Rows.Add(row);
        }

        private sealed class FixtureReader : ISchemaExplorerRowReader
        {
            private readonly string _parentScope;
            private readonly string _groupedTable;

            public FixtureReader(string parentScope, string groupedTable)
            {
                _parentScope = parentScope;
                _groupedTable = groupedTable;
                Data = new DataTable("F7U2Fixture");
                Data.Columns.Add("SchemaType", typeof(string));
                Data.Columns.Add("SchemaName", typeof(string));
                Data.Columns.Add("Schema_Browser", typeof(string));
                Data.Columns.Add("SchemaNode", typeof(string));
                Data.Columns.Add("SchemaDbo", typeof(string));
                Data.Columns.Add("ObjectID", typeof(string));
                Data.Columns.Add("SchemaObject", typeof(string));
            }

            public DataTable Data { get; }
            public bool IsGroupRow(int displayRowIndex) { return true; }
            public int GetGroupLevel(int displayRowIndex) { return 3; }
            public string GetGroupedText(int displayRowIndex) { return _groupedTable; }
            public string GetSchemaTypeFromGroupStartIndex(int displayRowIndex) { return "Tables"; }
            public string FindPreviousGroupText(int displayRowIndex, int groupLevel)
            {
                return groupLevel == 2 ? "Tables" : _parentScope;
            }
            public DataRow GetDataRow(int displayRowIndex) { return null; }
            public DataTable GetSchemaDataTable() { return Data; }
            public void SetCurrentRow(int displayRowIndex) { }
        }
    }
}
