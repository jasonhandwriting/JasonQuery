using JasonQuery.Core.Arrange.MySql;
using JasonQuery.Core.Arrange.Oracle;
using JasonQuery.Core.Arrange.PostgreSql;
using JasonQuery.Core.Arrange.SqlServer;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Arrange
{
    public static class ArrangeStrategyFactory
    {
        private static readonly Dictionary<DataSourceType, Func<IArrangeStrategy>> _strategyMap
                                = new Dictionary<DataSourceType, Func<IArrangeStrategy>>
        {
            {
                DataSourceType.Oracle,
                () => new OracleArrangeStrategy(new OracleSchemaPreparationStrategy())
            },
            {
                DataSourceType.PostgreSql,
                () => new PostgreSqlArrangeStrategy(new PostgreSqlSchemaPreparationStrategy())
            },
            {
                DataSourceType.SqlServer,
                () => new SqlServerArrangeStrategy(new SqlServerSchemaPreparationStrategy())
            },
            {
                DataSourceType.MySql,
                () => new MySqlArrangeStrategy(new MySqlSchemaPreparationStrategy())
            }
        };

        public static IArrangeStrategy Create(DataSourceType sourceType)
        {
            if (_strategyMap.TryGetValue(sourceType, out var factory))
            {
                return factory();
            }

            throw ExceptionHelper.NotSupportedDatabase(sourceType);
        }
    }
}
