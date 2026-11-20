using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-S1-C3C-D3C-04A: Input-rejection feedback GOLDEN RED, not permission to execute DDL.
    //No database, UI form instance, transaction, SQL dispatch or Git operation is used.
    [TestClass]
    [TestCategory("F7S1C3CD3C04A")]
    public sealed class SchemaExplorerMySqlCommentInputFeedbackGoldenTests
    {
        //D3C-02: verified MySQL 8.0.34 SHOW CREATE representation (two backslashes).
        private const string Definition = @"int NOT NULL DEFAULT '1' COMMENT 'It''s C:\\Data; SELECT 1; --'";
        private const string OriginalComment = @"It's C:\Data; SELECT 1; --";
        private const string NewComment = @"D3 It's C:\Data; SELECT 2; --";
        private const string NoBackslashMode = "STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES";
        private const string OrdinaryMode = "STRICT_TRANS_TABLES";
        private const string BlockReasonKey = "MySqlColumnCommentNbeControlCharactersUnsupported";

        private static readonly Guid PreviewConnection = new Guid("3B0C7D2B-9F2B-4B34-B52B-0C99AC02844B");
        private static readonly Guid LiveConnection = new Guid("739B3B6A-C0F1-46C8-9646-CC8E9AB85B7E");

        private static ColumnDdlRequest Request(string comment)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = "sakila",
                TableName = "jq_c3c_comment_smoke",
                ColumnName = "id",
                Comment = comment
            };
        }

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(Guid connection, string mode)
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                connection, "sakila", "jq_c3c_comment_smoke", "id",
                "8.0.34", Definition, 883, "verified-attributes", mode
            );
        }

        //Compile-clean RED: the future production method is looked up by reflection.
        //Do not add a test-only implementation or change this assertion to make RED pass.
        private static string InputRejectionReason(string original, string current, string verifiedMode, bool hasGeneratedSql)
        {
            var method = typeof(MySqlColumnDdlPreviewFeedbackPolicy).GetMethod
            (
                "GetInputRejectionReasonKey",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(string), typeof(string), typeof(bool) },
                null
            );

            Assert.IsNotNull(method, "EXPECTED RED: input-specific MySQL comment preview feedback policy is missing.");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return (string)method.Invoke
            (
                null,
                new object[] { original, current, verifiedMode, hasGeneratedSql }
            );
        }

        [TestMethod]
        public void D3C04A_G01_ValidNoBackslashSingleLinePreviewMustRemainAvailable()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(NewComment), Definition, true, out var sql
            ));

            Assert.IsTrue(sql.EndsWith("COMMENT 'D3 It''s C:\\Data; SELECT 2; --';", StringComparison.Ordinal));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(sql, true));
        }

        [TestMethod]
        public void D3C04A_G02_NoBackslashModeMustStillRejectCarriageReturnAndLineFeed()
        {
            var inputs = new[]
            {
                NewComment + "\r\n",
                NewComment + "\r",
                NewComment + "\n",
                "First line\r\nSecond line"
            };

            foreach (var input in inputs)
            {
                Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
                (
                    Request(input), Definition, true, out var sql
                ));

                Assert.AreEqual(string.Empty, sql);
                Assert.AreNotEqual(OriginalComment, input, "An edited input is not an unchanged comment.");
            }
        }

        [TestMethod]
        public void D3C04A_G03_NoBackslashModeMustStillRejectOtherUnsupportedControls()
        {
            var inputs = new[]
            {
                "Before\tAfter",
                "Before\bAfter",
                "Before\u001aAfter"
            };

            foreach (var input in inputs)
            {
                Assert.IsFalse(MySqlColumnCommentLiteralPolicy.TryQuote(input, true, out var literal));
                Assert.AreEqual(string.Empty, literal);
            }
        }

        [TestMethod]
        public void D3C04A_G04_OrdinaryModeControlEscapingMustRemainUnchanged()
        {
            const string input = "First line\r\nSecond line";

            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryQuote(input, false, out var literal));
            Assert.AreEqual("'First line\\r\\nSecond line'", literal);
            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryUnquote(literal, false, out var restored));
            Assert.AreEqual(input, restored);
        }

        [TestMethod]
        public void D3C04A_G05_UnverifiedModeMustNotProduceModeSensitiveSql()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(NewComment + "\n"), Definition, out var sql
            ));

            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void D3C04A_G06_ExecuteTimeModeChangeMustStillBeRejected()
        {
            var request = Request(NewComment);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                request, Definition, true, out var previewSql
            ));

            Assert.AreEqual
            (
                "F7S1_C3C_SESSION_MODE_CHANGED",
                MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                (
                    ColumnDdlOperation.Comment,
                    Receipt(PreviewConnection, NoBackslashMode),
                    Receipt(LiveConnection, OrdinaryMode),
                    request, previewSql
                )
            );
        }

        [TestMethod]
        public void D3C04A_R07_InputRejectionReasonMustDistinguishChangeFromUnsafeInput()
        {
            //EXPECTED RED: no input-rejection classifier currently exists.
            //The method must consume verified SESSION mode; it must not create an Execute permit.
            Assert.AreEqual
            (
                BlockReasonKey,
                InputRejectionReason(OriginalComment, NewComment + "\r\n", NoBackslashMode, false)
            );

            Assert.AreEqual
            (
                BlockReasonKey,
                InputRejectionReason(OriginalComment, "First\tSecond", NoBackslashMode, false)
            );

            Assert.AreEqual(string.Empty, InputRejectionReason(OriginalComment, OriginalComment, NoBackslashMode, false));
            Assert.AreEqual(string.Empty, InputRejectionReason(OriginalComment, NewComment, NoBackslashMode, true));
            Assert.AreEqual(string.Empty, InputRejectionReason(OriginalComment, NewComment, NoBackslashMode, false));
            Assert.AreEqual(string.Empty, InputRejectionReason(OriginalComment, NewComment + "\n", OrdinaryMode, false));
            Assert.AreEqual(string.Empty, InputRejectionReason(OriginalComment, NewComment + "\n", null, false));
        }

        [TestMethod]
        public void D3C04A_R08_UnsupportedInputNeedsDedicatedActionableFeedback()
        {
            //EXPECTED RED: the current fallback is a generic definition-unverified message.
            var message = MySqlColumnDdlPreviewFeedbackPolicy.GetDefaultMessage(BlockReasonKey);

            Assert.Contains("NO_BACKSLASH_ESCAPES", message);
            Assert.Contains("line break", message);
            Assert.Contains("Execute", message);
        }
    }
}
