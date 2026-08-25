using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal sealed class PostgreSqlProcedureCreateScriptFormatter : PostgreSqlCreateScriptFormatterBase
    {
        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Procedures);
        }

        protected override string FormatBody(string body, PostgreSqlCreateScriptFormatContext context)
        {
            return PostgreSqlRoutineCreateScriptFormatterHelper.FormatRoutineBody(body);
        }
    }
}
