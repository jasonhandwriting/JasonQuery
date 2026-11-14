using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-U3-C4: version-dependent MySQL Column DDL selection, without executing database operations.
    [TestClass]
    [TestCategory("F7U3C4")]
    public sealed class MySqlColumnDdlVersionGoldenTests
    {
        private const string ModernRename = "RenameColumn";
        private const string LegacyRename = "ChangeRequiresVerifiedDefinition";
        private const string Unknown = "Unknown";

        private static Type RequireVersionPolicy()
        {
            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.MySqlColumnDdlVersionPolicy", false
            );

            Assert.IsNotNull(type, "The MySQL server-version policy is not implemented yet (expected Golden Red).");
            return type;
        }

        private static MethodInfo RequireVersionSelector()
        {
            var type = RequireVersionPolicy();

            var method = type.GetMethod
            (
                "SelectRenameSyntax",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null
            );

            Assert.IsNotNull(method, "Version-dependent Rename syntax selection is missing (expected Golden Red).");
            Assert.IsTrue(method.ReturnType.IsEnum, "SelectRenameSyntax should return a closed enum, not a free-form SQL fragment.");

            return method;
        }

        private static string SelectRenameSyntax(string serverVersion)
        {
            return RequireVersionSelector().Invoke(null, new object[] { serverVersion }).ToString();
        }

        private static ColumnDdlRequest CreateRequest(ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = operation,
                SchemaDatabase = "sakila",
                SchemaName = string.Empty,
                TableName = "jq_column_ddl_test",
                ColumnName = "old_name",
                NewColumnName = "new_name",
                Comment = "test comment",
                ColumnType = string.Empty
            };
        }

        private static MethodInfo RequireModernRenameBuilder()
        {
            var method = typeof(ColumnDdlSqlBuilder).GetMethod
            (
                "BuildMySqlRenameColumnSql",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(ColumnDdlRequest) },
                null
            );

            Assert.IsNotNull(method, "A definition-free MySQL RENAME COLUMN SQL builder is required (expected Golden Red).");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return method;
        }

        [TestMethod]
        public void M401_MySqlRenameVersionPolicyMustExist()
        {
            Assert.IsNotNull(RequireVersionSelector());
        }

        [TestMethod]
        public void M402_MySql56And57RequireVerifiedDefinitionForChangeColumn()
        {
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("5.6.51"));
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("5.7.44-log"));
        }

        [TestMethod]
        public void M403_MySql800Through802DoNotSupportRenameColumn()
        {
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("8.0.0"));
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("8.0.2"));
        }

        [TestMethod]
        public void M404_MySql803IsTheFirstRenameColumnVersion()
        {
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.3"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.4"));
        }

        [TestMethod]
        public void M405_MySql80GeneralAvailabilityReleasesSupportRenameColumn()
        {
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.11"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.36"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.46"));
        }

        [TestMethod]
        public void M406_MySql84And9SeriesSupportRenameColumn()
        {
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.4.0"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("9.0.1"));
        }

        [TestMethod]
        public void M407_CommonMySqlSuffixesDoNotChangeNumericVersion()
        {
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.36-log"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.35-27"));
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("5.7.44-commercial"));
        }

        [TestMethod]
        public void M408_MariaDbMustNeverBeMistakenForMySqlByCompatibilityPrefix()
        {
            Assert.AreEqual(Unknown, SelectRenameSyntax("5.5.5-10.11.2-MariaDB"));
            Assert.AreEqual(Unknown, SelectRenameSyntax("10.5.2-MariaDB"));
            Assert.AreEqual(Unknown, SelectRenameSyntax("8.0.36-MariaDB"));
        }

        [TestMethod]
        public void M409_UnknownOrMalformedVersionMustFailClosed()
        {
            Assert.AreEqual(Unknown, SelectRenameSyntax(null));
            Assert.AreEqual(Unknown, SelectRenameSyntax(string.Empty));
            Assert.AreEqual(Unknown, SelectRenameSyntax("unknown"));
            Assert.AreEqual(Unknown, SelectRenameSyntax("8.0"));
            Assert.AreEqual(Unknown, SelectRenameSyntax("8.0.36; DROP TABLE t"));
        }

        [TestMethod]
        public void M410_MySqlVersionComparisonMustUseNumbersNotTextOrder()
        {
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("8.0.2"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("8.0.11"));
            Assert.AreEqual(LegacyRename, SelectRenameSyntax("5.7.44"));
        }

        [TestMethod]
        public void M411_MySqlReaderMustReadVersionOnTheVerifiedPhysicalConnection()
        {
            var method = typeof(MySqlReader).GetMethod
            (
                "TryReadServerVersionOnVerifiedConnection",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(Guid), typeof(string).MakeByRefType(), typeof(string).MakeByRefType() },
                null
            );

            Assert.IsNotNull(method, "Reader needs a same-connection VERSION() lookup; never reconnect or probe using DDL (expected Golden Red).");
            Assert.AreEqual(typeof(bool), method.ReturnType);
            Assert.IsTrue(method.IsAssembly, "Keep the verified-connection capability lookup internal.");
        }

        [TestMethod]
        public void M412_ModernRenameBuilderDoesNotRestateColumnAttributes()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);
            var sql = (string)RequireModernRenameBuilder().Invoke(null, new object[] { request });

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`jq_column_ddl_test` RENAME COLUMN `old_name` TO `new_name`;",
                sql
            );
            Assert.IsFalse(sql.Contains("CHANGE COLUMN"));
        }

        [TestMethod]
        public void M413_PreexistingLegacyRenameBuilderStillRequiresColumnType()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void M414_PreexistingMySqlCommentBuilderStillRequiresColumnType()
        {
            var request = CreateRequest(ColumnDdlOperation.Comment);

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        public void M415_MySqlCommentMustRejectUnverifiedCompleteDefinition()
        {
            Assert.AreEqual
            (
                "F7U3C2B_MYSQL_DEFINITION_UNVERIFIED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateMySqlColumnDefinitionSafety
                (
                    ColumnDdlOperation.Comment, false, "int NOT NULL", "int NOT NULL"
                )
            );
        }

        [TestMethod]
        public void M416_LegacyChangeColumnSqlIsNotSilentlyRemoved()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);

            request.ColumnType = "int NOT NULL";

            Assert.AreEqual
            (
                "ALTER TABLE `sakila`.`jq_column_ddl_test` CHANGE COLUMN `old_name` `new_name` int NOT NULL;",
                ColumnDdlSqlBuilder.Build(request)
            );
        }

        [TestMethod]
        public void M417_FutureMySql10SeriesUsesRenameColumnWithoutMajorVersionWhitelist()
        {
            Assert.AreEqual(ModernRename, SelectRenameSyntax("10.0.0"));
            Assert.AreEqual(ModernRename, SelectRenameSyntax("10.6.1-log"));
        }

        [TestMethod]
        public void M418_MariaDb10SeriesMustNotBeTreatedAsFutureMySql()
        {
            Assert.AreEqual(Unknown, SelectRenameSyntax("10.6.0-MariaDB"));
            Assert.AreEqual(Unknown, SelectRenameSyntax("5.5.5-10.11.2-MariaDB"));
        }
    }
}
