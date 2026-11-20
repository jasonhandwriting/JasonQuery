using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C3-B1: common MySQL column grammar and SQL-preservation contract.
    //Grammar is not live metadata proof, an Execute permit, or a Reader receipt.
    //No database connection, DDL dispatch, Git changes, or test-data mutations.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlCommonColumnDefinitionGoldenTests
    {
        private static ColumnDdlRequest Request(ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = operation,
                SchemaDatabase = "sakila",
                TableName = "test_columns",
                ColumnName = "old_column",
                NewColumnName = "new_column",
                Comment = "updated"
            };
        }

        private static void AssertQualified(string original, string expectedWithoutComment)
        {
            Assert.IsTrue
            (
                MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
                (
                    original, out var withoutComment
                ),
                "A syntactically supported MySQL common-column definition should qualify for later metadata proof: " + original
            );

            Assert.AreEqual(expectedWithoutComment, withoutComment);
        }

        //GREEN: established behavior and mandatory safety boundaries.
        [TestMethod]
        public void B1G01_ExistingIntegerDefinitionRemainsQualified()
        {
            AssertQualified("int NOT NULL DEFAULT 7 COMMENT 'before'", "int NOT NULL DEFAULT 7");
        }

        [TestMethod]
        public void B1G02_ExistingExplicitCharacterDefinitionRemainsQualified()
        {
            AssertQualified
            (
                "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc' COMMENT 'before'",
                "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc'"
            );
        }

        [TestMethod]
        public void B1G03_ModernRenameRemainsDefinitionFree()
        {
            Assert.AreEqual(MySqlColumnRenameSyntax.RenameColumn, MySqlColumnDdlVersionPolicy.SelectRenameSyntax("8.0.34"));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`test_columns` RENAME COLUMN `old_column` TO `new_column`;",
                ColumnDdlSqlBuilder.BuildMySqlRenameColumnSql(Request(ColumnDdlOperation.Rename))
            );
        }

        [TestMethod]
        public void B1G04_SecondStatementNeverQualifiesAsDefinition()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "int NOT NULL DEFAULT 7; DROP TABLE test_columns;", out _
            ));
        }

        [TestMethod]
        public void B1G05_SpecialAttributesRemainOutsideThisQualification()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "int NOT NULL AUTO_INCREMENT", out _
            ));

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "varchar(20) DEFAULT (uuid())", out _
            ));
        }

        [TestMethod]
        public void B1G06_PendingTransactionCannotInvokeAnyDdlDelegate()
        {
            var calls = 0;

            var status = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql,
                true,
                true,
                () => { calls++; return string.Empty; },
                () => { calls++; return string.Empty; },
                () => { calls++; return Guid.NewGuid(); },
                _ => { calls++; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", status);
            Assert.AreEqual(0, calls);
        }

        [TestMethod]
        public void B1G07_DanglingDefaultClauseMustStayRejected()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "double NULL DEFAULT", out _
            ));
        }

        //Expected RED on the current strict builder. The following strings represent valid common-column definition shapes, NOT evidence that a live column has those attributes.
        [TestMethod]
        public void B1R08_DoubleNullableDefaultNullGrammar()
        {
            AssertQualified("double DEFAULT NULL", "double DEFAULT NULL");
        }

        [TestMethod]
        public void B1R09_DecimalNullableDefaultNullGrammar()
        {
            AssertQualified("decimal(10,2) DEFAULT NULL", "decimal(10,2) DEFAULT NULL");
        }

        [TestMethod]
        public void B1R10_DecimalNotNullQuotedNumericDefaultGrammar()
        {
            AssertQualified("decimal(10,2) NOT NULL DEFAULT '12.50'", "decimal(10,2) NOT NULL DEFAULT '12.50'");
        }

        [TestMethod]
        public void B1R11_IntegerNullableDefaultNullGrammar()
        {
            AssertQualified("int DEFAULT NULL", "int DEFAULT NULL");
        }

        [TestMethod]
        public void B1R12_InheritedVarcharNullableDefaultNullGrammar()
        {
            AssertQualified("varchar(32) DEFAULT NULL", "varchar(32) DEFAULT NULL");
        }

        [TestMethod]
        public void B1R13_InheritedVarcharQuotedDefaultGrammar()
        {
            AssertQualified("varchar(32) DEFAULT 'abc'", "varchar(32) DEFAULT 'abc'");
        }

        [TestMethod]
        public void B1R14_InheritedCharNullableDefaultNullGrammar()
        {
            AssertQualified("char(12) DEFAULT NULL", "char(12) DEFAULT NULL");
        }

        [TestMethod]
        public void B1R15_DoubleCommentPreservesNullableDefaultNull()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(ColumnDdlOperation.Comment), "double DEFAULT NULL", out var sql
            ));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`test_columns` MODIFY COLUMN `old_column` double DEFAULT NULL COMMENT 'updated';",
                sql
            );
        }

        [TestMethod]
        public void B1R16_DecimalCommentPreservesPrecisionScaleAndDefault()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(ColumnDdlOperation.Comment),
                "decimal(10,2) NOT NULL DEFAULT '12.50' COMMENT 'before'", out var sql
            ));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`test_columns` MODIFY COLUMN `old_column` decimal(10,2) NOT NULL DEFAULT '12.50' COMMENT 'updated';",
                sql
            );
        }

        [TestMethod]
        public void B1R17_InheritedVarcharCommentPreservesColumnDefinition()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(ColumnDdlOperation.Comment),
                "varchar(32) DEFAULT NULL COMMENT 'before'", out var sql
            ));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`test_columns` MODIFY COLUMN `old_column` varchar(32) DEFAULT NULL COMMENT 'updated';",
                sql
            );
        }

        [TestMethod]
        public void B1R18_LegacyDecimalRenameRetainsOriginalDefinitionAndComment()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename
            (
                Request(ColumnDdlOperation.Rename),
                "decimal(10,2) NOT NULL DEFAULT '12.50' COMMENT 'before'", out var sql
            ));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`test_columns` CHANGE COLUMN `old_column` `new_column` decimal(10,2) NOT NULL DEFAULT '12.50' COMMENT 'before';",
                sql
            );
        }

        [TestMethod]
        public void B1R19_LegacyVarcharRenameRetainsInheritedDefinitionAndComment()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename
            (
                Request(ColumnDdlOperation.Rename),
                "varchar(32) DEFAULT 'abc' COMMENT 'before'", out var sql
            ));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`test_columns` CHANGE COLUMN `old_column` `new_column` varchar(32) DEFAULT 'abc' COMMENT 'before';",
                sql
            );
        }
    }
}
