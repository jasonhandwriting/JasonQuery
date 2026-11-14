using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C: Golden-first contracts for the three remaining non-Oracle Column DDL actions.
    //Pure tests only. Never open database connections or execute DDL.
    [TestClass]
    [TestCategory("F7U3C")]
    public sealed class SchemaExplorerColumnDdlOperationsGoldenTests
    {
        private const string BridgeTypeName = "JasonQuery.UI.LeftPanel.NonOracleSharedColumnDdlBridgePolicy";
        private const string QualifiedRawColumnName = "NonOracleMetadataColumnName";

        private static readonly DataSourceType[] Providers =
        {
            DataSourceType.PostgreSql,
            DataSourceType.SqlServer,
            DataSourceType.MySql
        };

        private static readonly SharedSchemaExplorerMenuCommand[] Commands =
        {
            SharedSchemaExplorerMenuCommand.CommentColumn,
            SharedSchemaExplorerMenuCommand.RenameColumn,
            SharedSchemaExplorerMenuCommand.DropColumn
        };

        private static readonly ColumnDdlOperation[] Operations =
        {
            ColumnDdlOperation.Comment,
            ColumnDdlOperation.Rename,
            ColumnDdlOperation.Drop
        };

        private static SchemaExplorerSelectionInfo Column(DataSourceType provider, string name = "c1")
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = provider,
                Kind = SchemaExplorerSelectionKind.ColumnInfo,
                IsColumnInfoRow = true,
                IsGroupRow = false,
                SchemaType = "Tables",
                SchemaName = name + ", int",
                SchemaNode = provider == DataSourceType.PostgreSql ? "public" : "MyDB",
                SchemaDbo = provider == DataSourceType.SqlServer ? "dbo" : string.Empty,
                ObjectId = provider == DataSourceType.SqlServer ? "101" : string.Empty,
                GroupLevel = -1,
                DisplayRowIndex = 7
            };
        }

        private static DataRow RawColumn(DataSourceType provider, string columnName = "c1", bool includeRawName = true)
        {
            var table = new DataTable("F7ColumnSnapshot");

            table.Columns.Add("SchemaNode", typeof(string));
            table.Columns.Add("SchemaType", typeof(string));
            table.Columns.Add("SchemaName", typeof(string));
            table.Columns.Add("Schema_Browser", typeof(string));
            table.Columns.Add("SchemaDbo", typeof(string));
            table.Columns.Add("ObjectID", typeof(string));

            if (includeRawName)
            {
                table.Columns.Add(QualifiedRawColumnName, typeof(string));
            }

            var row = table.NewRow();

            row["SchemaNode"] = provider == DataSourceType.PostgreSql ? "public" : "MyDB";
            row["SchemaType"] = "Tables";
            row["SchemaName"] = provider == DataSourceType.SqlServer ? "dbo.a_test" : "a_test";
            row["Schema_Browser"] = columnName + ", int";
            row["SchemaDbo"] = provider == DataSourceType.SqlServer ? "dbo" : string.Empty;
            row["ObjectID"] = provider == DataSourceType.SqlServer ? "101" : string.Empty;

            if (includeRawName)
            {
                row[QualifiedRawColumnName] = columnName;
            }

            table.Rows.Add(row);
            return row;
        }

        private static Type BridgeType()
        {
            var type = typeof(SharedSchemaExplorerObjectCommandPolicy).Assembly.GetType(BridgeTypeName, false);

            Assert.IsNotNull(type, "The non-Oracle Column DDL bridge must exist before enabling execution.");
            return type;
        }

        private static MethodInfo BridgeMethod(string name, Type[] parameters)
        {
            var method = BridgeType().GetMethod
            (
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                parameters,
                null
            );

            Assert.IsNotNull(method, "Missing Column DDL bridge method: " + name);
            return method;
        }

        private static bool Qualify(SchemaExplorerSelectionInfo selection, DataRow row)
        {
            //The existing U2 policy must remain compatible with Column Add.
            Assert.IsTrue(NonOracleSharedAddColumnPreviewPolicy.TryQualifyColumn(selection, row));

            var method = BridgeMethod
            (
                "TryQualifySelection",
                new[] { typeof(SchemaExplorerSelectionInfo), typeof(DataRow) }
            );

            return (bool)method.Invoke(null, new object[] { selection, row });
        }

        private static bool SameColumn(SchemaExplorerSelectionInfo before, SchemaExplorerSelectionInfo after)
        {
            var method = BridgeMethod
            (
                "IsSameColumnDataRow",
                new[] { typeof(SchemaExplorerSelectionInfo), typeof(SchemaExplorerSelectionInfo) }
            );

            return (bool)method.Invoke(null, new object[] { before, after });
        }

        private static string ColumnSql(DataSourceType provider, ColumnDdlOperation operation)
        {
            var request = new ColumnDdlRequest
            {
                DataSourceType = provider,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = provider == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = "a_test",
                ColumnName = "c1",
                NewColumnName = "c2",
                Comment = "U3-C comment",
                ColumnType = "int NOT NULL"
            };

            if (provider == DataSourceType.MySql && operation == ColumnDdlOperation.Comment)
            {
                //B3-B2A-R1: only the full-definition MODIFY shape is a positive MySQL Comment fixture.
                if (!MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
                (
                    request, "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'", out var sql
                ))
                {
                    return string.Empty;
                }

                return sql;
            }

            return ColumnDdlSqlBuilder.Build(request);
        }

        private static MethodInfo AllowlistMethod()
        {
            return typeof(NonOracleSchemaExplorerDdlExecuteGate).GetMethod
            (
                "IsQualifiedColumnDdlSql",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(DataSourceType), typeof(ColumnDdlOperation), typeof(string) },
                null
            );
        }

        [TestMethod]
        public void C301_ThreeProviderColumnMenusAlreadyOfferThreeCommands()
        {
            foreach (var provider in Providers)
            {
                var menu = SharedSchemaExplorerContextMenuPolicy.Plan(LeftPanelTargetKind.QueryForm, Column(provider)).ToArray();

                foreach (var command in Commands)
                {
                    CollectionAssert.Contains(menu, command, provider + "/" + command);
                }
            }
        }

        [TestMethod]
        public void C302_QualifiedColumnCanOpenThreeCommandsWithoutEnablingWrongTargets()
        {
            foreach (var provider in Providers)
            {
                var selection = Column(provider);

                Assert.IsTrue(Qualify(selection, RawColumn(provider)), provider.ToString());

                foreach (var command in Commands)
                {
                    Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute
                    (
                        LeftPanelTargetKind.QueryForm, selection, command
                    ), provider + "/" + command);
                }
            }
        }

        [TestMethod]
        public void C303_QualifiedCachedColumnMayOpenGeneratorButNotAuthorizeExecute()
        {
            foreach (var provider in Providers)
            {
                var selection = Column(provider);

                Assert.IsTrue(Qualify(selection, RawColumn(provider)));

                foreach (var command in Commands)
                {
                    Assert.IsTrue(SharedSchemaDdlInteractionPolicy.CanOpenGenerator
                    (
                        LeftPanelTargetKind.QueryForm, selection, command, true
                    ), provider + "/" + command);
                }
            }
        }

        [TestMethod]
        public void C304_ViewAndGroupRowsCannotImpersonateColumnDdlTargets()
        {
            foreach (var provider in Providers)
            {
                foreach (var command in Commands)
                {
                    var view = Column(provider);

                    view.SchemaType = "Views";

                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, view, command));

                    var group = Column(provider);

                    group.Kind = SchemaExplorerSelectionKind.GroupNode;
                    group.IsGroupRow = true;

                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, group, command));
                }
            }
        }

        [TestMethod]
        public void C305_SqlServerMissingObjectIdAndOtherProvidersMissingScopeAreDenied()
        {
            foreach (var provider in Providers)
            {
                var selection = Column(provider);

                selection.SchemaNode = string.Empty;

                foreach (var command in Commands)
                {
                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, selection, command));
                }
            }

            var sqlServer = Column(DataSourceType.SqlServer);

            sqlServer.ObjectId = string.Empty;

            foreach (var command in Commands)
            {
                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, sqlServer, command));
            }
        }

        [TestMethod]
        public void C306_DisplayCaptionWithoutRawColumnIdentityMustBeRejected()
        {
            foreach (var provider in Providers)
            {
                var selection = Column(provider);
                var row = RawColumn(provider, includeRawName: false);

                Assert.IsFalse(Qualify(selection, row), provider.ToString());
            }
        }

        [TestMethod]
        public void C307_MetadataRowMustPreserveTrustedExactColumnName()
        {
            foreach (var provider in Providers)
            {
                var selection = Column(provider);

                Assert.IsTrue(Qualify(selection, RawColumn(provider)), provider.ToString());

                var property = typeof(SchemaExplorerSelectionInfo).GetProperty("NonOracleRawColumnName");

                Assert.IsNotNull(property, "Preserve the actual metadata column name separately from the display caption.");
                Assert.AreEqual("c1", (string)property.GetValue(selection), provider.ToString());
            }
        }

        [TestMethod]
        public void C308_ColumnNameContainingCommaMustNeverBeParsedFromCaption()
        {
            const string rawName = "gross, net";

            foreach (var provider in Providers)
            {
                var selection = Column(provider, rawName);

                Assert.IsTrue(Qualify(selection, RawColumn(provider, rawName)), provider.ToString());

                var property = typeof(SchemaExplorerSelectionInfo).GetProperty("NonOracleRawColumnName");

                Assert.IsNotNull(property);
                Assert.AreEqual(rawName, (string)property.GetValue(selection), provider.ToString());
            }
        }

        [TestMethod]
        public void C309_SameCaptionButDifferentRawColumnCannotReuseCapturedSelection()
        {
            foreach (var provider in Providers)
            {
                var before = Column(provider);
                var after = Column(provider);
                var beforeRow = RawColumn(provider, "c1");
                var changedRow = RawColumn(provider, "c2");

                //Make the two visible captions identical; only raw metadata exposes the change.
                changedRow["Schema_Browser"] = "c1, int";

                Assert.IsTrue(Qualify(before, beforeRow));

                //Either reject inconsistent raw metadata immediately or reject it at the captured/current identity comparison. Never silently accept a different Column.
                var afterQualified = Qualify(after, changedRow);

                Assert.IsTrue(!afterQualified || !SameColumn(before, after), provider.ToString());
            }
        }

        [TestMethod]
        public void C310_ExistingSqlBuilderSupportsThreeColumnOperationsOnThreeProviders()
        {
            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(ColumnSql(provider, operation)), provider + "/" + operation);
                }
            }
        }

        [TestMethod]
        public void C311_ExactTableScopeAndSqlBuilderOutputAreStillRequired()
        {
            foreach (var provider in Providers)
            {
                var sql = ColumnSql(provider, ColumnDdlOperation.Drop);
                var schema = provider == DataSourceType.SqlServer ? "dbo" : "public";

                var valid = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", schema, "a_test",
                    "MyDB", schema, "a_test", "MyDB", sql, sql
                );

                Assert.AreEqual(string.Empty, valid, provider.ToString());

                var wrongTable = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", schema, "a_test",
                    "MyDB", schema, "different_table", "MyDB", sql, sql
                );

                Assert.AreEqual("F7U2C2_TABLE_MISMATCH", wrongTable, provider.ToString());

                var editedSql = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", schema, "a_test",
                    "MyDB", schema, "a_test", "MyDB", sql + " ", sql
                );

                Assert.AreEqual("F7U2C2_SQL_CHANGED", editedSql, provider.ToString());
            }
        }

        [TestMethod]
        public void C312_PendingTransactionMustRejectBeforeAnyConnectionOrDispatch()
        {
            var called = false;

            foreach (var provider in Providers)
            {
                called = false;

                var status = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
                (
                    provider, true, false,
                    () => { called = true; return string.Empty; },
                    () => { called = true; return string.Empty; },
                    () => { called = true; return Guid.NewGuid(); },
                    _ => { called = true; return string.Empty; }
                );

                Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", status, provider.ToString());
                Assert.IsFalse(called, provider.ToString());
            }
        }

        [TestMethod]
        public void C313_OperationSpecificColumnSqlAllowlistAcceptsNamedVariants()
        {
            var method = AllowlistMethod();

            Assert.IsNotNull(method, "The Reader must not receive arbitrary Column DDL: define an operation-aware allowlist first.");
            Assert.AreEqual(typeof(bool), method.ReturnType);

            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    Assert.AreEqual(true, method.Invoke(null, new object[] { provider, operation, ColumnSql(provider, operation) }), provider + "/" + operation);
                }
            }

            const string oldMySqlComment = "ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1` int COMMENT 'U3-C comment';";
            Assert.AreEqual(false, method.Invoke(null, new object[] { DataSourceType.MySql, ColumnDdlOperation.Comment, oldMySqlComment }), "MySql/Comment/legacy type-only CHANGE");
        }

        [TestMethod]
        public void C314_ColumnSqlAllowlistRejectsOtherOperationsAndArbitraryStatements()
        {
            var method = AllowlistMethod();

            Assert.IsNotNull(method, "Fail-closed operation-aware Column SQL allowlist is required.");

            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    foreach (var otherOperation in Operations)
                    {
                        if (otherOperation != operation)
                        {
                            Assert.AreEqual(false, method.Invoke(null, new object[] { provider, operation, ColumnSql(provider, otherOperation) }), provider + "/" + operation + " versus " + otherOperation);
                        }
                    }

                    foreach (var invalid in new[] { "COMMIT;", "DELETE FROM a_test;", "DROP TABLE a_test;", "SELECT 1;", ColumnSql(provider, operation) + " ROLLBACK;" })
                    {
                        Assert.AreEqual(false, method.Invoke(null, new object[] { provider, operation, invalid }), provider + "/" + operation + " invalid=" + invalid);
                    }
                }
            }
        }

        [TestMethod]
        public void C315_MySqlCommentAndRenameCannotBuildWithoutColumnDefinition()
        {
            foreach (var operation in new[] { ColumnDdlOperation.Comment, ColumnDdlOperation.Rename })
            {
                var request = new ColumnDdlRequest
                {
                    DataSourceType = DataSourceType.MySql,
                    Operation = operation,
                    SchemaDatabase = "MyDB",
                    SchemaName = "",
                    TableName = "a_test",
                    ColumnName = "c1",
                    NewColumnName = "c2",
                    Comment = "comment",
                    ColumnType = string.Empty
                };

                Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request), operation.ToString());
            }
        }

        [TestMethod]
        public void C316_PreexistingColumnAddFromColumnRowMustRemainAvailable()
        {
            foreach (var provider in Providers)
            {
                var selection = Column(provider);

                Assert.IsTrue(NonOracleSharedAddColumnPreviewPolicy.TryQualifyColumn(selection, RawColumn(provider)));

                Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute
                (
                    LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerMenuCommand.AddColumn
                ), provider.ToString());
            }
        }
    }
}
