using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonQuery.Tests.Core.Database.Diagnostics.PostgreSql
{
    [TestClass]
    public sealed class PostgreSqlStringLengthErrorAnalyzerTests
    {
        private const string Error31 = "value too long for type character varying(31)";
        private const string Error32 = "value too long for type character varying(32)";
        private const string Error34 = "value too long for type character varying(34)";
        private const string Error35 = "value too long for type character varying(35)";

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql01_UpdateLiteral_ReturnsExactT2()
        {
            const string sql = "update a_test\r\n" +
                               "   set t2 = '123456789012345678901234567890123'\r\n" +
                               " where t1 is null\r\n" +
                               "returning t2";

            AssertExact(sql, Error32, "t2", 33, 32, null, "123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql02_InsertLiteral_ReturnsExactT1()
        {
            const string sql = "insert into a_test (t1)\r\n" +
                               "values ('123456789012345eeeeeeeeeeeeeeeeeee67889123')";

            AssertExact(sql, Error31, "t1", 42, 31, 1, "123456789012345eeeeeeeeeeeeeeeeeee67889123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql04_UpdateExpression_ReturnsFallbackCandidateT2()
        {
            const string sql = "update a_test\r\n" +
                               "   set t2 = repeat('1', 33)\r\n" +
                               " where t1 is null";

            var diagnostic = Analyze(sql, Error32);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Candidate, diagnostic.Confidence);
            Assert.AreEqual(1, diagnostic.Candidates.Count);
            AssertCandidate(diagnostic.Candidates[0], "t2", null, 32, null, string.Empty);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql05_UpdateEscapedQuote_ReturnsExactT4()
        {
            const string sql = "update a_test\r\n" +
                               "   set t4 = '1234567sssss890123456789012345678901''23'\r\n" +
                               " where t1 = '123456789012345678890123'";

            AssertExact(sql, Error34, "t4", 39, 34, null, "1234567sssss890123456789012345678901'23");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql07_UpdateAliasWithAs_ReturnsExactT4()
        {
            const string sql = "update a_test as a\r\n" +
                               "   set t4 = '123456755555589012345678901234567890123'\r\n" +
                               " where a.t1 = '123456789012345678890123'";

            AssertExact(sql, Error34, "t4", 39, 34, null, "123456755555589012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql08_UpdateAliasWithoutAs_ReturnsExactT4()
        {
            const string sql = "update a_test a\r\n" +
                               "   set t4 = '12345678922222012345678901234567890123'\r\n" +
                               " where a.t1 = '123456789012345678890123'";

            AssertExact(sql, Error34, "t4", 38, 34, null, "12345678922222012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql09_QualifiedTable_ReturnsExactT4()
        {
            const string sql = "update public.a_test\r\n" +
                               "   set t4 = '123456789044444444444412345678901234567890123'\r\n" +
                               " where t1 = '123456789012345678890123'";

            AssertExact(sql, Error34, "t4", 45, 34, null, "123456789044444444444412345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql10_MultipleUpdateColumns_ReturnsTwoCandidates()
        {
            const string sql = "update a_test\r\n" +
                               "   set t1 = '12345678901234567890123456789012',\r\n" +
                               "       t2 = '123456789012345678901234567890123'\r\n" +
                               " where t1 is null";

            var diagnostic = Analyze(sql, Error31);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Candidate, diagnostic.Confidence);
            Assert.AreEqual(2, diagnostic.Candidates.Count);

            AssertCandidate(diagnostic.Candidates[0], "t1", 32, 31, null, "12345678901234567890123456789012");
            AssertCandidate(diagnostic.Candidates[1], "t2", 33, 32, null, "123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql11_InsertWithComment_ReturnsExactT2()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "-- comment\r\n" +
                               "values ('123456789012345678901234567890123')";

            AssertExact(sql, Error32, "t2", 33, 32, 1, "123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql12_InsertEscapedQuote_ReturnsExactT2()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('1234567890123456789012345678901''23')";

            AssertExact(sql, Error32, "t2", 34, 32, 1, "1234567890123456789012345678901'23");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql13_QuotedSchemaTableAndColumn_ReturnsExactT4()
        {
            const string sql = "insert into \"public\".\"a_test\" (\"t4\")\r\n" +
                               "values ('1234567890123456789012345678901234567890')";

            AssertExact(sql, Error34, "t4", 40, 34, 1, "1234567890123456789012345678901234567890");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql14_QualifiedInsert_ReturnsExactT4()
        {
            const string sql = "insert into public.a_test (t4)\r\n" +
                               "values ('10123456789012345678901234567890123')";

            AssertExact(sql, Error34, "t4", 35, 34, 1, "10123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql15_TwoLongInsertColumns_ReturnsTwoCandidates()
        {
            const string sql = "insert into a_test (t5, t6)\r\n" +
                               "values ('123456789012345678901234567890123456',\r\n" +
                               "        '1234567890123456789012345678901234567')";

            var diagnostic = Analyze(sql, Error35);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Candidate, diagnostic.Confidence);
            Assert.AreEqual(2, diagnostic.Candidates.Count);

            AssertCandidate(diagnostic.Candidates[0], "t5", 36, 35, 1, "123456789012345678901234567890123456");
            AssertCandidate(diagnostic.Candidates[1], "t6", 37, 36, 1, "1234567890123456789012345678901234567");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql16_ThirdValuesRow_ReturnsExactT2AndRowThree()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('12345678901234567890123456789012'),\r\n" +
                               "       ('abc'),\r\n" +
                               "       ('123456789012345678901234567890123')";

            AssertExact(sql, Error32, "t2", 33, 32, 3, "123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql17_SecondValuesRow_ReturnsExactT2AndRowTwo()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('12345678901234567890123456789012'),\r\n" +
                               "       ('123456789012345678901234567890123')";

            AssertExact(sql, Error32, "t2", 33, 32, 2, "123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Analyze_Sql18_TwoLongInsertColumns_ReturnsTwoCandidates()
        {
            const string sql = "insert into a_test (t1, t2)\r\n" +
                               "values ('12345678901234567890123456789012', " +
                               "'123456789012345678901234567890123')";

            var diagnostic = Analyze(sql, Error31);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Candidate, diagnostic.Confidence);
            Assert.AreEqual(2, diagnostic.Candidates.Count);

            AssertCandidate(diagnostic.Candidates[0], "t1", 32, 31, 1, "12345678901234567890123456789012");
            AssertCandidate(diagnostic.Candidates[1], "t2", 33, 32, 1, "123456789012345678901234567890123");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WithNullMetadataProvider_ReturnsUnknownWithoutMessage()
        {
            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze("insert into a_test (t1) values ('abc')", Error31, null);

            AssertUnknown(diagnostic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WhenMetadataProviderThrows_ReturnsUnknownWithoutMessage()
        {
            var provider = new ThrowingMetadataProvider();

            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze("insert into a_test (t1) values ('abc')", Error31, provider);

            AssertUnknown(diagnostic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WhenMetadataIsEmpty_ReturnsUnknownWithoutMessage()
        {
            var provider = new StubMetadataProvider(new List<PostgreSqlColumnLengthInfo>());
            var diagnostic =PostgreSqlStringLengthErrorAnalyzer.TryAnalyze("insert into a_test (t1) values ('abc')", Error31, provider);

            AssertUnknown(diagnostic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WithUnsupportedSql_ReturnsUnknownWithoutMessage()
        {
            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze("delete from a_test where t1 = 'abc'", Error31, CreateProvider());

            AssertUnknown(diagnostic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryAnalyze_WhenInsertColumnAndValueCountsDiffer_ReturnsUnknown()
        {
            const string sql = "insert into a_test (t1, t2)\r\n" +
                               "values ('123456789012345678901234567890123')";

            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze(sql, "INSERT has more target columns than expressions", CreateProvider());

            AssertUnknown(diagnostic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_ExactLiteralTakesPrecedenceOverDifferentLengthInErrorMessage()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('123456789012345678901234567890123')";

            var diagnostic = Analyze(sql, "value too long for type character varying(999)");

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Exact, diagnostic.Confidence);
            Assert.AreEqual("t2", diagnostic.Candidates[0].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WithExpressionAndNoLengthInError_ReturnsUnknown()
        {
            const string sql = "update a_test\r\n" +
                               "set t2 = repeat('1', 33)";

            var diagnostic = Analyze(sql, "value too long");

            AssertUnknown(diagnostic);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WithCharacterErrorMessage_UsesCharacterFallback()
        {
            const string sql = "update a_test\r\n" +
                               "set c1 = repeat('1', 9)";

            var provider = new StubMetadataProvider
            (
                new[]
                {
                    CreateColumn("c1", "character", 8)
                }
            );

            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze(sql, "value too long for type character(8)", provider);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Candidate, diagnostic.Confidence);
            Assert.AreEqual(1, diagnostic.Candidates.Count);
            Assert.AreEqual("c1", diagnostic.Candidates[0].ColumnName);
            Assert.AreEqual(8, diagnostic.Candidates[0].MaxLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WithUnicodeSurrogatePair_CountsTextElements()
        {
            const string sql = "insert into a_test (emoji_text)\r\n" +
                               "values ('A😀B')";

            var provider = new StubMetadataProvider
            (
                new[]
                {
                    CreateColumn("emoji_text", "character varying", 2)
                }
            );

            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze(sql, "value too long for type character varying(2)", provider);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Exact, diagnostic.Confidence);
            Assert.AreEqual(3, diagnostic.Candidates[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_WithVeryLongValue_TruncatesPreviewToFourThousandCharacters()
        {
            var value = new string('x', 4001);
            var sql = "insert into a_test (long_text) values ('" +
                      value +
                      "')";

            var provider = new StubMetadataProvider
            (
                new[]
                {
                    CreateColumn("long_text", "character varying", 4000)
                }
            );

            var diagnostic = PostgreSqlStringLengthErrorAnalyzer.TryAnalyze(sql, "value too long for type character varying(4000)", provider);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Exact, diagnostic.Confidence);
            Assert.AreEqual(4001, diagnostic.Candidates[0].ActualLength);
            Assert.AreEqual(4003, diagnostic.Candidates[0].SourceValuePreview.Length);
            Assert.IsTrue(diagnostic.Candidates[0].SourceValuePreview.EndsWith("...", StringComparison.Ordinal));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryAnalyze_RequestsMetadataForParsedSchemaAndTable()
        {
            const string sql = "insert into public.a_test (t1)\r\n" +
                               "values ('12345678901234567890123456789012')";

            var provider = CreateProvider();

            PostgreSqlStringLengthErrorAnalyzer.TryAnalyze(sql, Error31, provider);

            Assert.AreEqual("public", provider.LastSchemaName);
            Assert.AreEqual("a_test", provider.LastTableName);
        }

        private static PostgreSqlStringLengthDiagnostic Analyze(string sql, string errorMessage)
        {
            return PostgreSqlStringLengthErrorAnalyzer.TryAnalyze(sql, errorMessage, CreateProvider());
        }

        private static void AssertExact(string sql, string errorMessage, string expectedColumn, int expectedActualLength, int expectedMaxLength,
                                        int? expectedRowIndex, string expectedPreview)
        {
            var diagnostic = Analyze(sql, errorMessage);

            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Exact, diagnostic.Confidence);
            Assert.AreEqual(1, diagnostic.Candidates.Count);
            Assert.IsTrue(diagnostic.HasDiagnosticMessage);

            AssertCandidate(diagnostic.Candidates[0], expectedColumn, expectedActualLength, expectedMaxLength, expectedRowIndex, expectedPreview);
        }

        private static void AssertCandidate(PostgreSqlStringLengthCandidate candidate, string expectedColumn, int? expectedActualLength,
                                            int expectedMaxLength, int? expectedRowIndex, string expectedPreview)
        {
            Assert.AreEqual("public", candidate.SchemaName);
            Assert.AreEqual("a_test", candidate.TableName);
            Assert.AreEqual(expectedColumn, candidate.ColumnName);
            Assert.AreEqual($"public.a_test.{expectedColumn}", candidate.FullColumnName);
            Assert.AreEqual(expectedActualLength, candidate.ActualLength);
            Assert.AreEqual(expectedMaxLength, candidate.MaxLength);
            Assert.AreEqual(expectedRowIndex, candidate.ValuesRowIndex);
            Assert.AreEqual(expectedPreview, candidate.SourceValuePreview);
        }

        private static void AssertUnknown(PostgreSqlStringLengthDiagnostic diagnostic)
        {
            Assert.AreEqual(PostgreSqlStringLengthDiagnosticConfidence.Unknown, diagnostic.Confidence);
            Assert.AreEqual(0, diagnostic.Candidates.Count);
            Assert.IsFalse(diagnostic.HasDiagnosticMessage);
            Assert.AreEqual(string.Empty, diagnostic.Message);
        }

        private static StubMetadataProvider CreateProvider()
        {
            return new StubMetadataProvider
            (
                new[]
                {
                    CreateColumn("t1", "character varying", 31),
                    CreateColumn("t2", "character varying", 32),
                    CreateColumn("t4", "character varying", 34),
                    CreateColumn("t5", "character varying", 35),
                    CreateColumn("t6", "character varying", 36)
                }
            );
        }

        private static PostgreSqlColumnLengthInfo CreateColumn(string columnName, string dataType, int maxLength)
        {
            return new PostgreSqlColumnLengthInfo
            {
                SchemaName = "public",
                TableName = "a_test",
                ColumnName = columnName,
                DataType = dataType,
                CharacterMaximumLength = maxLength
            };
        }

        private sealed class StubMetadataProvider : IPostgreSqlColumnLengthMetadataProvider
        {
            private readonly IReadOnlyList<PostgreSqlColumnLengthInfo> _columns;

            public StubMetadataProvider(IEnumerable<PostgreSqlColumnLengthInfo> columns)
            {
                _columns = columns.ToList();
            }

            public string LastSchemaName { get; private set; }

            public string LastTableName { get; private set; }

            public IReadOnlyList<PostgreSqlColumnLengthInfo> GetColumnLengthInfo(string schemaName, string tableName)
            {
                LastSchemaName = schemaName;
                LastTableName = tableName;

                return _columns;
            }

            public bool TryGetColumnLengthInfo(string schemaName, string tableName, string columnName, out PostgreSqlColumnLengthInfo columnInfo)
            {
                columnInfo = _columns.FirstOrDefault(item => string.Equals(item.ColumnName, columnName, StringComparison.Ordinal));

                return columnInfo != null;
            }
        }

        private sealed class ThrowingMetadataProvider : IPostgreSqlColumnLengthMetadataProvider
        {
            public IReadOnlyList<PostgreSqlColumnLengthInfo> GetColumnLengthInfo(string schemaName, string tableName)
            {
                throw new InvalidOperationException("metadata failed");
            }

            public bool TryGetColumnLengthInfo(string schemaName, string tableName, string columnName, out PostgreSqlColumnLengthInfo columnInfo)
            {
                columnInfo = null;
                throw new InvalidOperationException("metadata failed");
            }
        }
    }
}
