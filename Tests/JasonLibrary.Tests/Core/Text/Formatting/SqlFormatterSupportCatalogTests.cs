using System.Linq;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatterSupportCatalogTests
    {
        [TestMethod]
        public void SqlServer_PlannedDefault_IsMicrosoftScriptDom()
        {
            var descriptor = SqlFormatterSupportCatalog.GetPlannedDefault(DatabaseProviderKind.SqlServer);

            Assert.IsNotNull(descriptor);
            Assert.AreEqual(SqlFormatterEngineKind.MicrosoftScriptDom, descriptor.Kind);
            Assert.AreEqual(SqlFormatterEngineReadiness.Ready, descriptor.Readiness);
        }

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle)]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        [DataRow(DatabaseProviderKind.MySql)]
        [DataRow(DatabaseProviderKind.Sqlite)]
        public void NonSqlServer_PlannedDefault_IsHogimnBehindSafetyGate(DatabaseProviderKind providerKind)
        {
            var descriptor = SqlFormatterSupportCatalog.GetPlannedDefault(providerKind);

            Assert.IsNotNull(descriptor);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, descriptor.Kind);
            Assert.AreEqual(SqlFormatterEngineReadiness.Ready, descriptor.Readiness);
        }

        [TestMethod]
        public void SqlServer_Candidates_IncludeTwoRejectedEnginesForAssessmentHistory()
        {
            var candidates = SqlFormatterSupportCatalog.GetCandidates(DatabaseProviderKind.SqlServer);

            CollectionAssert.AreEquivalent
            (
                new[]
                {
                    SqlFormatterEngineKind.MicrosoftScriptDom,
                    SqlFormatterEngineKind.Hogimn,
                    SqlFormatterEngineKind.Laan,
                    SqlFormatterEngineKind.TSqlSharp
                },
                candidates.Select(candidate => candidate.Kind).ToArray()
            );
        }

        [TestMethod]
        public void PostgreSql_ReadyEngine_IsHogimnBehindSafetyGate()
        {
            var engines = SqlFormatterSupportCatalog.GetReadyEngines(DatabaseProviderKind.PostgreSql);

            Assert.AreEqual(1, engines.Count);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, engines[0].Kind);
            StringAssert.Contains(engines[0].AssessmentNote, "safety gate");
        }

        [TestMethod]
        public void SqlServer_ReadyEngines_IncludeScriptDomAndHogimn()
        {
            var engines = SqlFormatterSupportCatalog.GetReadyEngines(DatabaseProviderKind.SqlServer);

            CollectionAssert.AreEquivalent
            (
                new[]
                {
                    SqlFormatterEngineKind.MicrosoftScriptDom,
                    SqlFormatterEngineKind.Hogimn
                },
                engines.Select(engine => engine.Kind).ToArray()
            );
        }
    }
}