using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-U3-C3: PostgreSQL truncation safety for new guided identifiers only.
    //All tests are pure/reflection checks: no database connection, DDL or transaction change.
    [TestClass]
    [TestCategory("F7U3C3")]
    public sealed class PostgreSqlIdentifierTruncationGoldenTests
    {
        private const string TooLong = "F7U3C_PG_IDENTIFIER_WOULD_TRUNCATE";
        private const string UnknownLimit = "F7U3C_PG_IDENTIFIER_LIMIT_UNVERIFIED";
        private const string InvalidName = "F7U3C_PG_IDENTIFIER_INVALID";

        private static MethodInfo RequirePurePolicy()
        {
            var method = typeof(NonOracleSchemaExplorerDdlExecuteGate).GetMethod
            (
                "ValidatePostgreSqlNewIdentifierLength",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(int) },
                null
            );

            Assert.IsNotNull(method, "The PostgreSQL new-name byte policy has not been implemented yet (expected Golden Red).");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return method;
        }

        private static string Validate(string rawNewName, int maxIdentifierBytes)
        {
            return (string)RequirePurePolicy().Invoke(null, new object[] { rawNewName, maxIdentifierBytes });
        }

        [TestMethod]
        public void B801_PureNewIdentifierPolicyMustExist()
        {
            Assert.IsNotNull(RequirePurePolicy());
        }

        [TestMethod]
        public void B802_Ascii63AllowedAndAscii64Rejected()
        {
            Assert.AreEqual(string.Empty, Validate(new string('a', 63), 63));
            Assert.AreEqual(TooLong, Validate(new string('a', 64), 63));
        }

        [TestMethod]
        public void B803_Cjk21AllowedAndCjk22RejectedByUtf8Bytes()
        {
            Assert.AreEqual(string.Empty, Validate(new string('中', 21), 63));
            Assert.AreEqual(TooLong, Validate(new string('中', 22), 63));
        }

        [TestMethod]
        public void B804_MixedAsciiAndCjkUseTotalUtf8Bytes()
        {
            Assert.AreEqual(string.Empty, Validate(new string('a', 60) + "中", 63));
            Assert.AreEqual(TooLong, Validate(new string('a', 61) + "中", 63));
        }

        [TestMethod]
        public void B805_ServerSuppliedLimitOverridesTheDefault63()
        {
            var identifier = new string('a', 80);

            Assert.AreEqual(TooLong, Validate(identifier, 63));
            Assert.AreEqual(string.Empty, Validate(identifier, 127));
        }

        [TestMethod]
        public void B806_DelimitersAndEscapedQuotesAreNotCountedAsIdentifierBytes()
        {
            Assert.AreEqual(string.Empty, Validate("\"AbC\"", 3));
            Assert.AreEqual(string.Empty, Validate("\"a\"\"b\"", 3));
            Assert.AreEqual(TooLong, Validate("\"a\"\"b\"", 2));
        }

        [TestMethod]
        public void B807_UnverifiedServerLimitMustFailClosed()
        {
            Assert.AreEqual(UnknownLimit, Validate("new_column", 0));
            Assert.AreEqual(UnknownLimit, Validate("new_column", -1));
        }

        [TestMethod]
        public void B808_InvalidOrBlankNewIdentifierMustFailClosed()
        {
            Assert.AreEqual(InvalidName, Validate(string.Empty, 63));
            Assert.AreEqual(InvalidName, Validate("   ", 63));
            Assert.AreEqual(InvalidName, Validate("a;b", 63));
        }

        [TestMethod]
        public void B809_ReaderMustExposeSameConnectionServerLimitCheck()
        {
            var method = typeof(PostgreSqlReader).GetMethod
            (
                "ValidateNewIdentifierLengthOnVerifiedConnection",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(Guid), typeof(string) },
                null
            );

            Assert.IsNotNull(method, "Reader must read max_identifier_length on the already-verified connection; not a hard-coded 63 (expected Golden Red).");
            Assert.AreEqual(typeof(string), method.ReturnType);
            Assert.IsTrue(method.IsAssembly, "Do not expose a privileged Reader validation entry as public API.");
        }

        [TestMethod]
        public void B810_BadDelimitedNameCannotBypassTheByteCheck()
        {
            Assert.AreEqual(InvalidName, Validate("\"unterminated", 63));
            Assert.AreEqual(InvalidName, Validate("\"x\";DROP TABLE t", 63));
        }

        [TestMethod]
        public void B811_ExistingPostgreSqlColumnRenamePreservesCase()
        {
            var request = new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.PostgreSql,
                Operation = ColumnDdlOperation.Rename,
                SchemaDatabase = "postgres",
                SchemaName = "public",
                TableName = "aa",
                ColumnName = "old",
                NewColumnName = "AaTest"
            };

            Assert.AreEqual
            (
                "ALTER TABLE \"public\".\"aa\" RENAME COLUMN \"old\" TO \"AaTest\";",
                ColumnDdlSqlBuilder.Build(request)
            );
        }

        [TestMethod]
        public void B812_ExistingIdentifierDecoderPreservesDelimitedCase()
        {
            var name = DdlIdentifierQuotingPolicy.GetNewIdentifierValue
            (
                DataSourceType.PostgreSql, "\"MixedCase\"", DdlNewIdentifierCaseMode.PreserveInput
            );

            Assert.AreEqual("MixedCase", name);
        }
    }
}
