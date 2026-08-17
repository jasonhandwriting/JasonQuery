namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal sealed class PostgreSqlCreateScriptFormatContext
    {
        public string SchemaNode { get; private set; }

        public string SchemaType { get; private set; }

        public string SchemaName { get; private set; }

        public string ScriptText { get; private set; }


        public static PostgreSqlCreateScriptFormatContext Create(string schemaNode, string schemaType, string schemaName, string scriptText)
        {
            return new PostgreSqlCreateScriptFormatContext
            {
                SchemaNode = schemaNode ?? string.Empty,
                SchemaType = schemaType ?? string.Empty,
                SchemaName = schemaName ?? string.Empty,
                ScriptText = scriptText ?? string.Empty
            };
        }
    }
}