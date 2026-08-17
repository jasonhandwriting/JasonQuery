namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal sealed class MySqlCreateScriptFormatContext
    {
        public string SchemaType { get; private set; }

        public string SchemaNode { get; private set; }

        public string SchemaName { get; private set; }

        public string ScriptText { get; private set; }

        public static MySqlCreateScriptFormatContext Create(string schemaType, string schemaNode, string schemaName, string scriptText)
        {
            return new MySqlCreateScriptFormatContext
            {
                SchemaType = schemaType ?? string.Empty,
                SchemaNode = schemaNode ?? string.Empty,
                SchemaName = schemaName ?? string.Empty,
                ScriptText = scriptText ?? string.Empty
            };
        }
    }
}