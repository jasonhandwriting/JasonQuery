using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-B2: non-destructive Golden contracts for three remaining Table DDL operations.
    //Never connect to a database or execute SQL in these tests.
    [TestClass]
    [TestCategory("F7U3B2")]
    public sealed class SchemaExplorerTableDdlOperationsGoldenTests
    {
        private static readonly DataSourceType[] Providers =
        {
            DataSourceType.PostgreSql,
            DataSourceType.SqlServer,
            DataSourceType.MySql
        };

        private static readonly SharedSchemaExplorerMenuCommand[] Commands =
        {
            SharedSchemaExplorerMenuCommand.RenameTable,
            SharedSchemaExplorerMenuCommand.DropTable,
            SharedSchemaExplorerMenuCommand.TruncateTable
        };

        private static readonly TableDdlOperation[] Operations =
        {
            TableDdlOperation.Rename,
            TableDdlOperation.Drop,
            TableDdlOperation.Truncate
        };

        private static SchemaExplorerSelectionInfo Table(DataSourceType provider)
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = provider,
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SchemaType = "Tables",
                SchemaName = "a_test",
                SchemaNode = provider == DataSourceType.PostgreSql ? "public" : "MyDB",
                SchemaDbo = "dbo",
                ObjectId = "101",
                DisplayRowIndex = 4,
                IsGroupRow = false,
                IsColumnInfoRow = false
            };
        }

        private static bool Allowed(DataSourceType provider, SharedSchemaExplorerMenuCommand command)
        {
            return SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, Table(provider), command);
        }

        private static string Sql(DataSourceType provider, TableDdlOperation operation)
        {
            return TableDdlSqlBuilder.Build(new TableDdlRequest
            {
                DataSourceType = provider,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = provider == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = "a_test",
                NewTableName = "a_test_new"
            });
        }

        private static MethodInfo DdlAllowlistMethod()
        {
            return typeof(NonOracleSchemaExplorerDdlExecuteGate).GetMethod
            (
                "IsQualifiedTableDdlSql",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(DataSourceType), typeof(TableDdlOperation), typeof(string) },
                null
            );
        }

        [TestMethod]
        public void B201_PostgreSqlTableOffersRenameDropAndTruncate()
        {
            foreach (var command in Commands)
            {
                Assert.IsTrue(Allowed(DataSourceType.PostgreSql, command), command.ToString());
            }
        }

        [TestMethod]
        public void B202_SqlServerTableOffersRenameDropAndTruncate()
        {
            foreach (var command in Commands)
            {
                Assert.IsTrue(Allowed(DataSourceType.SqlServer, command), command.ToString());
            }
        }

        [TestMethod]
        public void B203_MySqlTableOffersRenameDropAndTruncate()
        {
            foreach (var command in Commands)
            {
                Assert.IsTrue(Allowed(DataSourceType.MySql, command), command.ToString());
            }
        }

        [TestMethod]
        public void B204_QualifiedCachedTableCanOpenThreeDdlGenerators()
        {
            foreach (var provider in Providers)
            {
                foreach (var command in Commands)
                {
                    Assert.IsTrue(SharedSchemaDdlInteractionPolicy.CanOpenGenerator
                    (
                        LeftPanelTargetKind.QueryForm,
                        Table(provider),
                        command,
                        true
                    ), provider + "/" + command);
                }
            }
        }

        [TestMethod]
        public void B205_ViewAndColumnCannotImpersonateTableDdl()
        {
            foreach (var provider in Providers)
            {
                foreach (var command in Commands)
                {
                    var view = Table(provider);

                    view.SchemaType = "Views";

                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, view, command));

                    var column = Table(provider);

                    column.Kind = SchemaExplorerSelectionKind.ColumnInfo;
                    column.IsColumnInfoRow = true;
                    column.SchemaName = "id, integer";

                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, column, command));
                }
            }
        }

        [TestMethod]
        public void B206_BlankDatabaseOrSchemaCannotAuthorizeDdl()
        {
            foreach (var provider in Providers)
            {
                var selection = Table(provider);

                selection.SchemaNode = string.Empty;

                foreach (var command in Commands)
                {
                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, selection, command));
                }
            }
        }

        [TestMethod]
        public void B207_SqlServerTableRequiresObjectId()
        {
            var selection = Table(DataSourceType.SqlServer);

            selection.ObjectId = string.Empty;

            foreach (var command in Commands)
            {
                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, selection, command));
            }
        }

        [TestMethod]
        public void B208_CategoryGroupCannotBecomeTableDdlTarget()
        {
            foreach (var provider in Providers)
            {
                var selection = Table(provider);

                selection.Kind = SchemaExplorerSelectionKind.GroupNode;
                selection.IsGroupRow = true;
                selection.GroupLevel = 0;

                foreach (var command in Commands)
                {
                    Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.QueryForm, selection, command));
                }
            }
        }

        [TestMethod]
        public void B209_ExistingCommentAndAddColumnRemainAvailable()
        {
            foreach (var provider in Providers)
            {
                Assert.IsTrue(Allowed(provider, SharedSchemaExplorerMenuCommand.CommentTable), provider.ToString());
                Assert.IsTrue(Allowed(provider, SharedSchemaExplorerMenuCommand.AddColumn), provider.ToString());
            }
        }

        [TestMethod]
        public void B210_ExistingSqlBuilderSupportsNineTargetedDdlVariants()
        {
            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    var sql = Sql(provider, operation);

                    Assert.IsFalse(string.IsNullOrWhiteSpace(sql), provider + "/" + operation);
                }
            }
        }

        [TestMethod]
        public void B211_ExactSqlAndTableIdentityStillRequired()
        {
            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    var sql = Sql(provider, operation);
                    var schema = provider == DataSourceType.SqlServer ? "dbo" : "public";

                    var valid = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                    (
                        provider, "MyDB", schema, "a_test",
                        "MyDB", schema, "a_test", "MyDB", sql, sql
                    );

                    Assert.AreEqual(string.Empty, valid, provider + "/" + operation);

                    var otherTable = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                    (
                        provider, "MyDB", schema, "a_test",
                        "MyDB", schema, "other_table", "MyDB", sql, sql
                    );

                    Assert.AreEqual("F7U2C2_TABLE_MISMATCH", otherTable, provider + "/" + operation);

                    var changedSql = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                    (
                        provider, "MyDB", schema, "a_test",
                        "MyDB", schema, "a_test", "MyDB", sql + " ", sql
                    );

                    Assert.AreEqual("F7U2C2_SQL_CHANGED", changedSql, provider + "/" + operation);
                }
            }
        }

        [TestMethod]
        public void B212_PendingTransactionCannotTriggerConnectionOrDispatch()
        {
            var invoked = false;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.PostgreSql,
                true,
                false,
                () => { invoked = true; return string.Empty; },
                () => { invoked = true; return string.Empty; },
                () => { invoked = true; return Guid.NewGuid(); },
                _ => { invoked = true; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", result);
            Assert.IsFalse(invoked);
        }

        [TestMethod]
        public void B213_TableDdlAllowlistAcceptsOnlyNineNamedOperationCombinations()
        {
            var method = DdlAllowlistMethod();

            Assert.IsNotNull(method, "Operation-aware Table DDL allowlist must exist before enabling Execute.");
            Assert.AreEqual(typeof(bool), method.ReturnType);

            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    var sql = Sql(provider, operation);

                    Assert.AreEqual(true, method.Invoke(null, new object[] { provider, operation, sql }), provider + "/" + operation);
                }
            }
        }

        [TestMethod]
        public void B214_TableDdlAllowlistRejectsCrossOperationAndArbitrarySql()
        {
            var method = DdlAllowlistMethod();

            Assert.IsNotNull(method, "Fail-closed operation-aware SQL allowlist is required.");

            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    foreach (var otherOperation in Operations)
                    {
                        if (operation == otherOperation)
                        {
                            continue;
                        }

                        Assert.AreEqual(false, method.Invoke(null, new object[] { provider, operation, Sql(provider, otherOperation) }), provider + "/" + operation + " versus " + otherOperation);
                    }

                    foreach (var sql in new[] { "DELETE FROM a_test;", "DROP VIEW a_test;", "COMMIT;", "SELECT 1;", Sql(provider, operation) + " ROLLBACK;" })
                    {
                        Assert.AreEqual(false, method.Invoke(null, new object[] { provider, operation, sql }), provider + "/" + operation + ": " + sql);
                    }
                }
            }
        }
    }
}
