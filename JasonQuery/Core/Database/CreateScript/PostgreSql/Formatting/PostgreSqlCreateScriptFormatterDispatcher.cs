using System.Linq;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal static class PostgreSqlCreateScriptFormatterDispatcher
    {
        private static readonly IPostgreSqlCreateScriptFormatter[] _formatters =
        {
            new PostgreSqlViewCreateScriptFormatter(),
            new PostgreSqlFunctionCreateScriptFormatter(),
            new PostgreSqlProcedureCreateScriptFormatter(),
            new PostgreSqlIndexCreateScriptFormatter(),
            new PostgreSqlTriggerCreateScriptFormatter()
        };

        public static string Format(string schemaNode, string schemaType, string schemaName, string scriptText)
        {
            var context = PostgreSqlCreateScriptFormatContext.Create(schemaNode, schemaType, schemaName, scriptText);
            var formatter = _formatters.FirstOrDefault(x => x.CanFormat(context));

            if (formatter == null)
            {
                return PostgreSqlCreateScriptFormatterHelper.NormalizeLineEndings(scriptText);
            }

            return formatter.Format(context);
        }
    }
}
