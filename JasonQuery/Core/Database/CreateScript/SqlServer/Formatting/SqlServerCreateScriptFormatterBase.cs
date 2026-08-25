using System;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal abstract class SqlServerCreateScriptFormatterBase : ISqlServerCreateScriptFormatter
    {
        public bool CanFormat(SqlServerCreateScriptFormatContext context)
        {
            if (context == null)
            {
                return false;
            }

            return MatchesSchemaType(context.SchemaType);
        }

        public string Format(SqlServerCreateScriptFormatContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var script = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(context.ScriptText);

            SqlServerCreateScriptFormatterHelper.SplitHeaderAndBody(script, out var header, out var body);

            header = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(header);
            body = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(body);
            body = FormatBody(body, context);

            return SqlServerCreateScriptFormatterHelper.CombineHeaderAndBody(header, body);
        }

        protected abstract bool MatchesSchemaType(string schemaType);

        protected virtual string FormatBody(string body, SqlServerCreateScriptFormatContext context)
        {
            return body;
        }
    }
}
