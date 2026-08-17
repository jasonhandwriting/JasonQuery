using System.Linq;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerCreateScriptFormatterDispatcher
    {
        private static readonly ISqlServerCreateScriptFormatter[] _formatters =
        {
            new SqlServerTableCreateScriptFormatter(),
            new SqlServerViewCreateScriptFormatter(),
            new SqlServerFunctionCreateScriptFormatter(),
            new SqlServerProcedureCreateScriptFormatter(),
            new SqlServerTriggerCreateScriptFormatter(),
            new SqlServerIndexCreateScriptFormatter()
        };

        public static string Format(string schemaType, string schemaNode, string schemaDbo, string schemaName, string scriptText)
        {
            var context = SqlServerCreateScriptFormatContext.Create(schemaType, schemaNode, schemaDbo, schemaName, scriptText);
            var formatter = _formatters.FirstOrDefault(x => x.CanFormat(context));

            if (formatter == null)
            {
                return SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(scriptText);
            }

            return formatter.Format(context);
        }
    }
}