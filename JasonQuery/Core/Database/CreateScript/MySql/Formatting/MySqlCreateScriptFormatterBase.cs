using System;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal abstract class MySqlCreateScriptFormatterBase : IMySqlCreateScriptFormatter
    {
        public bool CanFormat(MySqlCreateScriptFormatContext context)
        {
            if (context == null)
            {
                return false;
            }

            return MatchesSchemaType(context.SchemaType);
        }

        public string Format(MySqlCreateScriptFormatContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var script = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(context.ScriptText);

            script = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(script);
            return FormatScript(script, context);
        }

        protected abstract bool MatchesSchemaType(string schemaType);

        protected virtual string FormatScript(string script, MySqlCreateScriptFormatContext context)
        {
            return script;
        }
    }
}