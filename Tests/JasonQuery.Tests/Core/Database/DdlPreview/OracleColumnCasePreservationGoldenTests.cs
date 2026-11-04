using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    [TestCategory("F7L2B2C_CASE")]
    public sealed class OracleColumnCasePreservationGoldenTests
    {
        [TestMethod]
        public void G01_OracleAddLowercaseNamePreservesInput()
        {
            var result = ColumnAddSqlBuilder.Build(CreateAdd(DataSourceType.Oracle, "new"));

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" ADD \"new\" VARCHAR2(50) NULL;", result.Sql);
        }

        [TestMethod]
        public void G02_OracleAddMixedCaseNamePreservesInput()
        {
            var result = ColumnAddSqlBuilder.Build(CreateAdd(DataSourceType.Oracle, "JQ98_Added2"));

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" ADD \"JQ98_Added2\" VARCHAR2(50) NULL;", result.Sql);
        }

        [TestMethod]
        public void G03_OracleRenameMixedCaseNamePreservesInput()
        {
            var request = CreateRename(DataSourceType.Oracle, "JQ98_Added", "JQ98_Added2");

            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" RENAME COLUMN \"JQ98_Added\" TO \"JQ98_Added2\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G04_OracleRenameLowercaseNamePreservesInput()
        {
            var request = CreateRename(DataSourceType.Oracle, "NOTE", "new");

            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" RENAME COLUMN \"NOTE\" TO \"new\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G05_OracleRenameUppercaseToLowercaseIsARealChange()
        {
            var request = CreateRename(DataSourceType.Oracle, "NOTE", "note");

            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" RENAME COLUMN \"NOTE\" TO \"note\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G06_OracleRenameCaseOnlyMixedCaseIsARealChange()
        {
            var request = CreateRename(DataSourceType.Oracle, "JQ98_ADDED2", "JQ98_Added2");

            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" RENAME COLUMN \"JQ98_ADDED2\" TO \"JQ98_Added2\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G07_OracleAddQuotedNameStillPreservesExactCase()
        {
            var result = ColumnAddSqlBuilder.Build(CreateAdd(DataSourceType.Oracle, "\"MiXeD\""));

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" ADD \"MiXeD\" VARCHAR2(50) NULL;", result.Sql);
        }

        [TestMethod]
        public void G08_OracleRenameQuotedNameStillPreservesExactCase()
        {
            var request = CreateRename(DataSourceType.Oracle, "NOTE", "\"MiXeD\"");

            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\" RENAME COLUMN \"NOTE\" TO \"MiXeD\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G09_OracleRenameToIdenticalNameIsNoOp()
        {
            var request = CreateRename(DataSourceType.Oracle, "NOTE", "NOTE");

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G10_UnsafeOracleAddNameIsRejected()
        {
            var result = ColumnAddSqlBuilder.Build(CreateAdd(DataSourceType.Oracle, "new;DROP"));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.UnsafeColumnName, result.FailureKind);
        }

        [TestMethod]
        public void G11_UnsafeOracleRenameNameIsRejected()
        {
            var request = CreateRename(DataSourceType.Oracle, "NOTE", "new;DROP");

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G12_PostgreSqlAddKeepsExistingNormalization()
        {
            var result = ColumnAddSqlBuilder.Build(CreateAdd(DataSourceType.PostgreSql, "New_Column"));

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual("ALTER TABLE \"public\".\"a_test\" ADD COLUMN \"new_column\" varchar(50) NULL;", result.Sql);
        }

        [TestMethod]
        public void G13_PostgreSqlRenameKeepsExistingNormalization()
        {
            var request = CreateRename(DataSourceType.PostgreSql, "c1", "New_Column");

            Assert.AreEqual("ALTER TABLE \"public\".\"a_test\" RENAME COLUMN \"c1\" TO \"new_column\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void G14_GlobalOracleDefaultQuotingStillNormalizes()
        {
            //Keep global Oracle SQL identifier defaults unchanged; this is a GUI builder-specific decision.
            Assert.AreEqual("\"NEW\"", DdlIdentifierQuotingPolicy.QuoteNewIdentifier(DataSourceType.Oracle, "new"));
        }

        [TestMethod]
        public void G15_OracleTableRenameAlreadyPreservesInput()
        {
            var request = new TableDdlRequest
            {
                DataSourceType = DataSourceType.Oracle,
                Operation = TableDdlOperation.Rename,
                SchemaName = "JQ98XX",
                TableName = "A_TEST",
                NewTableName = "MiXeD"
            };

            Assert.AreEqual("ALTER TABLE \"JQ98XX\".\"A_TEST\" RENAME TO \"MiXeD\";", TableDdlSqlBuilder.Build(request));
        }

        private static ColumnDdlRequest CreateRename(DataSourceType dataSourceType, string oldName, string newName)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = dataSourceType,
                Operation = ColumnDdlOperation.Rename,
                SchemaName = dataSourceType == DataSourceType.Oracle ? "JQ98XX" : "public",
                TableName = dataSourceType == DataSourceType.Oracle ? "JQ98_B2_C_ADD" : "a_test",
                ColumnName = oldName,
                NewColumnName = newName,
                ColumnType = "varchar(20)"
            };
        }

        private static ColumnAddRequest CreateAdd(DataSourceType dataSourceType, string newName)
        {
            return new ColumnAddRequest
            {
                DataSourceType = dataSourceType,
                SchemaName = dataSourceType == DataSourceType.Oracle ? "JQ98XX" : "public",
                TableName = dataSourceType == DataSourceType.Oracle ? "JQ98_B2_C_ADD" : "a_test",
                Column = new ColumnDefinition
                {
                    ColumnName = newName,
                    TypeKey = dataSourceType == DataSourceType.Oracle ? "VARCHAR2" : "VARCHAR",
                    Parameter1 = "50",
                    Parameter2 = string.Empty,
                    NullAllowed = true,
                    DefaultValueKind = ColumnDefaultValueKind.None
                }
            };
        }
    }
}
