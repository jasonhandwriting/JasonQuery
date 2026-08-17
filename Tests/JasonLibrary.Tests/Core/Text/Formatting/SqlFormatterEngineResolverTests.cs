using System;
using System.Collections.Generic;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Text.Formatting.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatterEngineResolverTests
    {
        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle, SqlFormatterEngineKind.Hogimn)]
        [DataRow(DatabaseProviderKind.PostgreSql, SqlFormatterEngineKind.Hogimn)]
        [DataRow(DatabaseProviderKind.SqlServer, SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(DatabaseProviderKind.MySql, SqlFormatterEngineKind.Hogimn)]
        [DataRow(DatabaseProviderKind.Sqlite, SqlFormatterEngineKind.Hogimn)]
        public void TryResolve_DefaultSelection_ReturnsPlannedEngine(DatabaseProviderKind providerKind, SqlFormatterEngineKind expectedEngineKind)
        {
            var resolver = new SqlFormatterEngineResolver();

            var resolved = resolver.TryResolve
            (
                providerKind,
                SqlFormatterEngineKind.Unknown,
                out var engine,
                out var errorMessage
            );

            Assert.IsTrue(resolved);
            Assert.IsNotNull(engine);
            Assert.AreEqual(expectedEngineKind, engine.Kind);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void TryResolve_SqlServerHogimnOverride_ReturnsHogimn()
        {
            var resolver = new SqlFormatterEngineResolver();

            var resolved = resolver.TryResolve
            (
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.Hogimn,
                out var engine,
                out var errorMessage
            );

            Assert.IsTrue(resolved);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, engine.Kind);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void TryResolve_UnsupportedOverride_ReturnsFailure()
        {
            var resolver = new SqlFormatterEngineResolver();

            var resolved = resolver.TryResolve
            (
                DatabaseProviderKind.Oracle,
                SqlFormatterEngineKind.MicrosoftScriptDom,
                out var engine,
                out var errorMessage
            );

            Assert.IsFalse(resolved);
            Assert.IsNull(engine);
            StringAssert.Contains(errorMessage, "does not support");
        }

        [TestMethod]
        public void TryResolve_RejectedEngine_ReturnsFailure()
        {
            var resolver = new SqlFormatterEngineResolver();

            var resolved = resolver.TryResolve
            (
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.Laan,
                out var engine,
                out var errorMessage
            );

            Assert.IsFalse(resolved);
            Assert.IsNull(engine);
            StringAssert.Contains(errorMessage, "not ready");
        }

        [TestMethod]
        public void TryResolve_UnknownProvider_ReturnsFailure()
        {
            var resolver = new SqlFormatterEngineResolver();

            var resolved = resolver.TryResolve
            (
                DatabaseProviderKind.Unknown,
                SqlFormatterEngineKind.Unknown,
                out var engine,
                out var errorMessage
            );

            Assert.IsFalse(resolved);
            Assert.IsNull(engine);
            StringAssert.Contains(errorMessage, "No default formatter engine");
        }

        [TestMethod]
        public void TryResolve_DefaultImplementationIsMissing_ReturnsFailure()
        {
            var resolver = new SqlFormatterEngineResolver
            (
                new ISqlFormatterEngine[]
                {
                    new MicrosoftScriptDomSqlFormatterEngine()
                }
            );

            var resolved = resolver.TryResolve
            (
                DatabaseProviderKind.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                out var engine,
                out var errorMessage
            );

            Assert.IsFalse(resolved);
            Assert.IsNull(engine);
            StringAssert.Contains(errorMessage, "implementation is not registered");
        }

        [TestMethod]
        public void Constructor_NullCollection_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => new SqlFormatterEngineResolver((IEnumerable<ISqlFormatterEngine>)null)
            );
        }

        [TestMethod]
        public void Constructor_NullEntry_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>
            (
                () => new SqlFormatterEngineResolver(new ISqlFormatterEngine[] { null })
            );
        }

        [TestMethod]
        public void Constructor_DuplicateEngineKind_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>
            (
                () => new SqlFormatterEngineResolver
                (
                    new ISqlFormatterEngine[]
                    {
                        new HogimnSqlFormatterEngine(),
                        new HogimnSqlFormatterEngine()
                    }
                )
            );
        }
    }
}