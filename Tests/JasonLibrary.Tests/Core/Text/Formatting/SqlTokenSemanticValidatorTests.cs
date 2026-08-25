using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlTokenSemanticValidatorTests
    {
        [TestMethod]
        public void Validate_ExactlyAuthorizedKeywordCaseChanges_ReturnSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.SqlServer,
                "select Id, Name from dbo.Customer where IsActive = 1",
                "SELECT Id,\r\n       Name\r\nFROM dbo.Customer\r\nWHERE IsActive=1",
                new[] { 0, 4, 8 }
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
            Assert.AreEqual(-1, result.TokenIndex);
        }

        [TestMethod]
        public void Validate_OracleUserCaseChangeAtAuthorizedOccurrence_ReturnsSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.Oracle,
                "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.OWNER = USER",
                "select C.OWNER from ALL_TAB_COLUMNS C where C.OWNER = user",
                new[] { 0, 4, 7, 12 }
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }

        [TestMethod]
        public void Validate_CaseChangeWithoutAuthorization_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.SqlServer,
                "select Id from Customer",
                "SELECT Id from Customer"
            );

            Assert.IsFalse(result.IsSafe);
            Assert.AreEqual(0, result.TokenIndex);
            Assert.AreEqual("select", result.OriginalToken);
            Assert.AreEqual("SELECT", result.FormattedToken);
        }

        [TestMethod]
        public void Validate_CaseChangeAuthorizedAtDifferentIndex_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.SqlServer,
                "select Id from Customer",
                "SELECT Id from Customer",
                new[] { 2 }
            );

            Assert.IsFalse(result.IsSafe);
            Assert.AreEqual(0, result.TokenIndex);
        }

        [TestMethod]
        public void Validate_NullCaseChangeAuthorization_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<System.ArgumentNullException>
            (
                () => SqlTokenSemanticValidator.Validate
                (
                    DatabaseProviderKind.SqlServer,
                    "SELECT 1",
                    "SELECT 1",
                    null
                )
            );
        }

        [TestMethod]
        public void Validate_UnquotedIdentifierCaseChange_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                "SELECT CustomerName FROM customer",
                "SELECT CUSTOMERNAME FROM customer"
            );

            Assert.IsFalse(result.IsSafe);
            Assert.AreEqual("CustomerName", result.OriginalToken);
            Assert.AreEqual("CUSTOMERNAME", result.FormattedToken);
        }

        [TestMethod]
        public void Validate_StringLiteralChange_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.Oracle,
                "SELECT 'Jason' FROM dual",
                "SELECT 'JASON' FROM dual"
            );

            Assert.IsFalse(result.IsSafe);
            StringAssert.Contains(result.ErrorMessage, "StringLiteral");
        }

        [TestMethod]
        public void Validate_PostgreSqlConcatenationSplitByWhitespace_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                "SELECT '\"' || value || '\"' FROM sample",
                "SELECT '\"' | | value | | '\"' FROM sample"
            );

            Assert.IsFalse(result.IsSafe);
            Assert.AreEqual("||", result.OriginalToken);
            Assert.AreEqual("|", result.FormattedToken);
        }

        [TestMethod]
        public void Validate_PostgreSqlEscapeStringPrefixSeparated_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                "SELECT E'\\n'",
                "SELECT E '\\n'"
            );

            Assert.IsFalse(result.IsSafe);
        }

        [TestMethod]
        public void Validate_CommaMovedBehindLineComment_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                "SELECT first_value, -- keep comma active\nsecond_value FROM sample",
                "SELECT first_value -- keep comma active\n, second_value FROM sample"
            );

            Assert.IsFalse(result.IsSafe);
            Assert.AreEqual(",", result.OriginalToken);
            Assert.AreEqual("-- keep comma active", result.FormattedToken);
        }

        [TestMethod]
        public void Validate_CommentOrderAndTextPreserved_ReturnsSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.SqlServer,
                "SELECT Id, -- identity\nName /* display */ FROM dbo.Customer",
                "SELECT Id,\r\n    -- identity\r\n    Name /* display */\r\nFROM dbo.Customer"
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }

        [TestMethod]
        public void Validate_PostgreSqlDollarQuotedLiteralPreserved_ReturnsSafe()
        {
            const string sql = "SELECT $body$begin\nraise notice 'ok';\nend$body$";

            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                sql,
                "SELECT\r\n    $body$begin\r\nraise notice 'ok';\r\nend$body$"
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }

        [TestMethod]
        public void Validate_OracleAlternativeQuotedLiteralPreserved_ReturnsSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.Oracle,
                "SELECT q'[Jason's SQL]' FROM dual",
                "SELECT Q'[Jason's SQL]'\r\nFROM dual"
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }

        [TestMethod]
        public void Validate_MySqlBackslashEscapedQuotePreserved_ReturnsSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.MySql,
                "SELECT 'Jason\\'s SQL' FROM `sample`",
                "SELECT\r\n    'Jason\\'s SQL'\r\nFROM `sample`"
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }

        [TestMethod]
        public void Validate_SqlServerNationalStringPrefixPreserved_ReturnsSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.SqlServer,
                "SELECT N'中文' AS Caption",
                "SELECT N'中文'\r\nAS Caption"
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }

        [TestMethod]
        public void Validate_QuotedIdentifierChange_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                "SELECT \"MixedCase\" FROM sample",
                "SELECT \"MIXEDCASE\" FROM sample"
            );

            Assert.IsFalse(result.IsSafe);
        }

        [TestMethod]
        public void Validate_AddedToken_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.Sqlite,
                "SELECT Id FROM sample",
                "SELECT DISTINCT Id FROM sample"
            );

            Assert.IsFalse(result.IsSafe);
            StringAssert.Contains(result.ErrorMessage, "token");
        }

        [TestMethod]
        public void Validate_UnterminatedFormattedLiteral_ReturnsUnsafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.SqlServer,
                "SELECT 'safe'",
                "SELECT 'unsafe"
            );

            Assert.IsFalse(result.IsSafe);
            StringAssert.Contains(result.ErrorMessage, "could not be tokenized safely");
        }

        [TestMethod]
        public void Validate_TabAndLineEndingNormalization_ReturnsSafe()
        {
            var result = SqlTokenSemanticValidator.Validate
            (
                DatabaseProviderKind.PostgreSql,
                "SELECT E'line1\tline2\nline3'",
                "SELECT E'line1    line2\r\nline3'"
            );

            Assert.IsTrue(result.IsSafe, result.ErrorMessage);
        }
    }
}
