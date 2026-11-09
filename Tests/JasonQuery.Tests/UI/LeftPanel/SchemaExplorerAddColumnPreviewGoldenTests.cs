using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U2-B: pure metadata-to-preview contract. No DB connection or DDL execution.
    //Reflective dispatch keeps Golden RED compilable before the new policy exists.
    [TestClass]
    public sealed class SchemaExplorerAddColumnPreviewGoldenTests
    {
        private const string PolicyName = "JasonQuery.UI.LeftPanel.NonOracleSharedAddColumnPreviewPolicy";
        private static readonly BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        private static SchemaExplorerSelectionInfo Table(DataSourceType provider)
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = provider,
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                IsGroupRow = false,
                IsColumnInfoRow = false,
                SchemaType = "Tables",
                SchemaName = provider == DataSourceType.SqlServer ? "dbo.Orders" : "Orders",
                SchemaNode = provider == DataSourceType.PostgreSql ? "ExactSchema" : "appdb",
                SchemaDbo = provider == DataSourceType.SqlServer ? "dbo" : string.Empty,
                ObjectId = provider == DataSourceType.SqlServer ? "101" : string.Empty,
                DisplayRowIndex = 4,
                GroupLevel = -1
            };
        }

        private static SchemaExplorerSelectionInfo Column(DataSourceType provider)
        {
            var selection = Table(provider);

            selection.Kind = SchemaExplorerSelectionKind.ColumnInfo;
            selection.IsColumnInfoRow = true;
            selection.SchemaName = "Id, INTEGER";

            return selection;
        }

        private static DataRow RawColumn(DataSourceType provider)
        {
            var source = new DataTable("SharedF7Snapshot");

            source.Columns.Add("SchemaNode", typeof(string));
            source.Columns.Add("SchemaType", typeof(string));
            source.Columns.Add("SchemaName", typeof(string));
            source.Columns.Add("Schema_Browser", typeof(string));
            source.Columns.Add("SchemaDbo", typeof(string));
            source.Columns.Add("ObjectID", typeof(string));

            var row = source.NewRow();

            row["SchemaNode"] = provider == DataSourceType.PostgreSql ? "ExactSchema" : "appdb";
            row["SchemaType"] = "Tables";
            row["SchemaName"] = provider == DataSourceType.SqlServer ? "dbo.Orders" : "Orders";
            row["Schema_Browser"] = "Id, INTEGER";
            row["SchemaDbo"] = provider == DataSourceType.SqlServer ? "dbo" : string.Empty;
            row["ObjectID"] = provider == DataSourceType.SqlServer ? "101" : string.Empty;
            source.Rows.Add(row);

            return row;
        }

        private static MethodInfo PolicyMethod(string methodName)
        {
            var type = typeof(SharedSchemaExplorerObjectCommandPolicy).Assembly.GetType(PolicyName, false);

            Assert.IsNotNull(type, "Missing F7-U2-B preview policy: " + PolicyName);

            var method = type.GetMethod(methodName, PublicStatic);

            Assert.IsNotNull(method, "Missing F7-U2-B policy method: " + methodName);
            return method;
        }

        private static bool Qualify(SchemaExplorerSelectionInfo selection, DataRow selectedDataRow)
        {
            var result = PolicyMethod("TryQualifyColumn").Invoke(null, new object[] { selection, selectedDataRow });

            return result is bool && (bool)result;
        }

        private static bool Create(SchemaExplorerSelectionInfo selection, out object target)
        {
            var arguments = new object[] { selection, null };
            var result = PolicyMethod("TryCreate").Invoke(null, arguments);

            target = arguments[1];
            return result is bool && (bool)result;
        }

        private static bool Same(SchemaExplorerSelectionInfo before, SchemaExplorerSelectionInfo after)
        {
            var result = PolicyMethod("IsSameSelection").Invoke(null, new object[] { before, after });

            return result is bool && (bool)result;
        }

        private static string TargetText(object target, string propertyName)
        {
            Assert.IsNotNull(target, "Missing preview target.");

            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(property, "Missing preview target field: " + propertyName);
            return Convert.ToString(property.GetValue(target, null));
        }

        private static bool TargetPreviewOnly(object target)
        {
            Assert.IsNotNull(target);

            var property = target.GetType().GetProperty("OfflineSqlPreviewOnly", BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(property, "The new F7 must explicitly request OfflineSqlPreviewOnly.");
            return (bool)property.GetValue(target, null);
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B01_PostgreSqlTableMapsToExactSchemaAndTable()
        {
            Assert.IsTrue(Create(Table(DataSourceType.PostgreSql), out var target));
            Assert.AreEqual(string.Empty, TargetText(target, "SchemaDatabase"));
            Assert.AreEqual("ExactSchema", TargetText(target, "SchemaName"));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B02_SqlServerTableMapsToDatabaseSchemaAndUnprefixedTable()
        {
            Assert.IsTrue(Create(Table(DataSourceType.SqlServer), out var target));
            Assert.AreEqual("appdb", TargetText(target, "SchemaDatabase"));
            Assert.AreEqual("dbo", TargetText(target, "SchemaName"));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B03_MySqlTableMapsToDatabaseAndTable()
        {
            Assert.IsTrue(Create(Table(DataSourceType.MySql), out var target));
            Assert.AreEqual("appdb", TargetText(target, "SchemaDatabase"));
            Assert.AreEqual(string.Empty, TargetText(target, "SchemaName"));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B04_PostgreSqlColumnUsesSelectedRawParentTable()
        {
            var selection = Column(DataSourceType.PostgreSql);

            Assert.IsTrue(Qualify(selection, RawColumn(selection.SourceType)));
            Assert.IsTrue(Create(selection, out var target));
            Assert.AreEqual("ExactSchema", TargetText(target, "SchemaName"));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B05_SqlServerColumnUsesRawObjectIdentityAndParentTable()
        {
            var selection = Column(DataSourceType.SqlServer);

            Assert.IsTrue(Qualify(selection, RawColumn(selection.SourceType)));
            Assert.IsTrue(Create(selection, out var target));
            Assert.AreEqual("appdb", TargetText(target, "SchemaDatabase"));
            Assert.AreEqual("dbo", TargetText(target, "SchemaName"));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.SchemaBrowserForm, selection, SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B06_MySqlColumnUsesSelectedRawDatabaseAndParentTable()
        {
            var selection = Column(DataSourceType.MySql);

            Assert.IsTrue(Qualify(selection, RawColumn(selection.SourceType)));
            Assert.IsTrue(Create(selection, out var target));
            Assert.AreEqual("appdb", TargetText(target, "SchemaDatabase"));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B07_MissingRawParentTableIsDenied()
        {
            var selection = Column(DataSourceType.PostgreSql);
            var row = RawColumn(selection.SourceType);

            row["SchemaName"] = " ";

            Assert.IsFalse(Qualify(selection, row));
            Assert.IsFalse(Create(selection, out _));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B08_RawSchemaOrDatabaseMismatchIsDenied()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.MySql })
            {
                var selection = Column(provider);
                var row = RawColumn(provider);

                row["SchemaNode"] = "other_scope";

                Assert.IsFalse(Qualify(selection, row), provider.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B09_SqlServerObjectIdMismatchIsDenied()
        {
            var selection = Column(DataSourceType.SqlServer);
            var row = RawColumn(selection.SourceType);

            row["ObjectID"] = "202";

            Assert.IsFalse(Qualify(selection, row));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B10_DottedSqlServerTableIdentifierPreservesEntireRawTable()
        {
            var selection = Column(DataSourceType.SqlServer);
            var row = RawColumn(selection.SourceType);

            row["SchemaName"] = "dbo.Order.Detail";

            Assert.IsTrue(Qualify(selection, row));
            Assert.IsTrue(Create(selection, out var target));
            Assert.AreEqual("Order.Detail", TargetText(target, "TableName"));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B11_ColumnCaptionIsNeverUsedAsParentTable()
        {
            var selection = Column(DataSourceType.PostgreSql);

            selection.SchemaName = "ColumnOnly, VARCHAR";

            var row = RawColumn(selection.SourceType);

            row["Schema_Browser"] = "ColumnOnly, VARCHAR";

            Assert.IsTrue(Qualify(selection, row));
            Assert.IsTrue(Create(selection, out var target));
            Assert.AreEqual("Orders", TargetText(target, "TableName"));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B12_ChangedColumnRowBetweenMenuAndClickIsDenied()
        {
            var before = Column(DataSourceType.PostgreSql);
            var after = Column(DataSourceType.PostgreSql);

            Assert.IsTrue(Qualify(before, RawColumn(before.SourceType)));
            Assert.IsTrue(Qualify(after, RawColumn(after.SourceType)));

            after.SchemaName = "OtherColumn, INTEGER";

            Assert.IsFalse(Same(before, after));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B13_ChangedSqlServerObjectIdBetweenMenuAndClickIsDenied()
        {
            var before = Table(DataSourceType.SqlServer);
            var after = Table(DataSourceType.SqlServer);

            after.ObjectId = "202";

            Assert.IsFalse(Same(before, after));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B14_ViewAndCategoryGroupsCannotCreateAddColumnTarget()
        {
            var view = Table(DataSourceType.MySql);

            view.SchemaType = "Views";

            Assert.IsFalse(Create(view, out _));

            var category = Table(DataSourceType.PostgreSql);

            category.IsGroupRow = true;
            category.GroupLevel = 2;

            Assert.IsFalse(Create(category, out _));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B15_AllNonOraclePreviewTargetsAreExplicitlyPreviewOnly()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                Assert.IsTrue(Create(Table(provider), out var target));
                Assert.IsTrue(TargetPreviewOnly(target), provider.ToString());
            }
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B16_NonOracleColumnMenusRetainAddColumnEntry()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                CollectionAssert.Contains(SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.QueryForm, Column(provider)).ToArray(), SharedSchemaExplorerMenuCommand.AddColumn);
            }
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B17_UnqualifiedNonOracleColumnCannotDispatchAddColumn()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, Column(provider), SharedSchemaExplorerMenuCommand.AddColumn));
            }
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B18_OracleU1TableAuthorizationRemainsUntouched()
        {
            var selection = Table(DataSourceType.Oracle);

            selection.SchemaName = "ORDERS";
            selection.HasQualifiedOracleSelectedOwner = true;
            selection.OracleSelectedOwner = "SYS";

            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B19_CaseSensitivePostgreSqlSchemaChangeCannotReuseTarget()
        {
            var before = Table(DataSourceType.PostgreSql);
            var after = Table(DataSourceType.PostgreSql);

            after.SchemaNode = "exactschema";

            Assert.IsFalse(Same(before, after));
        }

        [TestMethod, TestCategory("F7U2B")]
        public void B20_ColumnCaptionMismatchWithSelectedDataRowIsDenied()
        {
            var selection = Column(DataSourceType.MySql);
            var row = RawColumn(selection.SourceType);

            row["Schema_Browser"] = "OtherColumn, INTEGER";

            Assert.IsFalse(Qualify(selection, row));
        }
    }
}
