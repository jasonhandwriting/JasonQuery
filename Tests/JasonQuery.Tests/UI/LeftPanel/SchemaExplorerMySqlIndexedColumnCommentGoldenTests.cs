using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-S1-C3-B4-C1: one-column PRIMARY KEY metadata evidence, Golden-first.
    //No database connections, no DDL dispatch, no Git mutations.
    //The pure SHOW CREATE proof is NOT a Reader receipt or Execute authorization.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlIndexedColumnCommentGoldenTests
    {
        private const string ProofTypeName = "JasonQuery.Core.Database.DdlPreview.MySqlPrimaryKeyColumnProofPolicy";
        private const string SimpleColumnDefinition = "varchar(12) NOT NULL DEFAULT 'aabbcc' COMMENT 'vvv'";

        private const string RealisticCreate =
            "CREATE TABLE `abc` (\r\n"
            + "  `varchar_12` varchar(12) NOT NULL DEFAULT 'aabbcc' COMMENT 'vvv',\r\n"
            + "  `int0` int DEFAULT '19',\r\n"
            + "  PRIMARY KEY (`varchar_12`)\r\n"
            + ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci";

        private static ColumnDdlRequest CommentRequest()
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = "sakila",
                TableName = "abc",
                ColumnName = "varchar_12",
                Comment = "updated"
            };
        }

        private static string ValidateExisting(string columnKey, string oldComment = "vvv", string extra = "")
        {
            return MySqlCommonColumnDefinitionProofPolicy.Validate
            (
                SimpleColumnDefinition, "varchar(12)", "NO", "aabbcc", oldComment,
                "utf8mb4", "utf8mb4_0900_ai_ci", "utf8mb4", "utf8mb4_0900_ai_ci",
                extra, string.Empty, columnKey, true
            );
        }

        //Future pure contract:
        //internal static bool TryQualifySingleColumnPrimaryKey(
        //    string showCreateTable, string columnName, string columnKey, string isNullable,
        //    out string verifiedKeyClause)
        //The caller must obtain SHOW CREATE and metadata from its verified Reader connection.
        private static bool TryPrimaryKeyProof(string createSql, string columnName, string columnKey, string isNullable, out string verifiedClause)
        {
            verifiedClause = string.Empty;

            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType(ProofTypeName, false);

            Assert.IsNotNull(type, "B4-C1 EXPECTED RED: a pure primary-key corroboration policy is not yet implemented.");

            var method = type.GetMethod
            (
                "TryQualifySingleColumnPrimaryKey",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string).MakeByRefType() },
                null
            );

            Assert.IsNotNull(method, "B4-C1 EXPECTED RED: exact single-column PRIMARY KEY proof contract missing.");
            Assert.AreEqual(typeof(bool), method.ReturnType);

            var arguments = new object[] { createSql, columnName, columnKey, isNullable, null };
            var qualified = (bool)method.Invoke(null, arguments);

            verifiedClause = arguments[4] as string ?? string.Empty;
            return qualified;
        }

        //Future overload: the original 13-argument Validate remains fail-closed; new 14-argument Validate is allowed to consider a Reader-verified simple primary key.
        private static string ValidateVerifiedPrimaryKey(bool verifiedPrimaryKey, string oldComment = "vvv", string extra = "")
        {
            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.MySqlCommonColumnDefinitionProofPolicy", true
            );

            var parameterTypes = new Type[14];

            for (var i = 0; i < 12; i++)
            {
                parameterTypes[i] = typeof(string);
            }

            parameterTypes[12] = typeof(bool);
            parameterTypes[13] = typeof(bool);

            var method = type.GetMethod
            (
                "Validate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, parameterTypes, null
            );

            Assert.IsNotNull(method, "B4-C1 EXPECTED RED: verified-index overload is not yet implemented.");

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    SimpleColumnDefinition, "varchar(12)", "NO", "aabbcc", oldComment,
                    "utf8mb4", "utf8mb4_0900_ai_ci", "utf8mb4", "utf8mb4_0900_ai_ci",
                    extra, string.Empty, "PRI", true, verifiedPrimaryKey
                }
            );
        }

        [TestMethod]
        public void B4C1G01_UnindexedCommonVarcharProofRemainsQualified()
        {
            Assert.AreEqual(string.Empty, ValidateExisting(string.Empty));
        }

        [TestMethod]
        public void B4C1G02_UnverifiedPriRemainsDeniedByExistingProof()
        {
            Assert.AreEqual("F7U3C4B3_METADATA_OUTSIDE_SAFE_SUBSET", ValidateExisting("PRI"));
        }

        [TestMethod]
        public void B4C1G03_CommentBuilderDoesNotNeedToRestatePrimaryKey()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                CommentRequest(), SimpleColumnDefinition, out var sql
            ));

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`abc` MODIFY COLUMN `varchar_12` varchar(12) NOT NULL DEFAULT 'aabbcc' COMMENT 'updated';",
                sql
            );
        }

        [TestMethod]
        public void B4C1G04_ExistingProofStillRejectsUnsafeAttributes()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ValidateExisting(string.Empty, "different")));
            Assert.IsFalse(string.IsNullOrEmpty(ValidateExisting(string.Empty, "vvv", "auto_increment")));
        }

        [TestMethod]
        public void B4C1G05_EffectiveAttributeSignatureRecordsKeyStatus()
        {
            var plain = MySqlCommonColumnDefinitionProofPolicy.BuildVerifiedAttributesSignature
            (
                "varchar(12)", "NO", "aabbcc", string.Empty
            );

            var indexed = MySqlCommonColumnDefinitionProofPolicy.BuildVerifiedAttributesSignature
            (
                "varchar(12)", "NO", "aabbcc", "PRI"
            );

            Assert.AreNotEqual(plain, indexed);
        }

        [TestMethod]
        public void B4C1R06_RealSingleColumnPrimaryKeyMustBeCorroborated()
        {
            Assert.IsTrue(TryPrimaryKeyProof(RealisticCreate, "varchar_12", "PRI", "NO", out var clause));
            Assert.AreEqual("PRIMARY KEY (`varchar_12`)", clause);
        }

        [TestMethod]
        public void B4C1R07_NotNullUniqueShownAsPriMustNotMasqueradeAsPrimaryKey()
        {
            var sql = RealisticCreate.Replace("PRIMARY KEY (`varchar_12`)", "UNIQUE KEY `uq_v12` (`varchar_12`)");

            Assert.IsFalse(TryPrimaryKeyProof(sql, "varchar_12", "PRI", "NO", out var clause));
            Assert.AreEqual(string.Empty, clause);
        }

        [TestMethod]
        public void B4C1R08_CompositePrimaryKeyStaysOutsideInitialSlice()
        {
            var sql = RealisticCreate.Replace("PRIMARY KEY (`varchar_12`)", "PRIMARY KEY (`varchar_12`,`int0`)");

            Assert.IsFalse(TryPrimaryKeyProof(sql, "varchar_12", "PRI", "NO", out var clause));
            Assert.AreEqual(string.Empty, clause);
        }

        [TestMethod]
        public void B4C1R09_DifferentPrimaryKeyColumnMustBeRejected()
        {
            var sql = RealisticCreate.Replace("PRIMARY KEY (`varchar_12`)", "PRIMARY KEY (`int0`)");

            Assert.IsFalse(TryPrimaryKeyProof(sql, "varchar_12", "PRI", "NO", out var clause));
            Assert.AreEqual(string.Empty, clause);
        }

        [TestMethod]
        public void B4C1R10_NullableOrUnverifiedColumnKeyMustBeRejected()
        {
            Assert.IsFalse(TryPrimaryKeyProof(RealisticCreate, "varchar_12", "PRI", "YES", out _));
            Assert.IsFalse(TryPrimaryKeyProof(RealisticCreate, "varchar_12", string.Empty, "NO", out _));
        }

        [TestMethod]
        public void B4C1R11_UniAndMulRequireSeparateFutureQualification()
        {
            Assert.IsFalse(TryPrimaryKeyProof(RealisticCreate, "varchar_12", "UNI", "NO", out _));
            Assert.IsFalse(TryPrimaryKeyProof(RealisticCreate, "varchar_12", "MUL", "NO", out _));
        }

        [TestMethod]
        public void B4C1R12_DuplicatePrimaryClausesMustFailClosed()
        {
            var sql = RealisticCreate.Replace
            (
                "  PRIMARY KEY (`varchar_12`)",
                "  PRIMARY KEY (`varchar_12`),\r\n  PRIMARY KEY (`varchar_12`)"
            );

            Assert.IsFalse(TryPrimaryKeyProof(sql, "varchar_12", "PRI", "NO", out _));
        }

        [TestMethod]
        public void B4C1R13_OnlyVerifiedPrimaryKeyMayEnterNewMetadataProofOverload()
        {
            Assert.AreEqual(string.Empty, ValidateVerifiedPrimaryKey(true));
            Assert.IsFalse(string.IsNullOrEmpty(ValidateVerifiedPrimaryKey(false)));
            Assert.IsFalse(string.IsNullOrEmpty(ValidateExisting("PRI")));
        }

        [TestMethod]
        public void B4C1R14_EvenVerifiedPrimaryKeyCannotHideAttributeOrCommentMismatch()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ValidateVerifiedPrimaryKey(true, "changed")));
            Assert.IsFalse(string.IsNullOrEmpty(ValidateVerifiedPrimaryKey(true, "vvv", "auto_increment")));
        }

        [TestMethod]
        public void B4C1R15_CommentTextCannotMasqueradeAsPrimaryKeyDeclaration()
        {
            var sql = RealisticCreate.Replace
            (
                "PRIMARY KEY (`varchar_12`)",
                "UNIQUE KEY `uq_v12` (`varchar_12`)"
            ).Replace("COMMENT 'vvv'", "COMMENT 'PRIMARY KEY (`varchar_12`)'");

            Assert.IsFalse(TryPrimaryKeyProof(sql, "varchar_12", "PRI", "NO", out _));
        }
    }
}
