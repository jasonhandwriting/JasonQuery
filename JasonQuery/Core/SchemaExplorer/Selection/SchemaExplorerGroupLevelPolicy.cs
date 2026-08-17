using JasonQuery.Core.Database.Connection;

namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public static class SchemaExplorerGroupLevelPolicy
    {
        public static int GetMinimumObjectGroupLevel(DataSourceType sourceType)
        {
            switch (sourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return 2;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return 3;
                    }
                default:
                    {
                        return int.MaxValue;
                    }
            }
        }

        public static int GetSchemaTypeGroupLevel(DataSourceType sourceType)
        {
            switch (sourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return 1;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return 2;
                    }
                default:
                    {
                        return -1;
                    }
            }
        }

        public static bool IsObjectGroupLevel(DataSourceType sourceType, int groupLevel)
        {
            return groupLevel >= GetMinimumObjectGroupLevel(sourceType);
        }
    }
}