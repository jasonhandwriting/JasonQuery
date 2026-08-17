using JasonQuery.Core.Database.Metadata;
using System.Linq;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal static class MySqlCreateScriptFormatterDispatcher
    {
        private static readonly IMySqlCreateScriptFormatter[] _formatters =
        {
            new MySqlTableCreateScriptFormatter(),
            new MySqlViewCreateScriptFormatter(),
            new MySqlRoutineCreateScriptFormatter(SchemaObjectNames.Procedures),
            new MySqlRoutineCreateScriptFormatter(SchemaObjectNames.Functions),
            new MySqlTriggerCreateScriptFormatter(),
            new MySqlIndexCreateScriptFormatter()
        };

        public static string Format(string schemaType, string schemaNode, string schemaName, string scriptText)
        {
            var context = MySqlCreateScriptFormatContext.Create(schemaType, schemaNode, schemaName, scriptText);
            var formatter = _formatters.FirstOrDefault(x => x.CanFormat(context));

            if (formatter == null)
            {
                return MySqlCreateScriptFormatterHelper.NormalizeLineEndings(scriptText);
            }

            return formatter.Format(context);
        }
    }
}