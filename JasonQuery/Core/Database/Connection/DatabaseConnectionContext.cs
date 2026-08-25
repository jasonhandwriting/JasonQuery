namespace JasonQuery.Core.Database.Connection
{
    public sealed class DatabaseConnectionContext
    {
        private DataSourceType _currentDataSource = DataSourceType.None;
        private string _dataSourceDisplayName = string.Empty;

        public DatabaseConnectionContext()
        {
            Options = new DatabaseConnectionOptions();
        }

        public string DbUser { get; set; } = string.Empty;

        public string DbUserUppercase
        {
            get
            {
                return (DbUser ?? string.Empty).ToUpperInvariant();
            }
        }

        public string DbPassword { get; set; } = string.Empty;

        public string OracleSid { get; set; } = string.Empty;

        public string OracleConnectAs { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public int DbConnectionPort { get; set; } = 0;

        public string DbConnectionString { get; set; } = string.Empty;

        public string DbConnectionTitle { get; set; } = string.Empty;

        public string DbConnectionName { get; set; } = string.Empty;

        public string DbConnectionServer { get; set; } = string.Empty;

        public DatabaseConnectionOptions Options { get; private set; }

        public string DbServerVersion { get; set; } = string.Empty;

        public SqlServerVersionInfo SqlServerVersion { get; set; } = SqlServerVersionInfo.Unknown;

        public DatabaseServerVersionInfo ServerVersionInfo { get; set; } = DatabaseServerVersionInfo.Unknown;

        public void Reset()
        {
            _currentDataSource = DataSourceType.None;
            _dataSourceDisplayName = string.Empty;

            DbUser = string.Empty;
            DbPassword = string.Empty;
            DbServerVersion = string.Empty;
            SqlServerVersion = SqlServerVersionInfo.Unknown;
            OracleSid = string.Empty;
            OracleConnectAs = string.Empty;
            DatabaseName = string.Empty;
            ServerVersionInfo = DatabaseServerVersionInfo.Unknown;

            DbConnectionPort = 0;
            DbConnectionString = string.Empty;
            DbConnectionTitle = string.Empty;
            DbConnectionName = string.Empty;
            DbConnectionServer = string.Empty;

            Options.Reset();
        }
    }
}
