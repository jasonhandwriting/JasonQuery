using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //Existing Column last-mile DDL safety contracts. No connections, SQL dispatch, or side effects.
    //Reflection keeps the baseline Golden Red compilable before the new validation methods exist.
    [TestClass]
    [TestCategory("F7U3C2B")]
    public sealed class SchemaExplorerColumnDdlExecutionGoldenTests
    {
        private static readonly DataSourceType[] Providers =
        {
            DataSourceType.PostgreSql,
            DataSourceType.SqlServer,
            DataSourceType.MySql
        };

        private static readonly ColumnDdlOperation[] Operations =
        {
            ColumnDdlOperation.Comment,
            ColumnDdlOperation.Rename,
            ColumnDdlOperation.Drop
        };

        private static ColumnDdlRequest Request(DataSourceType provider, ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = provider,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = provider == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = "a_test",
                ColumnName = "c1",
                NewColumnName = "c2",
                Comment = "Column C2-B",
                ColumnType = "int NOT NULL"
            };
        }

        private static string Sql(DataSourceType provider, ColumnDdlOperation operation)
        {
            var request = Request(provider, operation);

            if (provider == DataSourceType.MySql && operation == ColumnDdlOperation.Comment)
            {
                //B3-B2A-R1: the old type-only CHANGE COLUMN is Preview Only, not an allowed Comment SQL shape.
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

        private static MethodInfo ExistingColumnIntentMethod()
        {
            var method = typeof(NonOracleSchemaExplorerDdlExecuteGate).GetMethod
            (
                "ValidateExistingColumnIntent",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(DataSourceType), typeof(ColumnDdlOperation), typeof(bool),
                    typeof(string), typeof(string), typeof(string), typeof(string)
                },
                null
            );

            Assert.IsNotNull(method, "Existing Column DDL needs a separate operation-aware, exact-Column validation gate; do not broaden the Add Column path.");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return method;
        }

        private static string ValidateExistingColumn(DataSourceType provider, ColumnDdlOperation operation, bool rawQualified, string selectedColumn, string formColumn, string displayedSql, string rebuiltSql)
        {
            return (string)ExistingColumnIntentMethod().Invoke
            (
                null,
                new object[] { provider, operation, rawQualified, selectedColumn, formColumn, displayedSql, rebuiltSql }
            );
        }

        private static MethodInfo MySqlDefinitionMethod()
        {
            var method = typeof(NonOracleSchemaExplorerDdlExecuteGate).GetMethod
            (
                "ValidateMySqlColumnDefinitionSafety",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(ColumnDdlOperation), typeof(bool), typeof(string), typeof(string) },
                null
            );

            Assert.IsNotNull(method, "MySQL CHANGE COLUMN needs authoritative complete-definition evidence; a display type is not sufficient.");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return method;
        }

        private static string ValidateMySqlDefinition(ColumnDdlOperation operation, bool verifiedLiveDefinition, string fullLiveDefinition, string completeSqlDefinition)
        {
            return (string)MySqlDefinitionMethod().Invoke
            (
                null,
                new object[] { operation, verifiedLiveDefinition, fullLiveDefinition, completeSqlDefinition }
            );
        }

        private static void Denied(string reason, string context)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(reason), context);
        }

        [TestMethod]
        public void B401_QualifiedExistingColumnAndExactOperationSqlMayPassThePureGate()
        {
            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    var sql = Sql(provider, operation);

                    Assert.IsFalse(string.IsNullOrWhiteSpace(sql));
                    Assert.AreEqual(string.Empty, ValidateExistingColumn(provider, operation, true, "c1", "c1", sql, sql), provider + "/" + operation);
                }
            }

            //Legacy incomplete MySQL Comment SQL must remain rejected by the pure gate.
            const string oldMySqlComment = "ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1` int COMMENT 'Column C2-B';";
            Denied(ValidateExistingColumn(DataSourceType.MySql, ColumnDdlOperation.Comment, true, "c1", "c1", oldMySqlComment, oldMySqlComment), "MySql/Comment/legacy type-only CHANGE");
        }

        [TestMethod]
        public void B402_ChangedOrDifferentCaseExistingColumnMustFailClosed()
        {
            foreach (var provider in Providers)
            {
                var sql = Sql(provider, ColumnDdlOperation.Drop);

                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Drop, true, "c1", "c2", sql, sql), provider + "/different column");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Drop, true, "c1", "C1", sql, sql), provider + "/case change");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Drop, true, "gross, net", "gross", sql, sql), provider + "/comma caption");
            }
        }

        [TestMethod]
        public void B403_AbsentRawColumnQualificationOrBlankIdentityMustFailClosed()
        {
            foreach (var provider in Providers)
            {
                var sql = Sql(provider, ColumnDdlOperation.Comment);

                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Comment, false, "c1", "c1", sql, sql), provider + "/unqualified");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Comment, true, "", "c1", sql, sql), provider + "/empty selection");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Comment, true, "c1", "", sql, sql), provider + "/empty form");
            }
        }

        [TestMethod]
        public void B404_ExistingColumnGateMustNotAuthorizeAddOrNone()
        {
            foreach (var provider in Providers)
            {
                var sql = Sql(provider, ColumnDdlOperation.Drop);

                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Add, true, "c1", "c1", sql, sql), provider + "/Add");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.None, true, "c1", "c1", sql, sql), provider + "/None");
            }
        }

        [TestMethod]
        public void B405_EditedEmptyOrUnrebuildableSqlCannotPass()
        {
            foreach (var provider in Providers)
            {
                var sql = Sql(provider, ColumnDdlOperation.Rename);

                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Rename, true, "c1", "c1", sql + " ", sql), provider + "/edited");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Rename, true, "c1", "c1", "", sql), provider + "/empty display");
                Denied(ValidateExistingColumn(provider, ColumnDdlOperation.Rename, true, "c1", "c1", sql, ""), provider + "/empty rebuild");
            }
        }

        [TestMethod]
        public void B406_OperationMismatchCannotReuseAnotherColumnDdlSql()
        {
            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    foreach (var other in Operations)
                    {
                        if (operation == other)
                        {
                            continue;
                        }

                        var sql = Sql(provider, other);

                        Denied(ValidateExistingColumn(provider, operation, true, "c1", "c1", sql, sql), provider + "/" + operation + " versus " + other);
                    }

                    Denied(ValidateExistingColumn(provider, operation, true, "c1", "c1", "COMMIT;", "COMMIT;"), provider + "/arbitrary");
                }
            }
        }

        [TestMethod]
        public void B407_MySqlCompleteVerifiedDefinitionMayPreserveAllNonCommentAttributes()
        {
            const string complete = "int NOT NULL DEFAULT 7";

            foreach (var operation in new[] { ColumnDdlOperation.Comment, ColumnDdlOperation.Rename })
            {
                Assert.AreEqual(string.Empty, ValidateMySqlDefinition(operation, true, complete, complete), operation.ToString());
            }

            //DROP COLUMN does not restate a full column definition.
            Assert.AreEqual(string.Empty, ValidateMySqlDefinition(ColumnDdlOperation.Drop, false, string.Empty, string.Empty));
        }

        [TestMethod]
        public void B408_MySqlMissingOrChangedDefinitionCannotReauthorizeChangeColumn()
        {
            const string complete = "int NOT NULL DEFAULT 7";

            foreach (var operation in new[] { ColumnDdlOperation.Comment, ColumnDdlOperation.Rename })
            {
                Denied(ValidateMySqlDefinition(operation, false, complete, complete), operation + "/not verified");
                Denied(ValidateMySqlDefinition(operation, true, string.Empty, complete), operation + "/missing live definition");
                Denied(ValidateMySqlDefinition(operation, true, complete, string.Empty), operation + "/missing SQL definition");
                Denied(ValidateMySqlDefinition(operation, true, complete, "int NULL DEFAULT 7"), operation + "/nullability drift");
                Denied(ValidateMySqlDefinition(operation, true, complete, "int NOT NULL DEFAULT 8"), operation + "/default drift");
            }
        }

        [TestMethod]
        public void B409_ExistingTableScopeValidationRemainsReusableForColumnDdl()
        {
            foreach (var provider in Providers)
            {
                var schema = provider == DataSourceType.SqlServer ? "dbo" : "public";
                var sql = Sql(provider, ColumnDdlOperation.Drop);

                var result = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
                (
                    provider, "MyDB", schema, "a_test", "MyDB", schema,
                    "a_test", "MyDB", sql, sql
                );

                Assert.AreEqual(string.Empty, result, provider.ToString());
            }
        }

        [TestMethod]
        public void B410_PostgreSqlLiveDatabaseMismatchRemainsDenied()
        {
            const DataSourceType provider = DataSourceType.PostgreSql;
            var sql = Sql(provider, ColumnDdlOperation.Drop);

            var result = NonOracleSchemaExplorerDdlExecuteGate.ValidateFormIntent
            (
                provider, "MyDB", "public", "a_test", "MyDB", "public", "a_test", "OtherDB", sql, sql
            );

            Denied(result, "PostgreSQL database switch");
        }

        [TestMethod]
        public void B411_PendingOrUnknownTransactionStopsBeforeAnyConnectionOperation()
        {
            foreach (var provider in Providers)
            {
                var called = false;

                var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
                (
                    provider, true, true,
                    () => { called = true; return string.Empty; },
                    () => { called = true; return string.Empty; },
                    () => { called = true; return Guid.NewGuid(); },
                    id => { called = true; return string.Empty; }
                );

                Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", result, provider.ToString());
                Assert.IsFalse(called, provider.ToString());
            }
        }

        [TestMethod]
        public void B412_MySqlImplicitCommitRequiresExplicitAcknowledgement()
        {
            var called = false;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql, false, false,
                () => { called = true; return string.Empty; },
                () => { called = true; return string.Empty; },
                () => { called = true; return Guid.NewGuid(); },
                id => { called = true; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_MYSQL_IMPLICIT_COMMIT_CONFIRMATION_REQUIRED", result);
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void B413_PhysicalConnectionChangeAfterVerificationCannotDispatch()
        {
            var initialId = Guid.NewGuid();
            var replacementId = Guid.NewGuid();
            var reads = 0;
            var dispatched = false;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.PostgreSql, false, true,
                () => string.Empty,
                () => string.Empty,
                () => { reads++; return reads == 1 ? initialId : replacementId; },
                id => { dispatched = true; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_CONNECTION_CHANGED", result);
            Assert.IsFalse(dispatched);
        }

        [TestMethod]
        public void B414_DispatchExceptionMustReportUnknownOutcomeWithoutRetry()
        {
            var calls = 0;
            var connectionId = Guid.NewGuid();

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.SqlServer, false, true,
                () => string.Empty,
                () => string.Empty,
                () => connectionId,
                id => { calls++; throw new InvalidOperationException("transport outcome unknown"); }
            );

            Assert.AreEqual("F7U2C2_EXECUTION_OUTCOME_UNKNOWN", result);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        public void B415_ExistingProviderColumnAllowlistAcceptsOnlyNamedOperations()
        {
            foreach (var provider in Providers)
            {
                foreach (var operation in Operations)
                {
                    var sql = Sql(provider, operation);

                    Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(provider, operation, sql), provider + "/" + operation);
                    Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(provider, ColumnDdlOperation.None, sql), provider + "/None");
                }
            }

            const string oldMySqlComment = "ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1` int COMMENT 'Column C2-B';";
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, oldMySqlComment), "MySql/Comment/legacy type-only CHANGE");
        }

        [TestMethod]
        public void B416_PreexistingRawColumnBridgeStillRejectsChangedSelectedIdentity()
        {
            foreach (var provider in Providers)
            {
                var before = Selection(provider);
                var after = Selection(provider);

                Assert.IsTrue(NonOracleSharedColumnDdlBridgePolicy.TryQualifySelection(before, RawRow(provider, "c1")), provider + "/before");
                Assert.IsTrue(NonOracleSharedColumnDdlBridgePolicy.TryQualifySelection(after, RawRow(provider, "c2")), provider + "/after");
                Assert.IsFalse(NonOracleSharedColumnDdlBridgePolicy.IsSameColumnDataRow(before, after), provider + "/changed raw name");
            }
        }

        private static SchemaExplorerSelectionInfo Selection(DataSourceType provider)
        {
            return new SchemaExplorerSelectionInfo
            {
                SourceType = provider,
                Kind = SchemaExplorerSelectionKind.ColumnInfo,
                IsColumnInfoRow = true,
                IsGroupRow = false,
                SchemaType = "Tables",
                SchemaName = "c1, int",
                SchemaNode = provider == DataSourceType.PostgreSql ? "public" : "MyDB",
                SchemaDbo = provider == DataSourceType.SqlServer ? "dbo" : string.Empty,
                ObjectId = provider == DataSourceType.SqlServer ? "101" : string.Empty,
                GroupLevel = -1,
                DisplayRowIndex = 7
            };
        }

        private static DataRow RawRow(DataSourceType provider, string column)
        {
            var table = new DataTable();

            foreach (var name in new[] { "SchemaNode", "SchemaType", "SchemaName", "Schema_Browser", "SchemaDbo", "ObjectID", "NonOracleMetadataColumnName" })
            {
                table.Columns.Add(name, typeof(string));
            }

            var row = table.NewRow();

            row["SchemaNode"] = provider == DataSourceType.PostgreSql ? "public" : "MyDB";
            row["SchemaType"] = "Tables";
            row["SchemaName"] = provider == DataSourceType.SqlServer ? "dbo.a_test" : "a_test";
            row["Schema_Browser"] = "c1, int";
            row["SchemaDbo"] = provider == DataSourceType.SqlServer ? "dbo" : string.Empty;
            row["ObjectID"] = provider == DataSourceType.SqlServer ? "101" : string.Empty;
            row["NonOracleMetadataColumnName"] = column;

            table.Rows.Add(row);
            return row;
        }
    }
}
