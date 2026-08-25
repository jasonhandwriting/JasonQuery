namespace JasonQuery.Core.Database.CreateScript.SqlServer
{
    internal sealed class SqlServerCreateScriptRequest
    {
        public string SchemaType { get; private set; }

        public string SchemaNode { get; private set; }

        public string SchemaDbo { get; private set; }

        public string SchemaName { get; private set; }

        public string ObjectID { get; set; }

        public string SchemaNameWithoutSchemaDbo { get; private set; }

        public bool ExcludeNativeDatabase { get; private set; }

        public string DateTimeFormat { get; private set; }

        public int SqlServerMajorVersion { get; private set; }

        public bool SupportsOptimizeForSequentialKey
        {
            get
            {
                return SqlServerMajorVersion >= 15;
            }
        }

        public static SqlServerCreateScriptRequest Create(string schemaType, string schemaNode, string schemaDbo, string schemaName,
                                                          string objectID, bool excludeNativeDatabase, string dateTimeFormat, int sqlServerMajorVersion = 0)
        {
            return new SqlServerCreateScriptRequest
            {
                SchemaType = schemaType ?? string.Empty,
                SchemaNode = schemaNode ?? string.Empty,
                SchemaDbo = schemaDbo ?? string.Empty,
                SchemaName = schemaName ?? string.Empty,
                ObjectID = objectID ?? string.Empty,
                SchemaNameWithoutSchemaDbo = (schemaName ?? string.Empty).Replace($"{schemaDbo}.", string.Empty),
                ExcludeNativeDatabase = excludeNativeDatabase,
                DateTimeFormat = dateTimeFormat ?? "yyyy/MM/dd HH:mm:ss",
                SqlServerMajorVersion = sqlServerMajorVersion > 0 ? sqlServerMajorVersion : 0
            };
        }
    }
}
