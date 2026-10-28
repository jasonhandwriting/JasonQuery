using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Metadata
{
    [TestClass]
    public sealed class MetadataCommentQueryBuilderTests
    {
        private static MetadataCommentQueryRequest Request(DataSourceType provider, MetadataCommentObjectKind kind)
        {
            return new MetadataCommentQueryRequest
            {
                DataSourceType = provider,
                ObjectKind = kind,
                SchemaDatabase = "store",
                SchemaName = "store",
                TableName = "Orders",
                ColumnName = "Detail"
            };
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        [DataRow(DataSourceType.Oracle, "Table", "ALL_TAB_COMMENTS", "x.OWNER")]
        [DataRow(DataSourceType.Oracle, "Column", "ALL_COL_COMMENTS", "x.COLUMN_NAME")]
        [DataRow(DataSourceType.PostgreSql, "Table", "pg_catalog.pg_class", "n.nspname")]
        [DataRow(DataSourceType.PostgreSql, "Column", "pg_catalog.pg_attribute", "a.attname")]
        [DataRow(DataSourceType.SqlServer, "Table", "[store].sys.tables", "s.name")]
        [DataRow(DataSourceType.SqlServer, "Column", "[store].sys.columns", "c.name")]
        [DataRow(DataSourceType.MySql, "Table", "INFORMATION_SCHEMA.TABLES", "t.TABLE_SCHEMA")]
        [DataRow(DataSourceType.MySql, "Column", "INFORMATION_SCHEMA.COLUMNS", "c.COLUMN_NAME")]
        public void G01_G02_AllEightShapes_HaveQualifiedBaseObject(DataSourceType provider, string kindName, string source, string identityPredicate)
        {
            var kind = kindName == "Column" ? MetadataCommentObjectKind.Column : MetadataCommentObjectKind.Table;
            var sql = MetadataCommentQueryBuilder.Build(Request(provider, kind));

            Assert.Contains(source, sql);
            Assert.Contains(identityPredicate, sql);
            Assert.Contains(" AS MetadataComment", sql);
            Assert.Contains(" AS HasExistingComment", sql);
            Assert.Contains("Orders", sql);
            Assert.Contains(provider == DataSourceType.Oracle ? "STORE" : "store", sql);

            if (kind == MetadataCommentObjectKind.Column)
            {
                Assert.Contains("Detail", sql);
            }
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G03_OracleColumn_UsesRequestedOwner_NotUserViews()
        {
            var request = Request(DataSourceType.Oracle, MetadataCommentObjectKind.Column);

            request.SchemaName = "PRIVATE_OWNER";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("FROM ALL_COL_COMMENTS x", sql);
            Assert.Contains("INNER JOIN ALL_TABLES t", sql);
            Assert.Contains("x.OWNER = 'PRIVATE_OWNER'", sql);
            Assert.DoesNotContain("USER_TAB_COLUMNS", sql);
            Assert.DoesNotContain("USER_COL_COMMENTS", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G04_OracleQuotedOwner_PreservesExactCaseAndEmbeddedQuote()
        {
            var request = Request(DataSourceType.Oracle, MetadataCommentObjectKind.Table);

            request.SchemaName = "\"Mixed'Owner\"";
            request.TableName = "\"Sales Table\"";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("x.OWNER = 'Mixed''Owner'", sql);
            Assert.Contains("x.TABLE_NAME = 'Sales Table'", sql);
            Assert.DoesNotContain("UPPER(", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G05_G06_SqlServerColumn_LeftJoinsCommentOnExistingColumn()
        {
            var sql = MetadataCommentQueryBuilder.Build(Request(DataSourceType.SqlServer, MetadataCommentObjectKind.Column));

            Assert.Contains("FROM [store].sys.tables t", sql);
            Assert.Contains("INNER JOIN [store].sys.columns c ON c.object_id = t.object_id", sql);
            Assert.Contains("LEFT JOIN [store].sys.extended_properties ep", sql);
            Assert.Contains("ep.minor_id = c.column_id", sql);
            Assert.Contains("CASE WHEN ep.major_id IS NULL THEN 0 ELSE 1 END AS HasExistingComment", sql);
            Assert.DoesNotContain("FROM [store].sys.extended_properties", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G08_SqlServerDatabaseIdentifier_QuotesSpaceAndClosingBracket()
        {
            var request = Request(DataSourceType.SqlServer, MetadataCommentObjectKind.Column);

            request.SchemaDatabase = "Finance] DB";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("[Finance]] DB].sys.tables", sql);
            Assert.Contains("[Finance]] DB].sys.columns", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G09_SqlServerSchemaPrefixIsNotSilentlyStrippedFromExistingTableName()
        {
            var request = Request(DataSourceType.SqlServer, MetadataCommentObjectKind.Column);

            request.SchemaName = "dbo";
            request.TableName = "dbo.Orders";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("s.name = N'dbo'", sql);
            Assert.Contains("t.name = N'dbo.Orders'", sql);
            Assert.DoesNotContain("{sSchemaName}", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        [DataRow("Table")]
        [DataRow("Column")]
        public void G10_MySqlConflictingSchemaAndDatabase_FailsClosed(string kindName)
        {
            var kind = kindName == "Column" ? MetadataCommentObjectKind.Column : MetadataCommentObjectKind.Table;
            var request = Request(DataSourceType.MySql, kind);

            request.SchemaDatabase = "db_main";
            request.SchemaName = "db_other";
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G10_MySqlColumnWithoutDatabase_CannotMatchColumnDdlTarget()
        {
            var request = Request(DataSourceType.MySql, MetadataCommentObjectKind.Column);

            request.SchemaDatabase = null;
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G15_AllFourProviders_EscapeApostrophesInLiteralValues()
        {
            foreach (var provider in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                var request = Request(provider, MetadataCommentObjectKind.Column);

                request.SchemaName = "Sc'h";
                request.SchemaDatabase = "Sc'h";
                request.TableName = "O'rders";
                request.ColumnName = "C'ol";

                var sql = MetadataCommentQueryBuilder.Build(request);

                Assert.IsFalse(string.IsNullOrEmpty(sql), provider.ToString());
                Assert.Contains(provider == DataSourceType.Oracle ? "SC''H" : "Sc''h", sql);
                Assert.Contains("O''rders", sql);
                Assert.Contains("C''ol", sql);
                Assert.DoesNotContain("O'rders", sql);
            }
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        [DataRow(DataSourceType.Oracle)]
        [DataRow(DataSourceType.PostgreSql)]
        [DataRow(DataSourceType.SqlServer)]
        [DataRow(DataSourceType.MySql)]
        public void G16_MixedCaseNames_PreservedNotCaseFolded(DataSourceType provider)
        {
            var request = Request(provider, MetadataCommentObjectKind.Column);

            //Oracle quoted owners must preserve the exact case (unquoted owners fold to uppercase).
            request.SchemaName = provider == DataSourceType.Oracle ? "\"MiXeD\"" : "MiXeD";
            request.SchemaDatabase = "MiXeD";
            request.TableName = "TableCase";
            request.ColumnName = "ColCase";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("MiXeD", sql);
            Assert.Contains("TableCase", sql);
            Assert.Contains("ColCase", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("D1B2")]
        [DataRow("Table")]
        [DataRow("Column")]
        public void OracleUnquotedOwner_IsUppercaseForMetadataIdentity(string kindName)
        {
            var kind = kindName == "Column" ? MetadataCommentObjectKind.Column : MetadataCommentObjectKind.Table;
            var request = Request(DataSourceType.Oracle, kind);

            request.SchemaName = "sys";
            request.TableName = "AABBCC";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("x.OWNER = 'SYS'", sql);
            Assert.DoesNotContain("x.OWNER = 'sys'", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("D1B2")]
        [DataRow(DataSourceType.Oracle, "Table")]
        [DataRow(DataSourceType.Oracle, "Column")]
        [DataRow(DataSourceType.PostgreSql, "Table")]
        [DataRow(DataSourceType.PostgreSql, "Column")]
        [DataRow(DataSourceType.SqlServer, "Table")]
        [DataRow(DataSourceType.SqlServer, "Column")]
        [DataRow(DataSourceType.MySql, "Table")]
        [DataRow(DataSourceType.MySql, "Column")]
        public void AllEightMetadataQueries_AreReadableMultilineSql(DataSourceType provider, string kindName)
        {
            var kind = kindName == "Column" ? MetadataCommentObjectKind.Column : MetadataCommentObjectKind.Table;
            var sql = MetadataCommentQueryBuilder.Build(Request(provider, kind));

            Assert.Contains(Environment.NewLine + "  FROM ", sql);
            Assert.Contains(Environment.NewLine + " WHERE ", sql);
            Assert.Contains(Environment.NewLine + "   AND ", sql);
            Assert.Contains(" AS MetadataComment", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G18_PostgreSqlRelkind_RemainsOrdinaryTableOnlyUntilExplicitDecision()
        {
            var sql = MetadataCommentQueryBuilder.Build(Request(DataSourceType.PostgreSql, MetadataCommentObjectKind.Table));

            Assert.Contains("c.relkind = 'r'", sql);
            Assert.DoesNotContain("c.relkind = 'p'", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G23_PostgreSqlSchemaPga_IsNotFilteredByPgUnderscoreWildcard()
        {
            var request = Request(DataSourceType.PostgreSql, MetadataCommentObjectKind.Table);

            request.SchemaName = "pga";

            var sql = MetadataCommentQueryBuilder.Build(request);

            Assert.Contains("n.nspname = 'pga'", sql);
            Assert.DoesNotContain("NOT LIKE 'pg_%'", sql);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G20_MySqlTableAndColumn_MetadataTargetEqualsDdlTarget()
        {
            var metadataTable = Request(DataSourceType.MySql, MetadataCommentObjectKind.Table);

            metadataTable.SchemaDatabase = "target_db";
            metadataTable.SchemaName = "target_db";

            var metadataColumn = Request(DataSourceType.MySql, MetadataCommentObjectKind.Column);

            metadataColumn.SchemaDatabase = "target_db";
            metadataColumn.SchemaName = "target_db";

            var tableSql = TableDdlSqlBuilder.Build(new TableDdlRequest
            {
                DataSourceType = DataSourceType.MySql, Operation = TableDdlOperation.Comment,
                SchemaDatabase = "target_db", SchemaName = "target_db", TableName = "Orders", Comment = "Hello"
            });

            var columnSql = ColumnDdlSqlBuilder.Build(new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql, Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = "target_db", SchemaName = "target_db", TableName = "Orders",
                ColumnName = "Detail", ColumnType = "varchar(80)", Comment = "Hello"
            });

            Assert.Contains("t.TABLE_SCHEMA = 'target_db'", MetadataCommentQueryBuilder.Build(metadataTable));
            Assert.Contains("t.TABLE_SCHEMA = 'target_db'", MetadataCommentQueryBuilder.Build(metadataColumn));
            Assert.Contains("`target_db`.`Orders`", tableSql);
            Assert.Contains("`target_db`.`Orders`", columnSql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("D1B1")]
        [DataRow(DataSourceType.PostgreSql)]
        [DataRow(DataSourceType.MySql)]
        public void BackslashSensitiveLiteralMode_FailsClosedUntilParameterized(DataSourceType provider)
        {
            var request = Request(provider, MetadataCommentObjectKind.Column);

            request.TableName = "A\\B";
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("D1B1")]
        public void MissingIdentityParts_ProduceNoSql()
        {
            var missingColumn = Request(DataSourceType.Oracle, MetadataCommentObjectKind.Column);

            missingColumn.ColumnName = null;
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(missingColumn));

            var missingSchema = Request(DataSourceType.PostgreSql, MetadataCommentObjectKind.Table);

            missingSchema.SchemaName = null;
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(missingSchema));

            var missingDatabase = Request(DataSourceType.SqlServer, MetadataCommentObjectKind.Table);

            missingDatabase.SchemaDatabase = null;
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(missingDatabase));
            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(null));

            var unsupported = Request(DataSourceType.None, MetadataCommentObjectKind.Table);

            Assert.AreEqual(string.Empty, MetadataCommentQueryBuilder.Build(unsupported));
        }
    }
}
