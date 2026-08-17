using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal sealed class PostgreSqlTriggerCreateScriptFormatter : PostgreSqlCreateScriptFormatterBase
    {
        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Triggers);
        }

        protected override string FormatBody(string body, PostgreSqlCreateScriptFormatContext context)
        {
            return PostgreSqlCreateScriptFormatterHelper.TrimOuterBlankLines(body);
        }
    }
}