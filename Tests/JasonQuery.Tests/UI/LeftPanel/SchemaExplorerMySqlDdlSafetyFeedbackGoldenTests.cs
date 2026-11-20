using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //MySQL Column DDL user feedback contract. Pure messages and repository resources only.
    //No database connection, DDL dispatch, schema modification or Execute authorization.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlDdlSafetyFeedbackGoldenTests
    {
        private static string Message(string reasonKey)
        {
            return MySqlColumnDdlPreviewFeedbackPolicy.GetDefaultMessage(reasonKey);
        }

        private static bool Contains(string text, string expected)
        {
            return (text ?? string.Empty).IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetLocalizationPath(string fileName)
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null)
            {
                var path = Path.Combine(directory.FullName, "JasonQuery", "localization", fileName);

                if (File.Exists(path))
                {
                    return path;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Localization file was not found: " + fileName);
            return string.Empty;
        }

        //Expected RED: replace generic reasons with an uncertainty-qualified risk and next action.
        [TestMethod]
        public void A01_UnsupportedDefinitionExplainsPossibleRiskAndAction()
        {
            var message = Message("MySqlColumnDdlUnsupportedDefinition");

            Assert.IsTrue(Contains(message, "may"), "Do not state that an unproven column attribute certainly exists.");
            Assert.IsTrue(Contains(message, "NULL") && Contains(message, "DEFAULT"), "Name common examples as possible risks.");
            Assert.IsTrue(Contains(message, "SHOW CREATE TABLE"), "Give a concrete verification action.");
        }

        [TestMethod]
        public void A02_MetadataUnverifiedProvidesSafeNextSteps()
        {
            var message = Message("MySqlColumnDdlMetadataUnverified");

            Assert.IsTrue(Contains(message, "SHOW CREATE TABLE"));
            Assert.IsTrue(Contains(message, "Query Editor"));
        }

        [TestMethod]
        public void A03_UnverifiedVersionOffersRecoveryWithoutGuessingSyntax()
        {
            var message = Message("MySqlColumnDdlVersionUnverified");

            Assert.IsTrue(Contains(message, "version"));
            Assert.IsTrue(Contains(message, "reconnect") || Contains(message, "reopen"));
        }

        [TestMethod]
        public void A04_GenericUnverifiedDefinitionOffersNextAction()
        {
            var message = Message("MySqlColumnDdlDefinitionUnverified");

            Assert.IsTrue(Contains(message, "SHOW CREATE TABLE"));
        }

        [TestMethod]
        public void A05_PendingTransactionExplainsManualResolutionBeforeRetry()
        {
            var message = Message("MySqlColumnDdlPendingTransaction");

            Assert.IsTrue(Contains(message, "Commit") && Contains(message, "Rollback"));
            Assert.IsTrue(Contains(message, "reopen"));
        }

        //Expected GREEN: the existing reason routing and permission boundary must not regress.
        [TestMethod]
        public void A06_UnsupportedDefinitionDoesNotClaimMySqlLacksTheOperation()
        {
            var message = Message("MySqlColumnDdlUnsupportedDefinition");

            Assert.IsFalse(Contains(message, "MySQL does not support"));
            Assert.IsFalse(Contains(message, "this column is AUTO_INCREMENT"));
        }

        [TestMethod]
        public void A07_ReadFailureDoesNotDisplayRawReaderException()
        {
            const string secret = "Password=SECRET_C3A_TEST";

            var reason = MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey("F7U3C4B3_READ_FAILED: " + secret);
            var message = Message(reason);

            Assert.AreEqual("MySqlColumnDdlMetadataReadFailed", reason);
            Assert.IsFalse(Contains(message, secret));
            Assert.IsFalse(Contains(message, "F7U3C4B3_"));
        }

        [TestMethod]
        public void A08_StableReasonClassificationRemainsUnchanged()
        {
            Assert.AreEqual("MySqlColumnDdlPendingTransaction", MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey("F7U2C2_TRANSACTION_UNSAFE"));
            Assert.AreEqual("MySqlColumnDdlUnsupportedDefinition", MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey("F7U3C4B3_METADATA_OUTSIDE_SAFE_SUBSET"));
            Assert.AreEqual("MySqlColumnDdlMetadataUnverified", MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey("F7U3C4B3_ATTRIBUTES_NOT_PROVEN"));
            Assert.AreEqual("MySqlColumnDdlVersionUnverified", MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey("F7U3C4B3_VERSION_UNVERIFIED"));
            Assert.AreEqual("MySqlColumnDdlDefinitionUnverified", MySqlColumnDdlPreviewFeedbackPolicy.GetReasonKey("UNRECOGNIZED_STATUS"));
        }

        [TestMethod]
        public void A09_AllThreeLanguagesRetainUniqueExistingFeedbackKeys()
        {
            var keys = new[]
            {
                "MySqlColumnDdlPendingTransaction",
                "MySqlColumnDdlUnsupportedDefinition",
                "MySqlColumnDdlMetadataUnverified",
                "MySqlColumnDdlConnectionUnverified",
                "MySqlColumnDdlConnectionChanged",
                "MySqlColumnDdlVersionUnverified",
                "MySqlColumnDdlMetadataReadFailed",
                "MySqlColumnDdlDefinitionUnverified",
                "MySqlSqlPreviewUnavailable"
            };

            foreach (var file in new[] { "chinese-cht.xml", "chinese-chs.xml", "english.xml" })
            {
                var xml = XDocument.Load(GetLocalizationPath(file));

                var rows = xml.Descendants("language")
                    .Where(x => (string)x.Attribute("category") == "form"
                                && (string)x.Attribute("class") == "EditColumnForm"
                                && (string)x.Attribute("type") == "msg"
                                && (string)x.Attribute("attribute") == "Text")
                    .ToArray();

                foreach (var key in keys)
                {
                    var matched = rows.Where(x => (string)x.Attribute("id") == key).ToArray();

                    Assert.AreEqual(1, matched.Length, file + " / " + key + " must be unique.");
                    Assert.IsFalse(string.IsNullOrWhiteSpace((string)matched[0].Attribute("name")), file + " / " + key + " has no translation.");
                }
            }
        }

        [TestMethod]
        public void A10_FeedbackDoesNotGrantExistingColumnExecutePermission()
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, null, false));
        }
    }
}
