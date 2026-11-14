using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4B2: compile-green / behavioral-red contracts.
    //This suite NEVER opens a database, performs DDL, or changes a Git index.
    //Golden Red target on the uploaded B1 source: 9 PASS / 9 Expected FAIL.
    [TestClass]
    [TestCategory("F7U3C4B2")]
    public sealed class SchemaExplorerMySqlCompleteDefinitionGoldenTests
    {
        private const string ModernSql = "ALTER TABLE `demo`.`b2_table` RENAME COLUMN `old_name` TO `new_name`;";
        private const string CompleteBuilderType = "JasonQuery.Core.Database.DdlPreview.MySqlCompleteColumnDefinitionSqlBuilder";
        private const string SnapshotType = "JasonQuery.Core.Database.DdlPreview.MySqlVerifiedColumnDefinitionSnapshot";

        private static ColumnDdlRequest NewRequest(ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = operation,
                SchemaDatabase = "demo",
                SchemaName = "demo",
                TableName = "b2_table",
                ColumnName = "old_name",
                NewColumnName = "new_name",
                ColumnType = "int", // Legacy preview input is intentionally incomplete.
                Comment = "updated"
            };
        }

        //Intended B3 pure builder signature:
        //internal static bool TryBuildLegacyRename(ColumnDdlRequest request, string verifiedCompleteDefinition, out string sql)
        //internal static bool TryBuildComment(ColumnDdlRequest request, string verifiedCompleteDefinition, out string sql)
        //The pure builder is NOT execution authorization. A Reader-produced receipt, same-connection verification, and the existing F7 Execute gate are mandatory.
        private static bool BuildComplete(string methodName, ColumnDdlRequest request, string completeDefinition, out string sql)
        {
            sql = string.Empty;

            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType(CompleteBuilderType, false);

            Assert.IsNotNull(type, "B2_EXPECTED_RED: complete-definition SQL builder does not exist.");

            var method = type.GetMethod
            (
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(ColumnDdlRequest), typeof(string), typeof(string).MakeByRefType() },
                null
            );

            Assert.IsNotNull(method, "B2_EXPECTED_RED: missing pure builder contract " + methodName);
            Assert.AreEqual(typeof(bool), method.ReturnType);

            var arguments = new object[] { request, completeDefinition, null };
            var accepted = (bool)method.Invoke(null, arguments);

            sql = arguments[2] as string ?? string.Empty;
            return accepted;
        }

        [TestMethod]
        public void B2G01_ModernRenameMustRemainDefinitionFreeAndQualified()
        {
            Assert.AreEqual(MySqlColumnRenameSyntax.RenameColumn, MySqlColumnDdlVersionPolicy.SelectRenameSyntax("8.0.34"));
            Assert.AreEqual(ModernSql, ColumnDdlSqlBuilder.BuildMySqlRenameColumnSql(NewRequest(ColumnDdlOperation.Rename)));

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Rename, ModernSql
            ));
        }

        [TestMethod]
        public void B2G02_LegacyAndMariaDbMustNotUnlockModernOrLegacyExecution()
        {
            Assert.AreEqual(MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition, MySqlColumnDdlVersionPolicy.SelectRenameSyntax("5.7.44"));
            Assert.AreEqual(MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition, MySqlColumnDdlVersionPolicy.SelectRenameSyntax("8.0.2"));
            Assert.AreEqual(MySqlColumnRenameSyntax.Unknown, MySqlColumnDdlVersionPolicy.SelectRenameSyntax("5.5.5-10.11.2-MariaDB"));

            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Rename));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
        }

        [TestMethod]
        public void B2G03_VerifiedPlainLegacyRenameMustPreserveCompleteDefinition()
        {
            var accepted = BuildComplete("TryBuildLegacyRename", NewRequest(ColumnDdlOperation.Rename), "int NOT NULL DEFAULT 7", out var sql);

            Assert.IsTrue(accepted);
            Assert.AreEqual("ALTER TABLE `demo`.`b2_table` CHANGE COLUMN `old_name` `new_name` int NOT NULL DEFAULT 7;", sql);
        }

        [TestMethod]
        public void B2G04_VerifiedPlainCommentMustUseModifyAndPreserveDefinition()
        {
            var accepted = BuildComplete("TryBuildComment", NewRequest(ColumnDdlOperation.Comment), "varchar(32) NOT NULL DEFAULT 'x'", out var sql);

            Assert.IsTrue(accepted);
            Assert.AreEqual("ALTER TABLE `demo`.`b2_table` MODIFY COLUMN `old_name` varchar(32) NOT NULL DEFAULT 'x' COMMENT 'updated';", sql);
        }

        [TestMethod]
        public void B2G05_MissingCompleteDefinitionMustFailClosed()
        {
            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", NewRequest(ColumnDdlOperation.Rename), string.Empty, out var sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void B2G06_RenameMustRetainUnsignedNotNullDefaultAndOldComment()
        {
            var complete = "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'";
            var accepted = BuildComplete("TryBuildLegacyRename", NewRequest(ColumnDdlOperation.Rename), complete, out var sql);

            Assert.IsTrue(accepted);
            Assert.Contains("`new_name` " + complete + ";", sql);
        }

        [TestMethod]
        public void B2G07_CharsetCollationOutsideInitialQualificationMustBeRejected()
        {
            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", NewRequest(ColumnDdlOperation.Rename), "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL", out var sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void B2G08_EnumAndSetDefinitionsOutsideInitialQualificationMustBeRejected()
        {
            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", NewRequest(ColumnDdlOperation.Rename), "enum('a,b','it''s') NOT NULL", out var sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void B2G09_AutoIncrementAndOnUpdateMustStayUnsupported()
        {
            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", NewRequest(ColumnDdlOperation.Rename), "int unsigned NOT NULL AUTO_INCREMENT", out var sql));
            Assert.AreEqual(string.Empty, sql);
            Assert.IsFalse(BuildComplete("TryBuildComment", NewRequest(ColumnDdlOperation.Comment), "timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP", out sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void B2G10_GeneratedInvisibleAndExpressionDefaultsMustStayUnsupported()
        {
            var request = NewRequest(ColumnDdlOperation.Rename);

            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", request, "int GENERATED ALWAYS AS (1 + 2) STORED", out var sql));
            Assert.AreEqual(string.Empty, sql);
            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", request, "varchar(20) INVISIBLE", out sql));
            Assert.AreEqual(string.Empty, sql);
            Assert.IsFalse(BuildComplete("TryBuildLegacyRename", request, "varchar(20) DEFAULT (uuid())", out sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void B2G11_MismatchedDefinitionOrSqlMustBeRejected()
        {
            Assert.AreEqual("F7U3C2B_MYSQL_DEFINITION_CHANGED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateMySqlColumnDefinitionSafety
                (
                    ColumnDdlOperation.Rename, true,
                    "int NOT NULL DEFAULT 7", "int NOT NULL DEFAULT 8"
                ));

            Assert.AreEqual("F7U3C2B_SQL_CHANGED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, true,
                    "old_name", "old_name", ModernSql, ModernSql.Replace("`new_name`", "`different`")
                ));
        }

        [TestMethod]
        public void B2G12_UnverifiedConnectionOrTargetMustFailClosed()
        {
            var reader = new MySqlReader();

            Assert.AreEqual(Guid.Empty, reader.CurrentOpenConnectionId);

            Assert.IsFalse(reader.TryReadServerVersionOnVerifiedConnection
            (
                Guid.Empty, out var version, out var error
            ));

            Assert.AreEqual(string.Empty, version);
            Assert.IsFalse(string.IsNullOrWhiteSpace(error));

            Assert.AreEqual("F7U2C2_CONNECTION_CHANGED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateTarget
                (
                    DataSourceType.MySql, "demo", "demo", "b2_table", null,
                    "demo", "demo", "b2_table", null, true, false, ModernSql, ModernSql
                ));
        }

        [TestMethod]
        public void B2G13_PendingTransactionMustNotAttemptConnectionOrDdl()
        {
            var calls = 0;

            var reason = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql, true, true,
                () => { calls++; return string.Empty; },
                () => { calls++; return string.Empty; },
                () => { calls++; return Guid.NewGuid(); },
                _ => { calls++; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", reason);
            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void B2G14_DeniedImplicitCommitMustNotCallAnyDelegate()
        {
            var calls = 0;

            var reason = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql, false, false,
                () => { calls++; return string.Empty; },
                () => { calls++; return string.Empty; },
                () => { calls++; return Guid.NewGuid(); },
                _ => { calls++; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_MYSQL_IMPLICIT_COMMIT_CONFIRMATION_REQUIRED", reason);
            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void B2G15_MultipleStatementsOrCrossOperationSqlMustBeRejected()
        {
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Rename, ModernSql + " COMMIT;"
            ));

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, ModernSql
            ));
        }

        [TestMethod]
        public void B2G16_UnknownDispatchOutcomeMustNeverRetry()
        {
            var connection = Guid.NewGuid();
            var calls = 0;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql, false, true,
                () => string.Empty,
                () => string.Empty,
                () => connection,
                _ => { calls++; throw new InvalidOperationException("simulated lost reply"); }
            );

            Assert.AreEqual("F7U2C2_EXECUTION_OUTCOME_UNKNOWN", result);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        public void B2G17_ReaderMustProduceImmutableSameConnectionDefinitionReceipt()
        {
            var snapshot = typeof(ColumnDdlSqlBuilder).Assembly.GetType(SnapshotType, false);

            Assert.IsNotNull(snapshot, "B2_EXPECTED_RED: immutable verified-definition receipt type missing.");
            Assert.IsTrue(snapshot.IsSealed);
            Assert.AreEqual(0, snapshot.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length, "A form must not construct a trusted receipt directly.");

            var readerMethod = typeof(MySqlReader).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(m => string.Equals(m.Name, "TryReadVerifiedColumnDefinitionOnConnection", StringComparison.Ordinal));

            Assert.IsNotNull(readerMethod, "B2_EXPECTED_RED: Reader missing verified-connection column definition API.");
            Assert.AreEqual(typeof(bool), readerMethod.ReturnType);

            var arguments = readerMethod.GetParameters();

            Assert.IsTrue(arguments.Length >= 5);
            Assert.AreEqual(typeof(Guid), arguments[0].ParameterType);
            Assert.IsTrue(arguments.Any(p => p.ParameterType.IsByRef && p.ParameterType.GetElementType() == snapshot));
        }

        [TestMethod]
        public void B2G18_OtherProvidersAndMySqlDropMustKeepExistingPolicy()
        {
            Assert.IsTrue(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.PostgreSql, EditColumnMode.Rename));
            Assert.IsTrue(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.SqlServer, EditColumnMode.Comment));
            Assert.IsTrue(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Drop));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Rename));
        }
    }
}
