using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JasonQuery.Tests.Core.Sql.Lexing
{
    [TestClass]
    public sealed class SqlTokenizerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_NullAndEmpty_ReturnNoTokens()
        {
            Assert.AreEqual(0, SqlTokenizer.Tokenize(null, DataSourceType.PostgreSql).Tokens.Count);
            Assert.AreEqual(0, SqlTokenizer.Tokenize(string.Empty, DataSourceType.PostgreSql).Tokens.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_CommonSql_PreservesExactSourceSpans()
        {
            const string sql = "SELECT * FROM t WHERE id = 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            Assert.IsTrue(result.Tokens.Count > 0);

            foreach (var token in result.Tokens)
            {
                Assert.AreEqual
                (
                    token.Text,
                    sql.Substring(token.Start, token.Length)
                );
            }

            var where = FindWord(result, "WHERE");

            Assert.AreEqual(sql.IndexOf("WHERE", StringComparison.Ordinal), where.Start);
            Assert.AreEqual(0, where.Depth);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_NestedParentheses_AssignsExpectedDepth()
        {
            const string sql = "SELECT f(a, (b + c)) FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var b = FindWord(result, "b");
            var from = FindWord(result, "FROM");

            Assert.AreEqual(2, b.Depth);
            Assert.AreEqual(0, from.Depth);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_ParenthesesInsideString_DoNotChangeDepth()
        {
            const string sql = "SELECT func('(x)', (1 + 2)) FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var literal = FindToken(result, "'(x)'");
            var from = FindWord(result, "FROM");

            Assert.AreEqual(SqlTokenKind.StringLiteral, literal.Kind);
            Assert.AreEqual(1, literal.Depth);
            Assert.AreEqual(0, from.Depth);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_DoubledSingleQuote_RemainsOneStringToken()
        {
            const string sql = "SELECT 'a''b' FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle);

            var literal = FindToken(result, "'a''b'");

            Assert.AreEqual(SqlTokenKind.StringLiteral, literal.Kind);
            Assert.IsTrue(literal.SuppressesKeywordMatching);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_LineComment_ProducesProtectedToken()
        {
            const string sql = "SELECT * FROM t -- FOR UPDATE\r\nWHERE id = 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var comment = FindToken(result, "-- FOR UPDATE");
            var where = FindWord(result, "WHERE");

            Assert.AreEqual(SqlTokenKind.LineComment, comment.Kind);
            Assert.IsTrue(comment.IsProtectedText);
            Assert.AreEqual(sql.IndexOf("WHERE", StringComparison.Ordinal), where.Start);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_BlockComment_ProducesProtectedToken()
        {
            const string sql = "SELECT /* FOR UPDATE */ 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle);

            var comment = FindToken(result, "/* FOR UPDATE */");

            Assert.AreEqual(SqlTokenKind.BlockComment, comment.Kind);
            Assert.IsTrue(comment.IsProtectedText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_UnterminatedString_ConsumesRemainder()
        {
            const string sql = "SELECT 'unterminated FOR UPDATE";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var literal = result.Tokens.Last();

            Assert.AreEqual(SqlTokenKind.StringLiteral, literal.Kind);
            Assert.AreEqual("'unterminated FOR UPDATE", literal.Text);
            Assert.AreEqual(sql.Length, literal.EndExclusive);
            Assert.IsFalse(result.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_UnterminatedBlockComment_ConsumesRemainder()
        {
            const string sql = "SELECT /* unterminated FOR UPDATE";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.SqlServer);

            var comment = result.Tokens.Last();

            Assert.AreEqual(SqlTokenKind.BlockComment, comment.Kind);
            Assert.AreEqual(sql.Length, comment.EndExclusive);
            Assert.IsFalse(result.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_DoubleQuotedIdentifier_SuppressesKeywordMatching()
        {
            const string sql = "SELECT \"FOR UPDATE\" FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var quoted = FindToken(result, "\"FOR UPDATE\"");

            Assert.AreEqual(SqlTokenKind.DelimitedIdentifier, quoted.Kind);
            Assert.IsTrue(quoted.SuppressesKeywordMatching);
            Assert.IsFalse(quoted.IsProtectedText);
            Assert.IsFalse(result.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [DataRow(DataSourceType.Oracle, "[WHERE]")]
        [DataRow(DataSourceType.PostgreSql, "`WHERE`")]
        [DataRow(DataSourceType.SqlServer, "`WHERE`")]
        [DataRow(DataSourceType.MySql, "[WHERE]")]
        public void Tokenize_ConservativeDelimitedIdentifiers_RemainSingleTokens(DataSourceType dataSourceType, string quoted)
        {
            var sql = "SELECT " + quoted + " FROM t";
            var result = SqlTokenizer.Tokenize(sql, dataSourceType);
            var token = FindToken(result, quoted);

            Assert.AreEqual(SqlTokenKind.DelimitedIdentifier, token.Kind);
            Assert.IsFalse(result.Tokens.Any(candidate => candidate.IsWord("WHERE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_OracleAlternativeQuote_BracketFormIsOneStringToken()
        {
            const string sql = "SELECT q'[FOR UPDATE]' FROM dual";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle);

            var token = FindToken(result, "q'[FOR UPDATE]'");

            Assert.AreEqual(SqlTokenKind.StringLiteral, token.Kind);
            Assert.IsFalse(result.Tokens.Any(candidate => candidate.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_OracleAlternativeQuote_ParenthesesDoNotChangeDepth()
        {
            const string sql = "SELECT q'{(FOR UPDATE)}', (1) FROM dual";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle);

            var literal = FindToken(result, "q'{(FOR UPDATE)}'");
            var one = FindToken(result, "1");
            var from = FindWord(result, "FROM");

            Assert.AreEqual(0, literal.Depth);
            Assert.AreEqual(1, one.Depth);
            Assert.AreEqual(0, from.Depth);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_PostgreSqlDollarQuote_IsOneStringToken()
        {
            const string sql = "SELECT $$FOR UPDATE$$";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var token = FindToken(result, "$$FOR UPDATE$$");

            Assert.AreEqual(SqlTokenKind.StringLiteral, token.Kind);
            Assert.IsFalse(result.Tokens.Any(candidate => candidate.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_PostgreSqlTaggedDollarQuote_IsOneStringToken()
        {
            const string sql = "SELECT $tag$FOR NO KEY UPDATE$tag$";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var token = FindToken(result, "$tag$FOR NO KEY UPDATE$tag$");

            Assert.AreEqual(SqlTokenKind.StringLiteral, token.Kind);
            Assert.AreEqual(sql.IndexOf("$tag$", StringComparison.Ordinal), token.Start);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_PostgreSqlDollarQuotedParentheses_DoNotChangeDepth()
        {
            const string sql = "SELECT $tag$(x)$tag$, (1)";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var literal = FindToken(result, "$tag$(x)$tag$");
            var one = FindToken(result, "1");

            Assert.AreEqual(0, literal.Depth);
            Assert.AreEqual(1, one.Depth);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_NestedBlockComment_RemainsOneCommentToken()
        {
            const string sql = "/* outer /* inner */ outer */ SELECT 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            Assert.AreEqual(SqlTokenKind.BlockComment, result.Tokens[0].Kind);
            Assert.AreEqual("/* outer /* inner */ outer */", result.Tokens[0].Text);
            Assert.AreEqual("SELECT", result.Tokens[1].Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_SqlServerBracketIdentifier_WithEscapedBracketIsOneToken()
        {
            const string sql = "SELECT [A]]B] FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.SqlServer);

            var token = FindToken(result, "[A]]B]");

            Assert.AreEqual(SqlTokenKind.DelimitedIdentifier, token.Kind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_SqlServerLockHint_LeavesExecutableHintWords()
        {
            const string sql = "SELECT * FROM t WITH (UPDLOCK)";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.SqlServer);

            var withToken = FindWord(result, "WITH");
            var updateLock = FindWord(result, "UPDLOCK");

            Assert.AreEqual(0, withToken.Depth);
            Assert.AreEqual(1, updateLock.Depth);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_MySqlBackslashEscape_DoesNotEndStringEarly()
        {
            const string sql = "SELECT 'a\\' FOR UPDATE' FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.MySql);

            var literal = FindToken(result, "'a\\' FOR UPDATE'");

            Assert.AreEqual(SqlTokenKind.StringLiteral, literal.Kind);
            Assert.IsFalse(result.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_MySqlHashComment_IsProtected()
        {
            const string sql = "# FOR UPDATE\r\nSELECT 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.MySql);

            Assert.AreEqual(SqlTokenKind.LineComment, result.Tokens[0].Kind);
            Assert.AreEqual("# FOR UPDATE", result.Tokens[0].Text);
            Assert.AreEqual("SELECT", result.Tokens[1].Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_MySqlDashComment_WithWhitespaceIsProtected()
        {
            const string sql = "-- FOR UPDATE\r\nSELECT 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.MySql);

            Assert.AreEqual(SqlTokenKind.LineComment, result.Tokens[0].Kind);
            Assert.AreEqual("-- FOR UPDATE", result.Tokens[0].Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_MySqlDashWithoutWhitespace_IsNotComment()
        {
            const string sql = "--FOR UPDATE\r\nSELECT 1";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.MySql);

            Assert.IsFalse(result.Tokens.Any(token => token.Kind == SqlTokenKind.LineComment));
            Assert.IsTrue(result.Tokens.Any(token => token.IsWord("FOR")));
            Assert.IsTrue(result.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_UnicodeText_PreservesDotNetStringIndexes()
        {
            const string sql = "SELECT 中文, '測試' FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var from = FindWord(result, "FROM");

            Assert.AreEqual(sql.IndexOf("FROM", StringComparison.Ordinal), from.Start);
            Assert.AreEqual("FROM", sql.Substring(from.Start, from.Length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void Tokenize_CrLf_PreservesOriginalIndexes()
        {
            const string sql = "prefix\r\nSELECT *\r\nFROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.SqlServer);

            var from = FindWord(result, "FROM");

            Assert.AreEqual(sql.IndexOf("FROM", StringComparison.Ordinal), from.Start);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void IsInsideProtectedText_ReturnsTrueForStringAndComment()
        {
            const string sql = "SELECT 'abc' /* def */ FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle);

            Assert.IsTrue(result.IsInsideProtectedText(sql.IndexOf("abc", StringComparison.Ordinal)));
            Assert.IsTrue(result.IsInsideProtectedText(sql.IndexOf("def", StringComparison.Ordinal)));
            Assert.IsFalse(result.IsInsideProtectedText(sql.IndexOf("FROM", StringComparison.Ordinal)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        public void IsInsideKeywordSuppressedText_IncludesDelimitedIdentifier()
        {
            const string sql = "SELECT \"WHERE\" FROM t";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);
            var position = sql.IndexOf("WHERE", StringComparison.Ordinal);

            Assert.IsFalse(result.IsInsideProtectedText(position));
            Assert.IsTrue(result.IsInsideKeywordSuppressedText(position));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_MySqlDashCommentCompatibilityOption_AllowsDashCommentWithoutWhitespace()
        {
            const string sql = "--FOR UPDATE\r\nSELECT 1";

            var result = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.MySql,
                SqlTokenizerOptions.MySqlDashCommentWithoutWhitespace
            );

            Assert.AreEqual(SqlTokenKind.LineComment, result.Tokens[0].Kind);
            Assert.AreEqual("--FOR UPDATE", result.Tokens[0].Text);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_MySqlHashInsideWordCompatibilityOption_StartsCommentAtHash()
        {
            const string sql = "SELECT abc#FOR UPDATE\r\nSELECT 1";

            var result = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.MySql,
                SqlTokenizerOptions.MySqlHashStartsCommentInsideWord
            );

            Assert.IsTrue(result.Tokens.Any(token => token.Kind == SqlTokenKind.Word && token.Text == "abc"));
            Assert.IsTrue(result.Tokens.Any(token => token.Kind == SqlTokenKind.LineComment && token.Text == "#FOR UPDATE"));
            Assert.IsFalse(result.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_WithoutForeignDelimitedIdentifierOption_AllowsNestedStringToRemainProtected()
        {
            const string sql = "SELECT ['FOR UPDATE'] FROM dual";
            var result = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle, SqlTokenizerOptions.None);

            Assert.IsFalse(result.Tokens.Any(token => token.Kind == SqlTokenKind.DelimitedIdentifier && token.Text.StartsWith("[")));
            Assert.AreEqual(SqlTokenKind.StringLiteral, FindToken(result, "'FOR UPDATE'").Kind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_MySqlDoubleQuoteBackslashOption_IsConfigurable()
        {
            const string sql = "SELECT \"a\\\" FOR UPDATE\" FROM t";

            var defaultResult = SqlTokenizer.Tokenize(sql, DataSourceType.MySql);
            var compatibilityResult = SqlTokenizer.Tokenize(sql, DataSourceType.MySql, SqlTokenizerOptions.None);

            Assert.IsFalse(defaultResult.Tokens.Any(token => token.IsWord("UPDATE")));
            Assert.IsTrue(compatibilityResult.Tokens.Any(token => token.IsWord("UPDATE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_MySqlBacktickBackslashOption_IsConfigurable()
        {
            const string sql = "SELECT `a\\` FOR UPDATE` FROM t";

            var escapedResult = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.MySql,
                SqlTokenizerOptions.MySqlBacktickIdentifierAllowsBackslashEscape
            );

            var unescapedResult = SqlTokenizer.Tokenize(sql, DataSourceType.MySql, SqlTokenizerOptions.None);

            Assert.IsFalse(escapedResult.Tokens.Any(token => token.IsWord("UPDATE")));
            Assert.IsTrue(unescapedResult.Tokens.Any(token => token.IsWord("UPDATE")));
        }
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_DisablePostgreSqlDollarQuotedText_PreservesLegacyConsumerBehavior()
        {
            const string sql = "SELECT $$WHERE (x)$$ FROM t";

            var defaultResult = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var compatibilityResult = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.PostgreSql,
                SqlTokenizerOptions.DisablePostgreSqlDollarQuotedText
            );

            Assert.IsTrue(defaultResult.Tokens.Any(token => token.Kind == SqlTokenKind.StringLiteral && token.Text == "$$WHERE (x)$$"));
            Assert.IsFalse(compatibilityResult.Tokens.Any(token => token.Kind == SqlTokenKind.StringLiteral && token.Text == "$$WHERE (x)$$"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_DisableOracleAlternativeQuotedText_PreservesLegacyConsumerBehavior()
        {
            const string sql = "SELECT q'[x' WHERE y]' FROM dual";

            var defaultResult = SqlTokenizer.Tokenize(sql, DataSourceType.Oracle);

            var compatibilityResult = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.Oracle,
                SqlTokenizerOptions.DisableOracleAlternativeQuotedText
            );

            Assert.IsTrue(defaultResult.Tokens.Any(token => token.Kind == SqlTokenKind.StringLiteral && token.Text == "q'[x' WHERE y]'"));
            Assert.IsFalse(compatibilityResult.Tokens.Any(token => token.Kind == SqlTokenKind.StringLiteral && token.Text == "q'[x' WHERE y]'"));
            Assert.IsTrue(compatibilityResult.Tokens.Any(token => token.IsWord("WHERE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_DisableNestedBlockComments_StopsAtFirstClosingDelimiter()
        {
            const string sql = "/* outer /* inner */ WHERE */ SELECT 1";

            var defaultResult = SqlTokenizer.Tokenize(sql, DataSourceType.PostgreSql);

            var compatibilityResult = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.PostgreSql,
                SqlTokenizerOptions.DisableNestedBlockComments
            );

            Assert.IsFalse(defaultResult.Tokens.Any(token => token.IsWord("WHERE")));
            Assert.IsTrue(compatibilityResult.Tokens.Any(token => token.IsWord("WHERE")));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexing")]
        [TestCategory("SqlLexingMigration")]
        public void Tokenize_DisableMySqlSingleQuotedBackslashEscape_PreservesLegacyConsumerBehavior()
        {
            const string sql = "SELECT 'a\\' WHERE ' FROM t";

            var defaultResult = SqlTokenizer.Tokenize(sql, DataSourceType.MySql);

            var compatibilityResult = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.MySql,
                SqlTokenizerOptions.DisableMySqlSingleQuotedStringBackslashEscape
            );

            Assert.IsFalse(defaultResult.Tokens.Any(token => token.IsWord("WHERE")));
            Assert.IsTrue(compatibilityResult.Tokens.Any(token => token.IsWord("WHERE")));
        }
        private static SqlToken FindWord(SqlTokenizationResult result, string word)
        {
            var token = result.Tokens.FirstOrDefault(candidate => candidate.IsWord(word));

            Assert.IsNotNull(token, "Expected word token was not found: " + word);
            return token;
        }

        private static SqlToken FindToken(SqlTokenizationResult result, string text)
        {
            var token = result.Tokens.FirstOrDefault(candidate => string.Equals(candidate.Text, text, StringComparison.Ordinal));

            Assert.IsNotNull(token, "Expected token was not found: " + text);
            return token;
        }
    }
}
