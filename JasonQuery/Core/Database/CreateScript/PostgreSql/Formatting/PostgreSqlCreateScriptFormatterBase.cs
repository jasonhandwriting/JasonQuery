using System;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal abstract class PostgreSqlCreateScriptFormatterBase : IPostgreSqlCreateScriptFormatter
    {
        public bool CanFormat(PostgreSqlCreateScriptFormatContext context)
        {
            if (context == null)
            {
                return false;
            }

            return MatchesSchemaType(context.SchemaType);
        }

        public string Format(PostgreSqlCreateScriptFormatContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var script = PostgreSqlCreateScriptFormatterHelper.NormalizeLineEndings(context.ScriptText);

            PostgreSqlCreateScriptFormatterHelper.SplitHeaderAndBody
            (
                script,
                out var header,
                out var body
            );

            header = PostgreSqlCreateScriptFormatterHelper.TrimOuterBlankLines(header);
            body = PostgreSqlCreateScriptFormatterHelper.TrimOuterBlankLines(body);
            body = FormatBody(body, context);

            return PostgreSqlCreateScriptFormatterHelper.CombineHeaderAndBody(header, body);
        }

        protected abstract bool MatchesSchemaType(string schemaType);

        protected virtual string FormatBody(string body, PostgreSqlCreateScriptFormatContext context)
        {
            return body;
        }
    }
}
