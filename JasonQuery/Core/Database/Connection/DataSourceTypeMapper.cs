using JasonLibrary.Core.Database.Enums;

namespace JasonQuery.Core.Database.Connection
{
    public static class DataSourceTypeMapper
    {
        public static DatabaseProviderKind ToDatabaseProviderKind(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return DatabaseProviderKind.Oracle;
                    }
                case DataSourceType.PostgreSql:
                    {
                        return DatabaseProviderKind.PostgreSql;
                    }
                case DataSourceType.SqlServer:
                    {
                        return DatabaseProviderKind.SqlServer;
                    }
                case DataSourceType.MySql:
                    {
                        return DatabaseProviderKind.MySql;
                    }
                case DataSourceType.None:
                default:
                    {
                        return DatabaseProviderKind.Unknown;
                    }
            }
        }
    }
}
