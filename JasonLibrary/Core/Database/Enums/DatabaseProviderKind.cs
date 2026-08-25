using JasonLibrary.Infrastructure.Enums;

namespace JasonLibrary.Core.Database.Enums
{
    public enum DatabaseProviderKind
    {
        Unknown = 0,
        Oracle,
        [EnumAlias("PostgreSQL")]
        PostgreSql,
        [EnumAlias("SQL Server")]
        SqlServer,
        [EnumAlias("MySQL/MariaDB")]
        MySql,
        [EnumAlias("SQLite")]
        Sqlite
    }
}
