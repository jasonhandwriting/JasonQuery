namespace JasonQuery.Core.Database.Connection
{
    public static class DataSourceTypeHelper
    {
        public static DataSourceType Parse(string value)
        {
            var normalized = value.ToUpperInvariant();

            switch (normalized)
            {
                case "ORACLE":
                    {
                        return DataSourceType.Oracle;
                    }
                case "POSTGRESQL":
                    {
                        return DataSourceType.PostgreSql;
                    }
                case "SQLSERVER":
                case "SQL SERVER":
                    {
                        return DataSourceType.SqlServer;
                    }
                case "MYSQL":
                case "MYSQL/MARIADB":
                    {
                        return DataSourceType.MySql;
                    }
                default:
                    {
                        return DataSourceType.None;
                    }
            }
        }

        /// <summary>
        /// 給內部邏輯/正式名稱使用。
        /// 不包含過渡期舊名，也不包含 UI 特殊顯示字樣。
        /// </summary>
        public static string ToCanonicalName(DataSourceType dataSource)
        {
            switch (dataSource)
            {
                case DataSourceType.Oracle:
                    {
                        return "Oracle";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return "PostgreSQL";
                    }
                case DataSourceType.SqlServer:
                    {
                        return "SQL Server";
                    }
                case DataSourceType.MySql:
                    {
                        return "MySQL";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        /// <summary>
        /// 給 UI 顯示使用的名稱。
        /// </summary>
        public static string ToDisplayName(DataSourceType dataSource)
        {
            switch (dataSource)
            {
                case DataSourceType.Oracle:
                    {
                        return "Oracle";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return "PostgreSQL";
                    }
                case DataSourceType.SqlServer:
                    {
                        return "SQL Server";
                    }
                case DataSourceType.MySql:
                    {
                        return "MySQL/MariaDB";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }
    }
}