using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DmlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JasonQuery.Tests.Core.Database.DmlPreview
{
    [TestClass]
    public sealed class DmlPreviewSqlLiteralFormatterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithNullRequest_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlLiteralFormatter.Format(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithNullColumnInfo_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlLiteralFormatter.Format
                                          (
                                              new DmlPreviewLiteralFormatRequest
                                              {
                                                   DataSourceType = DataSourceType.Oracle,
                                                   CellValue = "abc"
                                              }
                                          )
                           );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithNullKeyword_ReturnsUpperCaseNull()
        {
            Assert.AreEqual("NULL", Format(DataSourceType.PostgreSql, "null", CategoryDataTypeKind.String));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithCustomNullIndicator_ReturnsNull()
        {
            Assert.AreEqual("NULL", Format(DataSourceType.PostgreSql, "(null)", CategoryDataTypeKind.String, nullIndicators: new[] { "(null)" }));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithLowerCaseKeywords_ReturnsLowerCaseNull()
        {
            Assert.AreEqual("null", Format(DataSourceType.PostgreSql, "NULL", CategoryDataTypeKind.String, useUpperCaseKeywords: false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithDefaultKeywordAndDefaultValue_ReturnsDefault()
        {
            Assert.AreEqual("DEFAULT", Format(DataSourceType.PostgreSql, "default", CategoryDataTypeKind.Number, defaultValue: "nextval('seq')"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithExactDefaultValue_ReturnsDefault()
        {
            Assert.AreEqual("DEFAULT", Format(DataSourceType.PostgreSql, "nextval('seq')", CategoryDataTypeKind.Number, defaultValue: "nextval('seq')"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithUnquotedDefaultValue_ReturnsDefault()
        {
            Assert.AreEqual("DEFAULT", Format(DataSourceType.PostgreSql, "abc", CategoryDataTypeKind.String, defaultValue: "'abc'"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_WithDefaultButDefaultNotAllowed_DoesNotReturnDefault()
        {
            Assert.AreEqual("'default'", Format(DataSourceType.PostgreSql, "default", CategoryDataTypeKind.String, defaultValue: "'abc'", allowDefaultKeyword: false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_EmptyStringColumn_ReturnsEmptySqlString()
        {
            Assert.AreEqual("''", Format(DataSourceType.Oracle, string.Empty, CategoryDataTypeKind.String));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_EmptyNullableNumber_ReturnsNull()
        {
            Assert.AreEqual("NULL", Format(DataSourceType.Oracle, string.Empty, CategoryDataTypeKind.Number, isNullable: true));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_EmptyRequiredNumber_ReturnsZero()
        {
            Assert.AreEqual("0", Format(DataSourceType.Oracle, string.Empty, CategoryDataTypeKind.Number, isNullable: false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_EmptyNullableDateTime_ReturnsNull()
        {
            Assert.AreEqual("NULL", Format(DataSourceType.Oracle, string.Empty, CategoryDataTypeKind.DateTime, isNullable: true));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_EmptyRequiredDateTime_ReturnsEmptySqlString()
        {
            Assert.AreEqual("''", Format(DataSourceType.Oracle, string.Empty, CategoryDataTypeKind.DateTime, isNullable: false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Format_EmptyValueWithDefault_ReturnsDefault()
        {
            Assert.AreEqual("DEFAULT", Format(DataSourceType.PostgreSql, string.Empty, CategoryDataTypeKind.Number, defaultValue: "nextval('seq')"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Format_OracleString_EscapesSingleQuote()
        {
            Assert.AreEqual("'12''31'", Format(DataSourceType.Oracle, "12'31", CategoryDataTypeKind.String));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Format_OracleDateTime_ReturnsTimestampLiteral()
        {
            Assert.AreEqual("TIMESTAMP '2026-07-1907:43:15.123456'", Format(DataSourceType.Oracle, "2026/07/19 07:43:15.123456", CategoryDataTypeKind.DateTime));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Format_OracleInvalidDate_PreservesOriginalParts()
        {
            Assert.AreEqual("TIMESTAMP 'not-a-date12:34'", Format(DataSourceType.Oracle, "not-a-date 12:34", CategoryDataTypeKind.DateTime));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Format_OracleNumber_ReturnsUnquotedValue()
        {
            Assert.AreEqual("15", Format(DataSourceType.Oracle, "15", CategoryDataTypeKind.Number));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void Format_PostgreSqlString_EscapesSingleQuote()
        {
            Assert.AreEqual("'日本''Japan'", Format(DataSourceType.PostgreSql, "日本'Japan", CategoryDataTypeKind.String));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void Format_PostgreSqlDateTime_PreservesDisplayText()
        {
            Assert.AreEqual("'2026/07/19'", Format(DataSourceType.PostgreSql, "2026/07/19", CategoryDataTypeKind.DateTime));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void Format_PostgreSqlNumber_ReturnsUnquotedValue()
        {
            Assert.AreEqual("1", Format(DataSourceType.PostgreSql, "1", CategoryDataTypeKind.Number));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerNString_ReturnsNPrefixAndEscapesSingleQuote()
        {
            Assert.AreEqual("N'Taipei''1015'", Format(DataSourceType.SqlServer, "Taipei'1015", CategoryDataTypeKind.String, specialDataTypeKind: SpecialDataTypeKind.NString));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerBitTrue_ReturnsOne()
        {
            Assert.AreEqual("1", Format(DataSourceType.SqlServer, "TRUE", CategoryDataTypeKind.Number, baseDataType: "BIT"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerBitFalse_ReturnsZero()
        {
            Assert.AreEqual("0", Format(DataSourceType.SqlServer, "false", CategoryDataTypeKind.Number, baseDataType: "BIT"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerUniqueIdentifier_ReturnsQuotedValue()
        {
            Assert.AreEqual("'5E5E493D-77BB-4DD1-A8F2-83B83D0D2A6D'", Format(DataSourceType.SqlServer, "5E5E493D-77BB-4DD1-A8F2-83B83D0D2A6D", CategoryDataTypeKind.String, baseDataType: "UNIQUEIDENTIFIER"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerDate_ReturnsIsoDate()
        {
            Assert.AreEqual("'2026-07-19'", Format(DataSourceType.SqlServer, "2026/07/19", CategoryDataTypeKind.DateTime, baseDataType: "DATE"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerTime_PreservesText()
        {
            Assert.AreEqual("'07:43:15.1234567'", Format(DataSourceType.SqlServer, "07:43:15.1234567", CategoryDataTypeKind.DateTime, baseDataType: "TIME"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerDateTime_UsesMillisecondsAndTrimsTrailingZero()
        {
            Assert.AreEqual("'2026-07-19 07:43:15.12'", Format(DataSourceType.SqlServer, "2026/07/19 07:43:15.120", CategoryDataTypeKind.DateTime, baseDataType: "DATETIME2"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Format_SqlServerInvalidDateTime_PreservesText()
        {
            Assert.AreEqual("'invalid-date'", Format(DataSourceType.SqlServer, "invalid-date", CategoryDataTypeKind.DateTime, baseDataType: "DATETIME"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Format_MySqlString_EscapesSingleQuote()
        {
            Assert.AreEqual("'Adoni''r'", Format(DataSourceType.MySql, "Adoni'r", CategoryDataTypeKind.String));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Format_MySqlBitTrue_ReturnsOne()
        {
            Assert.AreEqual("1", Format(DataSourceType.MySql, "true", CategoryDataTypeKind.Number, baseDataType: "BIT"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Format_MySqlBitFalse_ReturnsZero()
        {
            Assert.AreEqual("0", Format(DataSourceType.MySql, "FALSE", CategoryDataTypeKind.Number, baseDataType: "BIT"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Format_MySqlDate_ReturnsIsoDate()
        {
            Assert.AreEqual("'2026-07-19'", Format(DataSourceType.MySql, "2026/07/19", CategoryDataTypeKind.DateTime, baseDataType: "DATE"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Format_MySqlTime_PreservesText()
        {
            Assert.AreEqual("'07:43:15.123456'", Format(DataSourceType.MySql, "07:43:15.123456", CategoryDataTypeKind.DateTime, baseDataType: "TIME"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Format_MySqlDateTime_UsesMicrosecondsAndTrimsTrailingZero()
        {
            Assert.AreEqual("'2026-07-19 07:43:15.1234'", Format(DataSourceType.MySql, "2026/07/19 07:43:15.123400", CategoryDataTypeKind.DateTime, baseDataType: "DATETIME"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EscapeSqlString_WithNull_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlLiteralFormatter.EscapeSqlString(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void EscapeSqlString_WithMultipleQuotes_DoublesEveryQuote()
        {
            Assert.AreEqual("a''b''c", DmlPreviewSqlLiteralFormatter.EscapeSqlString("a'b'c"));
        }

        private static string Format(DataSourceType dataSourceType, string cellValue, CategoryDataTypeKind categoryDataTypeKind, bool isNullable = true, string baseDataType = "", SpecialDataTypeKind specialDataTypeKind = SpecialDataTypeKind.String,
                                     string defaultValue = "", bool allowDefaultKeyword = true, bool useUpperCaseKeywords = true, IEnumerable<string> nullIndicators = null)
        {
            return DmlPreviewSqlLiteralFormatter.Format
            (
                new DmlPreviewLiteralFormatRequest
                {
                    DataSourceType = dataSourceType,
                    CellValue = cellValue,
                    ColumnInfo = new ColumnInfo
                    {
                        CategoryDataTypeKind = categoryDataTypeKind,
                        SpecialDataTypeKind = specialDataTypeKind,
                        BaseDataType = baseDataType,
                        IsNullable = isNullable
                    },
                    DefaultValue = defaultValue,
                    AllowDefaultKeyword = allowDefaultKeyword,
                    UseUpperCaseKeywords = useUpperCaseKeywords,
                    NullValueIndicators = nullIndicators
                }
            );
        }
    }
}