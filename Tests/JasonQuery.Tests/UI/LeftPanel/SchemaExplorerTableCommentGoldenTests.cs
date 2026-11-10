using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-B1: pure contract tests. No database connections and no DDL are executed.
    //Cached selection allows SQL preview; only Execute-time validation can authorize dispatch.
    [TestClass]
    [TestCategory("F7U3B1")]
    public sealed class SchemaExplorerTableCommentGoldenTests
    {
        private static readonly DataSourceType[] Providers =
        {
            DataSourceType.PostgreSql,
            DataSourceType.SqlServer,
            DataSourceType.MySql
        };

        private static SchemaExplorerSelectionInfo Table(DataSourceType source)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = source,
                SchemaType = "Tables",
                SchemaName = "a_test",
                SchemaNode = source == DataSourceType.PostgreSql ? "public" : "MyDB",
                SchemaDbo = "dbo",
                ObjectId = "101",
                IsGroupRow = false,
                IsColumnInfoRow = false,
                DisplayRowIndex = 4
            };
        }

        private static string CommentSql(DataSourceType source, string tableName = "a_test")
        {
            return TableDdlSqlBuilder.Build(new TableDdlRequest
            {
                DataSourceType = source,
                Operation = TableDdlOperation.Comment,
                SchemaDatabase = "MyDB",
                SchemaName = source == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = tableName,
                Comment = "U3-B1 table comment",
                HasExistingSqlServerComment = false
            });
        }

        private static bool Allowed(DataSourceType source)
        {
            return SharedSchemaExplorerObjectCommandPolicy.CanExecute
            (
                LeftPanelTargetKind.QueryForm,
                Table(source),
                SharedSchemaExplorerMenuCommand.CommentTable
            );
        }

        [TestMethod]
        public void B101_PostgreSqlQualifiedTableCanOpenComment()
        {
            Assert.IsTrue(Allowed(DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void B102_SqlServerQualifiedTableCanOpenComment()
        {
            Assert.IsTrue(Allowed(DataSourceType.SqlServer));
        }

        [TestMethod]
        public void B103_MySqlQualifiedTableCanOpenComment()
        {
            Assert.IsTrue(Allowed(DataSourceType.MySql));
        }

        [TestMethod]
        public void B104_ThreeProviderTableCommentGeneratorCanOpenFromQualifiedCachedSnapshot()
        {
            foreach (var provider in Providers)
            {
                Assert.IsTrue(SharedSchemaDdlInteractionPolicy.CanOpenGenerator
                (
                    LeftPanelTargetKind.QueryForm,
                    Table(provider),
                    SharedSchemaExplorerMenuCommand.CommentTable,
                    true
                ), provider.ToString());
            }
        }

        [TestMethod]
        public void B105_ViewAndColumnCannotImpersonateTableComment()
        {
            foreach (var provider in Providers)
            {
                var view = Table(provider);

                view.SchemaType = "Views";

                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute
                (
                    LeftPanelTargetKind.QueryForm, view, SharedSchemaExplorerMenuCommand.CommentTable
                ));

                var column = Table(provider);

                column.Kind = SchemaExplorerSelectionKind.ColumnInfo;
                column.IsColumnInfoRow = true;
                column.SchemaName = "a_col, INTEGER";

                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute
                (
                    LeftPanelTargetKind.QueryForm, column, SharedSchemaExplorerMenuCommand.CommentTable
                ));
            }
        }

        [TestMethod]
        public void B106_TableCommentRequiresExactProviderScopeAndObjectId()
        {
            foreach (var provider in Providers)
            {
                var missingScope = Table(provider);

                missingScope.SchemaNode = string.Empty;

                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute
                (
                    LeftPanelTargetKind.QueryForm, missingScope, SharedSchemaExplorerMenuCommand.CommentTable
                ));
            }

            var noObjectId = Table(DataSourceType.SqlServer);

            noObjectId.ObjectId = string.Empty;

            Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute
            (
                LeftPanelTargetKind.QueryForm, noObjectId, SharedSchemaExplorerMenuCommand.CommentTable
            ));
        }


        [TestMethod]
        public void B107_CategoryGroupCannotAuthorizeTableComment()
        {
            foreach (var provider in Providers)
            {
                var group = Table(provider);

                group.Kind = SchemaExplorerSelectionKind.GroupNode;
                group.IsGroupRow = true;
                group.GroupLevel = 0;

                Assert.IsFalse(SharedSchemaExplorerObjectCommandPolicy.CanExecute
                (
                    LeftPanelTargetKind.QueryForm,
                    group,
                    SharedSchemaExplorerMenuCommand.CommentTable
                ), provider.ToString());
            }
        }


        [TestMethod]
        public void B108_CommentSqlMustMatchSelectedTableAndExactRebuild()
        {
            foreach (var provider in Providers)
            {
                var expected = CommentSql(provider);

                Assert.IsFalse(string.IsNullOrWhiteSpace(expected));

                var selectedSchema = provider == DataSourceType.SqlServer ? "dbo" : "public";

                var result = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", selectedSchema, "a_test",
                    "MyDB", selectedSchema, "a_test", "MyDB",
                    expected, expected
                );

                Assert.AreEqual(string.Empty, result, provider.ToString());

                var changedSql = CommentSql(provider, "other_table");

                result = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", selectedSchema, "a_test",
                    "MyDB", selectedSchema, "a_test", "MyDB",
                    changedSql, expected
                );

                Assert.AreEqual("F7U2C2_SQL_CHANGED", result, provider.ToString());

                result = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", selectedSchema, "a_test",
                    "MyDB", selectedSchema, "other_table", "MyDB",
                    expected, expected
                );

                Assert.AreEqual("F7U2C2_TABLE_MISMATCH", result, provider.ToString());
            }
        }

        [TestMethod]
        public void B109_PostgreSqlLiveDatabaseSwitchRejectsTableComment()
        {
            var sql = CommentSql(DataSourceType.PostgreSql);

            Assert.AreEqual("F7U2C2_DATABASE_MISMATCH", NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
            (
                DataSourceType.PostgreSql, "MyDB", "public", "a_test",
                "MyDB", "public", "a_test", "other_db", sql, sql
            ));
        }

        [TestMethod]
        public void B110_SqlServerAndMySqlFormDatabaseSwitchRejectsTableComment()
        {
            foreach (var provider in new[] { DataSourceType.SqlServer, DataSourceType.MySql })
            {
                var schema = provider == DataSourceType.SqlServer ? "dbo" : "public";
                var sql = CommentSql(provider);

                Assert.AreEqual("F7U2C2_DATABASE_MISMATCH", NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", schema, "a_test",
                    "other_db", schema, "a_test", "MyDB", sql, sql
                ), provider.ToString());
            }
        }

        [TestMethod]
        public void B111_TransactionUnsafeDoesNotReconnectOrDispatch()
        {
            var called = false;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.PostgreSql,
                true,
                false,
                () => { called = true; return string.Empty; },
                () => { called = true; return string.Empty; },
                () => { called = true; return Guid.NewGuid(); },
                _ => { called = true; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", result);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void B112_MySqlImplicitCommitRequiresExplicitAcknowledgement()
        {
            var called = false;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql,
                false,
                false,
                () => { called = true; return string.Empty; },
                () => { called = true; return string.Empty; },
                () => { called = true; return Guid.NewGuid(); },
                _ => { called = true; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_MYSQL_IMPLICIT_COMMIT_CONFIRMATION_REQUIRED", result);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void B113_TableCommentDispatchMustUseOperationSpecificSqlAllowlist()
        {
            //The minimal U3-B1 implementation will add a pure, easily tested SQL-shape check.
            //The separate Reader guard still rejects batches and verifies the physical connection.
            var method = typeof(NonOracleSchemaExplorerDdlExecuteGate).GetMethod
            (
                "IsQualifiedTableCommentSql",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(DataSourceType), typeof(string) },
                null
            );

            Assert.IsNotNull(method, "U3-B1 Table Comment allowlist entry must exist before GUI execution is enabled.");

            foreach (var provider in Providers)
            {
                Assert.AreEqual(true, method.Invoke(null, new object[] { provider, CommentSql(provider) }), provider.ToString());
                Assert.AreEqual(false, method.Invoke(null, new object[] { provider, "DELETE FROM a_test;" }), provider.ToString());
                Assert.AreEqual(false, method.Invoke(null, new object[] { provider, "DROP TABLE a_test;" }), provider.ToString());
            }
        }
    }
}
