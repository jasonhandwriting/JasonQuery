using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-S1-C3C-C1: verified MySQL COMMENT Preview/Execute intent, Golden-first.
    //No connection is opened, no DDL is executed, and no Git operation is performed.
    //Pure parsing or matching never grants Execute permission.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlColumnCommentExecuteSafetyGoldenTests
    {
        private const string Definition = "varchar(32) NOT NULL DEFAULT 'a' COMMENT 'old'";
        private const string DatabaseName = "demo";
        private const string TableName = "comment_test";
        private const string ColumnName = "note";
        private const long ServerConnectionId = 381;
        private static readonly Guid ConnectionId = new Guid("C74FEA83-DF77-4B38-9D34-775BF66A3A86");

        private static ColumnDdlRequest Request(string comment)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = DatabaseName,
                TableName = TableName,
                ColumnName = ColumnName,
                Comment = comment
            };
        }

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(string mode)
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                ConnectionId, DatabaseName, TableName, ColumnName,
                "8.0.34", Definition, ServerConnectionId, "verified-attributes", mode
            );
        }

        private static MySqlVerifiedColumnDefinitionSnapshot LegacyReceipt()
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                ConnectionId, DatabaseName, TableName, ColumnName,
                "8.0.34", Definition, ServerConnectionId, "verified-attributes"
            );
        }

        private static MethodInfo FindMethod(Type type, string name, params Type[] parameterTypes)
        {
            var method = type.GetMethod
            (
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
                null,
                parameterTypes,
                null
            );

            Assert.IsNotNull(method, "C3C-C1 EXPECTED RED: missing " + type.Name + "." + name);
            return method;
        }

        private static bool ExtractCanonical(string sql, string database, string table, string column,
                                             string verifiedDefinition, bool noBackslashEscapes, out string comment)
        {
            comment = string.Empty;

            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.MySqlColumnCommentSqlIntentPolicy", false
            );

            Assert.IsNotNull(type, "C3C-C1 EXPECTED RED: missing pure canonical COMMENT intent parser.");

            var method = FindMethod
            (
                type, "TryExtractCanonicalComment",
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(bool), typeof(string).MakeByRefType()
            );

            Assert.AreEqual(typeof(bool), method.ReturnType);

            var arguments = new object[] { sql, database, table, column, verifiedDefinition, noBackslashEscapes, null };
            var accepted = (bool)method.Invoke(null, arguments);

            comment = arguments[6] as string ?? string.Empty;
            return accepted;
        }

        private static bool BuildVerifiedPreview(ColumnDdlRequest request, MySqlVerifiedColumnDefinitionSnapshot receipt, out string sql)
        {
            sql = string.Empty;

            var method = FindMethod
            (
                typeof(EditColumnForm), "TryBuildMySqlVerifiedCommentPreview",
                typeof(ColumnDdlRequest), typeof(MySqlVerifiedColumnDefinitionSnapshot), typeof(string).MakeByRefType()
            );

            Assert.AreEqual(typeof(bool), method.ReturnType);
            Assert.IsTrue(method.IsStatic);

            var arguments = new object[] { request, receipt, null };
            var accepted = (bool)method.Invoke(null, arguments);

            sql = arguments[2] as string ?? string.Empty;
            return accepted;
        }

        [TestMethod]
        public void C3CCG01_EmptySessionModeIsAValidModeValue()
        {
            Assert.IsTrue(MySqlColumnCommentSessionModePolicy.TryParse(string.Empty, out var noBackslashEscapes));
            Assert.IsFalse(noBackslashEscapes);
        }

        [TestMethod]
        public void C3CCG02_UnicodeAndApostropheRoundTripInOrdinaryMode()
        {
            const string original = "繁體中文 🚀 John's comment";

            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryQuote(original, false, out var literal));
            Assert.IsTrue(MySqlColumnCommentLiteralPolicy.TryUnquote(literal, false, out var restored));
            Assert.AreEqual(original, restored);
        }

        [TestMethod]
        public void C3CCG03_ModeNeutralUnicodePreviewRemainsAvailable()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request("原文 John's"), Definition, out var sql
            ));

            Assert.IsTrue(sql.EndsWith(" COMMENT '原文 John''s';", StringComparison.Ordinal));
        }

        [TestMethod]
        public void C3CCG04_UnknownModeCannotCreateBackslashPreview()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(@"C:\Data\file"), Definition, out var sql
            ));

            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void C3CCG05_ExistingModeAwareShapeAcceptsCanonicalBackslashComment()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(@"C:\Data\file"), Definition, false, out var sql
            ));

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(sql, false));

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode
            (
                sql + " DROP TABLE `demo`.`comment_test`;", false
            ));
        }

        [TestMethod]
        public void C3CCG06_UnverifiedReaderCannotReadSessionMode()
        {
            var reader = new MySqlReader();

            Assert.AreEqual(Guid.Empty, reader.CurrentOpenConnectionId);
            Assert.IsFalse(reader.TryReadVerifiedSessionSqlModeOnConnection(Guid.Empty, out var mode, out _));
            Assert.IsTrue(string.IsNullOrEmpty(mode));
        }

        [TestMethod]
        public void C3CCG07_PendingTransactionCannotReachAnyDdlDelegate()
        {
            var calls = 0;

            var reason = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql, true, true,
                () => { calls++; return string.Empty; },
                () => { calls++; return string.Empty; },
                () => { calls++; return ConnectionId; },
                _ => { calls++; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", reason);
            Assert.AreEqual(0, calls);
        }

        //A valid, verified empty SESSION.sql_mode must be distinguishable from a legacy receipt that does not contain any verified session-mode evidence.
        [TestMethod]
        public void C3CCR08_ValidEmptyModeAndMissingModeEvidenceMustRemainDistinct()
        {
            var property = typeof(MySqlVerifiedColumnDefinitionSnapshot).GetProperty
            (
                "HasVerifiedSessionSqlMode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            Assert.IsNotNull(property, "C3C-C1 EXPECTED RED: mode provenance is not represented.");
            Assert.AreEqual(typeof(bool), property.PropertyType);
            Assert.IsFalse(property.CanWrite);
            Assert.IsTrue((bool)property.GetValue(Receipt(string.Empty)));
            Assert.IsFalse((bool)property.GetValue(LegacyReceipt()));
            Assert.IsFalse((bool)property.GetValue(Receipt(null)));
        }

        [TestMethod]
        public void C3CCR09_ModeSensitiveExecuteMustRejectMissingModeEvidencePrecisely()
        {
            var request = Request(@"C:\Data\file");

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                request, Definition, false, out var sql
            ));

            Assert.AreEqual
            (
                "F7S1_C3C_SESSION_MODE_UNVERIFIED",
                MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                (
                    ColumnDdlOperation.Comment, LegacyReceipt(), LegacyReceipt(), request, sql
                )
            );
        }

        [TestMethod]
        public void C3CCR10_ModeChangeMustBeReportedBeforeSqlComparison()
        {
            var request = Request(@"C:\Data\file");

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                request, Definition, false, out var sql
            ));

            Assert.AreEqual
            (
                "F7S1_C3C_SESSION_MODE_CHANGED",
                MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                (
                    ColumnDdlOperation.Comment, Receipt("STRICT_TRANS_TABLES"),
                    Receipt("STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES"), request, sql
                )
            );
        }

        [TestMethod]
        public void C3CCR11_MatchingVerifiedModeMustAllowCanonicalSqlComparison()
        {
            var request = Request(@"C:\Data\file");

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                request, Definition, false, out var sql
            ));

            Assert.AreEqual
            (
                string.Empty,
                MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                (
                    ColumnDdlOperation.Comment, Receipt("STRICT_TRANS_TABLES"),
                    Receipt("STRICT_TRANS_TABLES"), request, sql
                )
            );
        }

        [TestMethod]
        public void C3CCR12_ModeAwareBuilderMustAcceptVerifiedOldBackslashComment()
        {
            const string oldDefinition = @"int NOT NULL DEFAULT 7 COMMENT 'C:\\Data'";

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request("updated"), oldDefinition, false, out var sql
            ));

            Assert.IsTrue(sql.Contains("int NOT NULL DEFAULT 7 COMMENT 'updated';"));
        }

        [TestMethod]
        public void C3CCR13_ModeAwareMetadataProofMustPreserveDecodedOldComment()
        {
            var method = FindMethod
            (
                typeof(MySqlCommonColumnDefinitionProofPolicy), "Validate",
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(bool), typeof(bool), typeof(bool)
            );

            var args = new object[]
            {
                @"int NOT NULL DEFAULT 7 COMMENT 'C:\\Data'", "int", "NO", "7",
                @"C:\Data", string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty,
                false, false, false
            };

            Assert.AreEqual(string.Empty, (string)method.Invoke(null, args));
        }

        [TestMethod]
        public void C3CCR14_CanonicalIntentParserMustRecoverOriginalTextExactly()
        {
            const string original = "It's C:\\Data; DROP TABLE users; -- 🚀";

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(original), Definition, false, out var sql
            ));

            Assert.IsTrue(ExtractCanonical
            (
                sql, DatabaseName, TableName, ColumnName, Definition, false, out var restored
            ));

            Assert.AreEqual(original, restored);
        }

        [TestMethod]
        public void C3CCR15_CanonicalIntentParserMustRejectASecondStatement()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request("safe"), Definition, false, out var sql
            ));

            Assert.IsFalse(ExtractCanonical
            (
                sql + " DROP TABLE `demo`.`comment_test`;",
                DatabaseName, TableName, ColumnName, Definition, false, out _
            ));
        }

        [TestMethod]
        public void C3CCR16_CanonicalIntentParserMustRejectAChangedTarget()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request("safe"), Definition, false, out var sql
            ));

            Assert.IsFalse(ExtractCanonical
            (
                sql, DatabaseName, TableName, "different_column", Definition, false, out _
            ));
        }

        [TestMethod]
        public void C3CCR17_FormPreviewMustUseVerifiedModeAndRejectLegacyModeEvidence()
        {
            var request = Request(@"C:\Data\file");

            Assert.IsTrue(BuildVerifiedPreview(request, Receipt("STRICT_TRANS_TABLES"), out var sql));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(sql, false));
            Assert.IsFalse(BuildVerifiedPreview(request, LegacyReceipt(), out _));
        }
    }
}
