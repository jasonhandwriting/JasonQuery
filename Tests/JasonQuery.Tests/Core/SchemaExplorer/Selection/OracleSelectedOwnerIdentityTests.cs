using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.SchemaExplorer.Selection
{
    [TestClass]
    public sealed class OracleSelectedOwnerIdentityTests
    {
        private static SchemaExplorerSelectionResolveRequest Request(DataSourceType provider)
        {
            return new SchemaExplorerSelectionResolveRequest
            {
                SourceType = provider,
                DisplayRowIndex = 0,
                IsExternalSelection = true,
                CurrentSchemaType = "Tables",
                CurrentSchemaName = "AABBCC",
                CurrentSchemaNode = "unverified"
            };
        }

        private static void SetProperty(object target, string name, object value)
        {
            var property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(property, "Missing owner identity contract: " + name);
            property.SetValue(target, value);
        }

        private static T GetProperty<T>(object target, string name)
        {
            var property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(property, "Missing owner identity contract: " + name);
            return (T)property.GetValue(target);
        }

        private static SchemaExplorerSelectionInfo ResolveQualified(string sessionUser, string metadataOwner)
        {
            var request = Request(DataSourceType.Oracle);

            SetProperty(request, "RequireOracleOwnerIdentity", true);
            SetProperty(request, "OracleVerifiedSessionUser", sessionUser);
            SetProperty(request, "OracleMetadataOwner", metadataOwner);

            return SchemaExplorerSelectionResolver.Resolve(request, null);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G01_LegacyExternalOracleSelection_RemainsUnchangedWithoutOptIn()
        {
            var result = SchemaExplorerSelectionResolver.Resolve(Request(DataSourceType.Oracle), null);

            Assert.AreEqual(SchemaExplorerSelectionKind.ExternalObject, result.Kind);
            Assert.AreEqual("unverified", result.SchemaNode);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G02_OtherProviders_RemainingLegacySelectionUnchanged()
        {
            var result = SchemaExplorerSelectionResolver.Resolve(Request(DataSourceType.PostgreSql), null);

            Assert.AreEqual(SchemaExplorerSelectionKind.ExternalObject, result.Kind);
            Assert.AreEqual("unverified", result.SchemaNode);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G03_ExactSysOwner_IsQualified()
        {
            var result = ResolveQualified("SYS", "SYS");

            Assert.AreEqual(SchemaExplorerSelectionKind.ExternalObject, result.Kind);
            Assert.AreEqual("SYS", GetProperty<string>(result, "OracleSelectedOwner"));
            Assert.IsTrue(GetProperty<bool>(result, "HasQualifiedOracleSelectedOwner"));
            Assert.AreEqual("SYS", result.SchemaNode);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G04_QuotedMixedCaseOwner_PreservedExactly()
        {
            var result = ResolveQualified("MiXeD", "MiXeD");

            Assert.AreEqual("MiXeD", GetProperty<string>(result, "OracleSelectedOwner"));
            Assert.AreEqual("MiXeD", result.SchemaNode);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G05_MixedCaseMismatch_FailsClosed()
        {
            var result = ResolveQualified("MiXeD", "MIXED");

            Assert.AreEqual(SchemaExplorerSelectionKind.Invalid, result.Kind);
            Assert.IsFalse(result.CanDisplayObject);
            Assert.AreEqual(string.Empty, result.SchemaNode);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G06_MissingSessionUser_FailsClosed()
        {
            var result = ResolveQualified(string.Empty, "SYS");

            Assert.AreEqual(SchemaExplorerSelectionKind.Invalid, result.Kind);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G07_MissingMetadataOwner_FailsClosed()
        {
            var result = ResolveQualified("SYS", string.Empty);

            Assert.AreEqual(SchemaExplorerSelectionKind.Invalid, result.Kind);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G08_StaleOwnerAfterConnectionChange_FailsClosed()
        {
            var result = ResolveQualified("SYSTEM", "SYS");

            Assert.AreEqual(SchemaExplorerSelectionKind.Invalid, result.Kind);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2")]
        public void F02B2_G09_CaseFoldedLoginValue_NotAcceptedAsExactSessionIdentity()
        {
            var result = ResolveQualified("sys", "SYS");

            Assert.AreEqual(SchemaExplorerSelectionKind.Invalid, result.Kind);
        }
    }
}
