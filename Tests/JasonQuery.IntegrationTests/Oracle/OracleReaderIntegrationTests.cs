using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.Oracle
{
    [TestClass]
    [DoNotParallelize]
    public sealed class OracleReaderIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        public void ConnectionSmoke_ConnectsAndDisconnects()
        {
            IntegrationTestAssertions.AssertConnectionSmoke(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        public void ScalarQuery_ReturnsOne()
        {
            IntegrationTestAssertions.AssertScalarQuery(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Paging")]
        public void ExecutePageReader_ReturnsRequestedRows()
        {
            IntegrationTestAssertions.AssertPagedQuery(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Schema")]
        public void GetSchemaTable_ReturnsRequiredMetadata()
        {
            IntegrationTestAssertions.AssertSchemaTable(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Schema")]
        public void SchemaColumnInfoBuilder_ResolvesNumberAndStringColumns()
        {
            IntegrationTestAssertions.AssertColumnInfoCollector(TestContext, DataSourceType.Oracle);
        }
    }
}