using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.QueryEditor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.SqlBuilder.Oracle
{
    [TestClass]
    public sealed class OracleQueryEditorSqlBuilderOwnerTests
    {
        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G01_TableAndView_UsesExactOwnerWithoutCaseFold()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql("MiXeD");

            Assert.Contains("WHERE NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('MiXeD', 'NLS_SORT=BINARY')", sql);
            Assert.DoesNotContain("UPPER(Owner)", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G02_TableAndView_PreservesOwnerProjectionCase()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql("MiXeD");

            Assert.Contains("Owner AS Schema", sql);
            Assert.DoesNotContain("UPPER(Owner) AS Schema", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G03_TableInfo_UsesExactOwner()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildTableInfo("MIXED");

            Assert.Contains("AND NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('MIXED', 'NLS_SORT=BINARY')", sql);
            Assert.DoesNotContain("UPPER(Owner)", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G04_ViewInfo_UsesExactOwner()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildViewInfo("MiXeD");

            Assert.Contains("AND NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('MiXeD', 'NLS_SORT=BINARY')", sql);
            Assert.DoesNotContain("UPPER(Owner)", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G05_QuotedOwnerContent_EscapesApostropheAsSqlLiteral()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql("Mi'XeD");

            Assert.Contains("WHERE NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('Mi''XeD', 'NLS_SORT=BINARY')", sql);
            Assert.DoesNotContain("NLSSORT('Mi'XeD', 'NLS_SORT=BINARY')", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G06_EmptyOwner_FailsClosed()
        {
            Assert.AreEqual(string.Empty, OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql(string.Empty));
            Assert.AreEqual(string.Empty, OracleQueryEditorSqlBuilder.BuildTableInfo(null));
            Assert.AreEqual(string.Empty, OracleQueryEditorSqlBuilder.BuildViewInfo(" "));
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G07_ExistingTableViewUnionShape_RemainsIntact()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql("SYS");

            Assert.Contains("FROM All_Tables", sql);
            Assert.Contains("FROM All_Views", sql);
            Assert.Contains("UNION ALL", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02A")]
        public void F02A_G08_ExistingOrderBy_RemainsIntact()
        {
            var sql = OracleQueryEditorSqlBuilder.BuildTableAndViewNameSql("SYS");

            Assert.Contains("ORDER BY SchemaType, SchemaName", sql);
        }
    }
}
