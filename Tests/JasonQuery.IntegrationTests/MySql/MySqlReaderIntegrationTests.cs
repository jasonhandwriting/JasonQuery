using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.MySql
{
    [TestClass]
    [DoNotParallelize]
    public sealed class MySqlReaderIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        public void ConnectionSmoke_ConnectsAndDisconnects()
        {
            IntegrationTestAssertions.AssertConnectionSmoke(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        public void ScalarQuery_ReturnsOne()
        {
            IntegrationTestAssertions.AssertScalarQuery(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Paging")]
        public void ExecutePageReader_ReturnsRequestedRows()
        {
            IntegrationTestAssertions.AssertPagedQuery(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Schema")]
        public void GetSchemaTable_ReturnsRequiredMetadata()
        {
            IntegrationTestAssertions.AssertSchemaTable(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Schema")]
        public void SchemaColumnInfoBuilder_ResolvesNumberAndStringColumns()
        {
            IntegrationTestAssertions.AssertColumnInfoCollector(TestContext, DataSourceType.MySql);
        }
    }
}