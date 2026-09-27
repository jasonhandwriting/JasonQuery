using JasonLibrary.Core.Database.Enums;
using JasonQuery.Core.Database.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Connection
{
    [TestClass]
    public sealed class DataSourceTypeMapperTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlFormatter")]
        [DataRow(DataSourceType.None, DatabaseProviderKind.Unknown)]
        [DataRow(DataSourceType.Oracle, DatabaseProviderKind.Oracle)]
        [DataRow(DataSourceType.PostgreSql, DatabaseProviderKind.PostgreSql)]
        [DataRow(DataSourceType.SqlServer, DatabaseProviderKind.SqlServer)]
        [DataRow(DataSourceType.MySql, DatabaseProviderKind.MySql)]
        public void ToDatabaseProviderKind_KnownDataSource_ReturnsExpectedProvider(DataSourceType dataSourceType, DatabaseProviderKind expected)
        {
            var actual = DataSourceTypeMapper.ToDatabaseProviderKind(dataSourceType);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlFormatter")]
        public void ToDatabaseProviderKind_UndefinedDataSource_ReturnsUnknown()
        {
            var actual = DataSourceTypeMapper.ToDatabaseProviderKind((DataSourceType)999);

            Assert.AreEqual(DatabaseProviderKind.Unknown, actual);
        }
    }
}
