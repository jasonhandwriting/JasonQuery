using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-S1-C3-B4-C3C-A: MySQL Comment literal session-mode safety, Golden-first.
    //No database connection, DDL dispatch, transaction change or Git mutation.
    //A pure decision or SQL shape is never permission to Execute.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlColumnCommentSessionModeGoldenTests
    {
        private const string Definition = "varchar(32) NOT NULL DEFAULT 'a' COMMENT 'old'";
        private const string TablePrefix = "ALTER TABLE `demo`.`comment_test` MODIFY COLUMN `note` ";

        private static ColumnDdlRequest Request(string comment)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = "demo",
                TableName = "comment_test",
                ColumnName = "note",
                Comment = comment
            };
        }

        private static Type FindPolicy(string fullName)
        {
            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType(fullName, false);

            Assert.IsNotNull(type, "C3C EXPECTED RED: missing pure policy " + fullName);
            return type;
        }

        private static MethodInfo FindMethod(Type type, string methodName, params Type[] parameterTypes)
        {
            var method = type.GetMethod
            (
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
                null,
                parameterTypes,
                null
            );

            Assert.IsNotNull(method, "C3C EXPECTED RED: missing " + type.Name + "." + methodName);
            return method;
        }

        //Planned pure contract:
        //  MySqlColumnCommentSessionModePolicy.TryParse(string sessionSqlMode, out bool noBackslashEscapes)
        //  This parses a value ALREADY retrieved from the verified Reader connection.
        private static bool ParseVerifiedMode(string sqlMode, out bool noBackslashEscapes)
        {
            noBackslashEscapes = false;

            var type = FindPolicy("JasonQuery.Core.Database.DdlPreview.MySqlColumnCommentSessionModePolicy");
            var method = FindMethod(type, "TryParse", typeof(string), typeof(bool).MakeByRefType());

            Assert.AreEqual(typeof(bool), method.ReturnType);

            var parameters = new object[] { sqlMode, false };
            var parsed = (bool)method.Invoke(null, parameters);

            noBackslashEscapes = (bool)parameters[1];
            return parsed;
        }

        //Planned pure contract:
        //  MySqlColumnCommentLiteralPolicy.TryUnquote(string quotedLiteral, bool noBackslashEscapes, out string value)
        private static bool Unquote(string quotedLiteral, bool noBackslashEscapes, out string value)
        {
            value = string.Empty;

            var type = FindPolicy("JasonQuery.Core.Database.DdlPreview.MySqlColumnCommentLiteralPolicy");
            var method = FindMethod(type, "TryUnquote", typeof(string), typeof(bool), typeof(string).MakeByRefType());

            Assert.AreEqual(typeof(bool), method.ReturnType);

            var parameters = new object[] { quotedLiteral, noBackslashEscapes, null };
            var accepted = (bool)method.Invoke(null, parameters);

            value = parameters[2] as string ?? string.Empty;
            return accepted;
        }

        //Planned pure contract; no SQL is executed by this method:
        //  NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(string sql, bool noBackslashEscapes)
        private static bool ModeAwareSqlShape(string sql, bool noBackslashEscapes)
        {
            var type = typeof(NonOracleSchemaExplorerDdlExecuteGate);
            var method = FindMethod(type, "IsQualifiedMySqlCommentSqlForSessionMode", typeof(string), typeof(bool));

            Assert.AreEqual(typeof(bool), method.ReturnType);
            return (bool)method.Invoke(null, new object[] { sql, noBackslashEscapes });
        }

        //Planned pure contract. The Reader must independently verify preview and live modes:
        //  MySqlVerifiedColumnDefinitionExecutionPolicy.ValidateSessionModeForComment(string previewMode, string liveMode, string comment)
        private static string ValidateModes(string previewMode, string liveMode, string comment)
        {
            var type = typeof(MySqlVerifiedColumnDefinitionExecutionPolicy);
            var method = FindMethod(type, "ValidateSessionModeForComment", typeof(string), typeof(string), typeof(string));

            Assert.AreEqual(typeof(string), method.ReturnType);
            return (string)method.Invoke(null, new object[] { previewMode, liveMode, comment });
        }

        [TestMethod]
        public void C3CG01_ModeNeutralUnicodePreviewRemainsQualified()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request("原文 John's 🚀"), Definition, out var sql
            ));

            Assert.IsTrue(sql.EndsWith(" COMMENT '原文 John''s 🚀';", StringComparison.Ordinal));
        }

        [TestMethod]
        public void C3CG02_UnknownModeCannotGenerateBackslashPreview()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(@"C:\Data\test"), Definition, out var sql
            ));

            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void C3CG03_ExistingModeAwareLiteralQuotingRemainsDistinct()
        {
            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryQuote(@"C:\Data", false, out var normal));
            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryQuote(@"C:\Data", true, out var noEscape));

            Assert.AreEqual("'C:\\\\Data'", normal);
            Assert.AreEqual("'C:\\Data'", noEscape);
        }

        [TestMethod]
        public void C3CG04_UnicodeCommentCannotBecomeSeparateStatementOrRename()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request("安全; DROP TABLE abc; --"), Definition, out var sql
            ));

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, sql
            ));

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, sql + " DROP TABLE `demo`.`comment_test`;"
            ));

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Rename, sql
            ));
        }

        [TestMethod]
        public void C3CG05_OldUnicodeCommentGrammarRemainsQualified()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "int DEFAULT NULL COMMENT 'Mary''s 原文'", out var withoutComment
            ));

            Assert.AreEqual("int DEFAULT NULL", withoutComment);
        }

        [TestMethod]
        public void C3CG06_UnverifiedReaderCannotGrantSessionOrDdlPermission()
        {
            var reader = new MySqlReader();

            Assert.AreEqual(Guid.Empty, reader.CurrentOpenConnectionId);
            Assert.IsFalse(reader.TryReadServerVersionOnVerifiedConnection(Guid.Empty, out var version, out _));
            Assert.AreEqual(string.Empty, version);
        }

        [TestMethod]
        public void C3CR07_CommaDelimitedNoBackslashEscapesIsRecognized()
        {
            Assert.IsTrue(ParseVerifiedMode("STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES", out var enabled));
            Assert.IsTrue(enabled);
        }

        [TestMethod]
        public void C3CR08_ModeTokenCannotMatchSubstringOrSimilarName()
        {
            Assert.IsTrue(ParseVerifiedMode("NO_ENGINE_SUBSTITUTION,PREFIX_NO_BACKSLASH_ESCAPES", out var enabled));
            Assert.IsFalse(enabled);
            Assert.IsTrue(ParseVerifiedMode(string.Empty, out enabled));
            Assert.IsFalse(enabled);
        }

        [TestMethod]
        public void C3CR09_NullSessionModeEvidenceMustBeRejected()
        {
            Assert.IsFalse(ParseVerifiedMode(null, out _));
        }

        [TestMethod]
        public void C3CR10_DefaultModeQuoteBackslashAndNewlineRoundTrip()
        {
            const string raw = "It's C:\\Data\\\r\nnext";

            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryQuote(raw, false, out var literal));
            Assert.IsTrue(Unquote(literal, false, out var restored));
            Assert.AreEqual(raw, restored);
        }

        [TestMethod]
        public void C3CR11_NoBackslashEscapesModeMustKeepLiteralBackslashes()
        {
            const string raw = @"It's C:\Data\test";

            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryQuote(raw, true, out var literal));
            Assert.IsTrue(Unquote(literal, true, out var restored));
            Assert.AreEqual(raw, restored);
        }

        [TestMethod]
        public void C3CR12_UnquoteMustNotConsumeSqlOutsideSingleLiteral()
        {
            Assert.IsFalse(Unquote("'ok'; DROP TABLE `demo`.`comment_test`;", false, out _));
            Assert.IsFalse(Unquote("'unclosed", false, out _));
        }

        [TestMethod]
        public void C3CR13_ReaderSqlModeMustRequireVerifiedOpenConnection()
        {
            //Dedicated Reader-only SELECT; it must never reconnect or dispatch DDL.
            var method = FindMethod
            (
                typeof(MySqlReader),
                "TryReadVerifiedSessionSqlModeOnConnection",
                typeof(Guid), typeof(string).MakeByRefType(), typeof(string).MakeByRefType()
            );

            Assert.AreEqual(typeof(bool), method.ReturnType);

            var parameters = new object[] { Guid.Empty, null, null };

            Assert.IsFalse((bool)method.Invoke(new MySqlReader(), parameters));
            Assert.IsTrue(string.IsNullOrWhiteSpace(parameters[1] as string));
        }

        [TestMethod]
        public void C3CR14_ReaderReceiptMustCarryVerifiedSessionMode()
        {
            //This is evidence, never a reusable authorization token.
            var property = typeof(MySqlVerifiedColumnDefinitionSnapshot).GetProperty
            (
                "VerifiedSessionSqlMode",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.IsNotNull(property, "C3C EXPECTED RED: Reader receipt lacks verified SESSION.sql_mode evidence.");
            Assert.AreEqual(typeof(string), property.PropertyType);
            Assert.IsFalse(property.CanWrite);
        }

        [TestMethod]
        public void C3CR15_OldBackslashCommentNeedsModeAwareMetadataGrammar()
        {
            var method = FindMethod
            (
                typeof(MySqlCompleteColumnDefinitionSqlBuilder),
                "TryQualifyCompleteDefinition",
                typeof(string), typeof(bool), typeof(string).MakeByRefType()
            );

            Assert.AreEqual(typeof(bool), method.ReturnType);

            var parameters = new object[]
            {
                @"varchar(32) NOT NULL DEFAULT 'a' COMMENT 'C:\\Data'", false, null
            };

            Assert.IsTrue((bool)method.Invoke(null, parameters));
            Assert.AreEqual("varchar(32) NOT NULL DEFAULT 'a'", parameters[2] as string);
        }

        [TestMethod]
        public void C3CR16_ModeAwareShapeMustNotTreatBackslashLiteralAsSql()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(@"C:\Data\test"), Definition, false, out var sql
            ));

            Assert.IsTrue(ModeAwareSqlShape(sql, false));
            Assert.IsFalse(ModeAwareSqlShape(sql + " DROP TABLE `demo`.`comment_test`;", false));
        }

        [TestMethod]
        public void C3CR17_ChangedModeMustDenyModeSensitiveExecute()
        {
            var reason = ValidateModes("STRICT_TRANS_TABLES", "STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES", @"C:\Data");

            Assert.IsFalse(string.IsNullOrEmpty(reason));
            Assert.AreEqual(string.Empty, ValidateModes("STRICT_TRANS_TABLES", "STRICT_TRANS_TABLES", @"C:\Data"));
        }
    }
}
