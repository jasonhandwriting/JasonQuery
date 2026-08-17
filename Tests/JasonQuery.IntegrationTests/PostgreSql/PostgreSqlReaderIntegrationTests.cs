using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.PostgreSql
{
    [TestClass]
    [DoNotParallelize]
    public sealed class PostgreSqlReaderIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        public void ConnectionSmoke_ConnectsAndDisconnects()
        {
            IntegrationTestAssertions.AssertConnectionSmoke(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        public void ScalarQuery_ReturnsOne()
        {
            IntegrationTestAssertions.AssertScalarQuery(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Paging")]
        public void ExecutePageReader_ReturnsRequestedRows()
        {
            IntegrationTestAssertions.AssertPagedQuery(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Schema")]
        public void GetSchemaTable_ReturnsRequiredMetadata()
        {
            IntegrationTestAssertions.AssertSchemaTable(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Schema")]
        public void SchemaColumnInfoBuilder_ResolvesNumberAndStringColumns()
        {
            IntegrationTestAssertions.AssertColumnInfoCollector(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("KeyInfo")]
        public void KeyInfo_PrimaryKeyColumn_IsReported()
        {
            IntegrationTestAssertions.AssertPrimaryKeyMetadata(TestContext, DataSourceType.PostgreSql);
        }
    }
}