using System.Linq;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    [TestClass]
    public sealed class SqlFormatterEnginePreferenceResolverTests
    {
        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle, SqlFormatterEngineKind.Hogimn)]
        [DataRow(DatabaseProviderKind.PostgreSql, SqlFormatterEngineKind.Hogimn)]
        [DataRow(DatabaseProviderKind.SqlServer, SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(DatabaseProviderKind.MySql, SqlFormatterEngineKind.Hogimn)]
        [DataRow(DatabaseProviderKind.Sqlite, SqlFormatterEngineKind.Hogimn)]
        public void GetEffectiveEngine_Unknown_ReturnsPlannedDefault(DatabaseProviderKind providerKind, SqlFormatterEngineKind expected)
        {
            var actual = SqlFormatterEnginePreferenceResolver.GetEffectiveEngine
            (
                providerKind,
                SqlFormatterEngineKind.Unknown
            );

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Parse_ValidSupportedEngine_ReturnsStoredPreference()
        {
            var actual = SqlFormatterEnginePreferenceResolver.Parse
            (
                DatabaseProviderKind.SqlServer,
                "Hogimn"
            );

            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, actual);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("NotAnEngine")]
        [DataRow("Laan")]
        [DataRow("TSqlSharp")]
        public void Parse_InvalidOrUnavailableValue_ReturnsUnknown(string storedValue)
        {
            var actual = SqlFormatterEnginePreferenceResolver.Parse
            (
                DatabaseProviderKind.SqlServer,
                storedValue
            );

            Assert.AreEqual(SqlFormatterEngineKind.Unknown, actual);
        }

        [TestMethod]
        public void Parse_EngineDoesNotSupportProvider_ReturnsUnknown()
        {
            var actual = SqlFormatterEnginePreferenceResolver.Parse
            (
                DatabaseProviderKind.Oracle,
                "MicrosoftScriptDom"
            );

            Assert.AreEqual(SqlFormatterEngineKind.Unknown, actual);
        }

        [TestMethod]
        public void GetChoices_SqlServer_ReturnsConcreteReadyEnginesAndMarksRecommended()
        {
            var choices = SqlFormatterEnginePreferenceResolver.GetChoices(DatabaseProviderKind.SqlServer);

            CollectionAssert.AreEqual
            (
                new[]
                {
                    SqlFormatterEngineKind.MicrosoftScriptDom,
                    SqlFormatterEngineKind.Hogimn
                },
                choices.Select(choice => choice.Kind).ToArray()
            );

            Assert.IsFalse(choices[0].SupportsMaxLineWidth);
            Assert.IsTrue(choices[0].SupportsListItemsPerLine);
            Assert.IsTrue(choices[0].IsRecommended);
            StringAssert.Contains(choices[0].DisplayName, "Recommended");
            Assert.IsTrue(choices[1].SupportsMaxLineWidth);
            Assert.IsTrue(choices[1].SupportsListItemsPerLine);
            Assert.IsFalse(choices[1].IsRecommended);
            Assert.IsFalse(choices[1].DisplayName.Contains("MIT"));
        }

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle)]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        [DataRow(DatabaseProviderKind.MySql)]
        [DataRow(DatabaseProviderKind.Sqlite)]
        public void GetChoices_NonSqlServer_ReturnsOnlyHogimn(DatabaseProviderKind providerKind)
        {
            var choices = SqlFormatterEnginePreferenceResolver.GetChoices(providerKind);

            CollectionAssert.AreEqual
            (
                new[]
                {
                    SqlFormatterEngineKind.Hogimn
                },
                choices.Select(choice => choice.Kind).ToArray()
            );

            Assert.IsTrue(choices.All(choice => choice.SupportsMaxLineWidth));
            Assert.IsTrue(choices.All(choice => choice.SupportsListItemsPerLine));
            Assert.IsTrue(choices[0].IsRecommended);
            Assert.IsFalse(choices[0].DisplayName.Contains("Recommended"));
            Assert.IsFalse(choices[0].DisplayName.Contains("MIT"));
        }

        [DataTestMethod]
        [DataRow(SqlFormatterEngineKind.Hogimn, true)]
        [DataRow(SqlFormatterEngineKind.MicrosoftScriptDom, true)]
        [DataRow(SqlFormatterEngineKind.Laan, false)]
        [DataRow(SqlFormatterEngineKind.TSqlSharp, false)]
        [DataRow(SqlFormatterEngineKind.Unknown, false)]
        public void SqlFormatterEngineChoice_ListItemsPerLineCapability_MatchesImplementedEngines(SqlFormatterEngineKind engineKind, bool expected)
        {
            var choice = new SqlFormatterEngineChoice(engineKind, engineKind.ToString(), false);

            Assert.AreEqual(expected, choice.SupportsListItemsPerLine);
        }
    }
}