namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal sealed class SqlServerCreateScriptFormatContext
    {
        public string SchemaType { get; private set; }

        public string SchemaNode { get; private set; }

        public string SchemaDbo { get; private set; }

        public string SchemaName { get; private set; }

        public string ScriptText { get; private set; }

        public static SqlServerCreateScriptFormatContext Create(string schemaType, string schemaNode, string schemaDbo, string schemaName, string scriptText)
        {
            return new SqlServerCreateScriptFormatContext
            {
                SchemaType = schemaType ?? string.Empty,
                SchemaNode = schemaNode ?? string.Empty,
                SchemaDbo = schemaDbo ?? string.Empty,
                SchemaName = schemaName ?? string.Empty,
                ScriptText = scriptText ?? string.Empty
            };
        }
    }
}
