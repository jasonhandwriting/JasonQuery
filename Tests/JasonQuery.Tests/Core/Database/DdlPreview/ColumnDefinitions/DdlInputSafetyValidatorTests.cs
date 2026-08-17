using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview.ColumnDefinitions
{
    [TestClass]
    public sealed class DdlInputSafetyValidatorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "COLUMN1")]
        [DataRow(DataSourceType.Oracle, "_COLUMN1")]
        [DataRow(DataSourceType.Oracle, "\"Column Name\"")]
        [DataRow(DataSourceType.Oracle, "\"Column.Name\"")]
        [DataRow(DataSourceType.PostgreSql, "column_1")]
        [DataRow(DataSourceType.PostgreSql, "\"Column Name\"")]
        [DataRow(DataSourceType.PostgreSql, "\"Column.Name\"")]
        [DataRow(DataSourceType.PostgreSql, "欄位名稱")]
        [DataRow(DataSourceType.SqlServer, "Column1")]
        [DataRow(DataSourceType.SqlServer, "[Column Name]")]
        [DataRow(DataSourceType.SqlServer, "[Column.Name]")]
        [DataRow(DataSourceType.SqlServer, "[Column]]Name]")]
        [DataRow(DataSourceType.MySql, "column1")]
        [DataRow(DataSourceType.MySql, "`Column Name`")]
        [DataRow(DataSourceType.MySql, "`Column.Name`")]
        [DataRow(DataSourceType.MySql, "`Column``Name`")]
        public void IsSafeSingleIdentifier_WithSafeIdentifier_ReturnsTrue(DataSourceType dataSourceType, string identifier)
        {
            Assert.IsTrue(DdlInputSafetyValidator.IsSafeSingleIdentifier(identifier, dataSourceType));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, null)]
        [DataRow(DataSourceType.Oracle, "")]
        [DataRow(DataSourceType.Oracle, "schema.column")]
        [DataRow(DataSourceType.Oracle, "column;drop")]
        [DataRow(DataSourceType.PostgreSql, "column -- comment")]
        [DataRow(DataSourceType.PostgreSql, "\"unclosed")]
        [DataRow(DataSourceType.SqlServer, "[unclosed")]
        [DataRow(DataSourceType.SqlServer, "a.b")]
        [DataRow(DataSourceType.MySql, "`unclosed")]
        [DataRow(DataSourceType.MySql, "a/*x*/")]
        [DataRow(DataSourceType.None, "column1")]
        public void IsSafeSingleIdentifier_WithUnsafeIdentifier_ReturnsFalse(DataSourceType dataSourceType, string identifier)
        {
            Assert.IsFalse(DdlInputSafetyValidator.IsSafeSingleIdentifier(identifier, dataSourceType));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("public.my_domain")]
        [DataRow("VECTOR(3)")]
        [DataRow("\"MySchema\".\"MyType\"")]
        [DataRow("[dbo].[MyType]")]
        [DataRow("timestamp with time zone")]
        [DataRow("numeric(18,2)")]
        public void IsSafeCustomType_WithSafeType_ReturnsTrue(string typeText)
        {
            Assert.IsTrue(DdlInputSafetyValidator.IsSafeCustomType(typeText));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("varchar(10); drop table a")]
        [DataRow("varchar(10) -- comment")]
        [DataRow("varchar(10) /* comment */")]
        [DataRow("varchar(10) NOT NULL")]
        [DataRow("varchar(10) DEFAULT 'x'")]
        [DataRow("varchar(10) PRIMARY KEY")]
        [DataRow("varchar(10")]
        [DataRow("varchar(10)\r\nDROP TABLE a")]
        public void IsSafeCustomType_WithUnsafeType_ReturnsFalse(string typeText)
        {
            Assert.IsFalse(DdlInputSafetyValidator.IsSafeCustomType(typeText));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("CURRENT_TIMESTAMP")]
        [DataRow("SYS_GUID()")]
        [DataRow("NEWID()")]
        [DataRow("nextval('public.seq')")]
        [DataRow("concat('A', 'B')")]
        public void IsSafeSqlExpression_WithSingleExpression_ReturnsTrue(string expression)
        {
            Assert.IsTrue(DdlInputSafetyValidator.IsSafeSqlExpression(expression));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("CURRENT_TIMESTAMP; DROP TABLE a")]
        [DataRow("CURRENT_TIMESTAMP -- comment")]
        [DataRow("func(")]
        [DataRow("'unclosed")]
        [DataRow("x\r\nDROP TABLE a")]
        public void IsSafeSqlExpression_WithUnsafeExpression_ReturnsFalse(string expression)
        {
            Assert.IsFalse(DdlInputSafetyValidator.IsSafeSqlExpression(expression));
        }
    }
}