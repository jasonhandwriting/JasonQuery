using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.SqlServer
{
    [TestClass]
    [DoNotParallelize]
    public sealed class SqlServerReaderIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        public void ConnectionSmoke_ConnectsAndDisconnects()
        {
            IntegrationTestAssertions.AssertConnectionSmoke(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        public void ScalarQuery_ReturnsOne()
        {
            IntegrationTestAssertions.AssertScalarQuery(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Paging")]
        public void ExecutePageReader_ReturnsRequestedRows()
        {
            IntegrationTestAssertions.AssertPagedQuery(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Schema")]
        public void GetSchemaTable_ReturnsRequiredMetadata()
        {
            IntegrationTestAssertions.AssertSchemaTable(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Schema")]
        public void SchemaColumnInfoBuilder_ResolvesNumberAndStringColumns()
        {
            IntegrationTestAssertions.AssertColumnInfoCollector(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("KeyInfo")]
        public void KeyInfo_PrimaryKeyColumn_IsReported()
        {
            IntegrationTestAssertions.AssertPrimaryKeyMetadata(TestContext, DataSourceType.SqlServer);
        }
    }
}
