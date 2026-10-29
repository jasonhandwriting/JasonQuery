using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class OracleDdlTargetIdentityGoldenTests
    {
        private static TableDdlRequest Table(TableDdlOperation operation)
        {
            return new TableDdlRequest
            {
                DataSourceType = DataSourceType.Oracle,
                Operation = operation,
                SchemaName = "legacy",
                TableName = "AABBCC",
                NewTableName = "NEXT",
                Comment = "test"
            };
        }

        private static ColumnDdlRequest Column(ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.Oracle,
                Operation = operation,
                SchemaName = "legacy",
                TableName = "AABBCC",
                ColumnName = "OLD",
                NewColumnName = "NEXT",
                Comment = "test",
                ColumnDefinition = Definition()
            };
        }

        private static ColumnDefinition Definition()
        {
            return new ColumnDefinition
            {
                ColumnName = "AA",
                TypeKey = "VARCHAR2",
                Parameter1 = "50",
                NullAllowed = true
            };
        }

        //Reflection lets Golden compile BEFORE the new request contract exists.
        private static void Set(object value, string property, object next)
        {
            var prop = value.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(prop, "Missing F02-B3-A contract property: " + property);
            prop.SetValue(value, next);
        }

        private static object Identity(string owner = "SYS")
        {
            var type = typeof(TableDdlRequest).Assembly.GetType("JasonQuery.Core.Database.DdlPreview.OracleDdlTargetIdentity");

            Assert.IsNotNull(type, "Missing F02-B3-A OracleDdlTargetIdentity");

            var identity = Activator.CreateInstance(type, true);
            var id = Guid.Parse("11111111-2222-3333-4444-555555555555");

            Set(identity, "VerifiedSessionUser", owner);
            Set(identity, "MetadataOwner", owner);
            Set(identity, "SelectedOwner", owner);
            Set(identity, "VerifiedConnectionId", id);
            Set(identity, "SelectionConnectionId", id);
            Set(identity, "CurrentConnectionId", id);
            Set(identity, "HasQualifiedSelectedOwner", true);

            return identity;
        }

        private static void Enable(object request, object identity)
        {
            Set(request, "RequireOracleDdlTargetIdentity", true);
            Set(request, "OracleTargetIdentity", identity);
        }

        private static string ExpectedOwner(string owner)
        {
            return "\"" + owner.Replace("\"", "\"\"") + "\"";
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G01_LegacyOracleRequestRemainsUnchanged()
        {
            var request = Table(TableDdlOperation.Drop);

            Assert.AreEqual("DROP TABLE \"legacy\".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G02_NonOracleLegacyRequestRemainsUnchanged()
        {
            var request = Table(TableDdlOperation.Drop);

            request.DataSourceType = DataSourceType.PostgreSql;
            request.SchemaName = "public";
            Assert.AreEqual("DROP TABLE \"public\".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G03_TrustedSysTableDropUsesExactOwner()
        {
            var request = Table(TableDdlOperation.Drop);

            Enable(request, Identity());
            Assert.AreEqual("DROP TABLE \"SYS\".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G04_QuotedMixedCaseTableRenamePreservesOwner()
        {
            var request = Table(TableDdlOperation.Rename);

            Enable(request, Identity("MiXeD"));
            Assert.AreEqual("ALTER TABLE \"MiXeD\".\"AABBCC\" RENAME TO \"NEXT\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G05_TrustedTableTruncateUsesExactOwner()
        {
            var request = Table(TableDdlOperation.Truncate);

            Enable(request, Identity());
            Assert.AreEqual("TRUNCATE TABLE \"SYS\".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G06_TrustedColumnAddDelegationPreservesOwner()
        {
            var request = Column(ColumnDdlOperation.Add);

            Enable(request, Identity());
            Assert.AreEqual("ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" VARCHAR2(50) NULL;", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G07_TrustedColumnDropUsesExactOwner()
        {
            var request = Column(ColumnDdlOperation.Drop);

            Enable(request, Identity());
            Assert.AreEqual("ALTER TABLE \"SYS\".\"AABBCC\" DROP COLUMN \"OLD\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G08_MixedCaseColumnRenamePreservesOwner()
        {
            var request = Column(ColumnDdlOperation.Rename);

            Enable(request, Identity("MiXeD"));
            Assert.AreEqual("ALTER TABLE \"MiXeD\".\"AABBCC\" RENAME COLUMN \"OLD\" TO \"NEXT\";", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G09_TrustedTableCommentUsesExactOwnerNotLegacySchema()
        {
            var request = Table(TableDdlOperation.Comment);

            Enable(request, Identity("MiXeD"));
            Assert.AreEqual("COMMENT ON TABLE \"MiXeD\".\"AABBCC\" IS 'test';", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G10_TrustedColumnCommentUsesExactOwnerNotLegacySchema()
        {
            var request = Column(ColumnDdlOperation.Comment);

            Enable(request, Identity("MiXeD"));
            Assert.AreEqual("COMMENT ON COLUMN \"MiXeD\".\"AABBCC\".\"OLD\" IS 'test';", ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G11_MissingVerifiedSessionUserFailsClosed()
        {
            var request = Table(TableDdlOperation.Drop);
            var identity = Identity();

            Set(identity, "VerifiedSessionUser", string.Empty);
            Enable(request, identity);
            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G12_CaseMismatchFailsClosed()
        {
            var request = Column(ColumnDdlOperation.Drop);
            var identity = Identity("MiXeD");

            Set(identity, "SelectedOwner", "MIXED");
            Enable(request, identity);
            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G13_MissingConnectionIdentityFailsClosed()
        {
            var request = Table(TableDdlOperation.Truncate);
            var identity = Identity();

            Set(identity, "VerifiedConnectionId", Guid.Empty);
            Enable(request, identity);
            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G14_StaleConnectionIdentityFailsClosed()
        {
            var request = Column(ColumnDdlOperation.Rename);
            var identity = Identity();

            Set(identity, "CurrentConnectionId", Guid.NewGuid());
            Enable(request, identity);
            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G15_UnqualifiedSelectionFailsClosed()
        {
            var request = Table(TableDdlOperation.Drop);
            var identity = Identity();

            Set(identity, "HasQualifiedSelectedOwner", false);
            Enable(request, identity);
            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G16_RawMetadataOwnerDoubleQuoteIsEscapedExactly()
        {
            var request = Table(TableDdlOperation.Drop);
            var owner = "X\"Y";

            Enable(request, Identity(owner));
            Assert.AreEqual("DROP TABLE " + ExpectedOwner(owner) + ".\"AABBCC\";", TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G17_ControlCharacterOwnerFailsClosed()
        {
            var request = Column(ColumnDdlOperation.Comment);

            Enable(request, Identity("BAD\nOWNER"));
            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G18_RequiredIdentityMissingFailsClosed()
        {
            var request = Table(TableDdlOperation.Comment);

            Enable(request, null);
            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod, TestCategory("F02B3A")]
        public void G19_DirectColumnAddBuilderFailsClosedOnStaleIdentity()
        {
            var request = new ColumnAddRequest
            {
                DataSourceType = DataSourceType.Oracle,
                SchemaName = "legacy",
                TableName = "AABBCC",
                Column = Definition()
            };

            var identity = Identity();

            Set(identity, "SelectionConnectionId", Guid.NewGuid());
            Enable(request, identity);
            Assert.IsFalse(ColumnAddSqlBuilder.Build(request).Succeeded);
        }
    }
}
