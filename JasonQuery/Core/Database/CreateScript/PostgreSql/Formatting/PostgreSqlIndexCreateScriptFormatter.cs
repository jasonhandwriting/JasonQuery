using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal sealed class PostgreSqlIndexCreateScriptFormatter : PostgreSqlCreateScriptFormatterBase
    {
        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Functions);
        }

        protected override string FormatBody(string body, PostgreSqlCreateScriptFormatContext context)
        {
            body = PostgreSqlCreateScriptFormatterHelper.TrimOuterBlankLines(body);

            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            if (PostgreSqlCreateScriptFormatterHelper.TrySplitTrailingOwnerStatement(body, out var mainBody, out var ownerStatement))
            {
                return PostgreSqlCreateScriptFormatterHelper.JoinBlocks
                (
                    new[]
                    {
                        PostgreSqlCreateScriptFormatterHelper.NormalizeSimpleBlocks(mainBody),
                        ownerStatement
                    }
                );
            }

            return PostgreSqlCreateScriptFormatterHelper.NormalizeSimpleBlocks(body);
        }
    }
}