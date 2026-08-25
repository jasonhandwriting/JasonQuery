using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal sealed class SqlServerTriggerCreateScriptFormatter : SqlServerCreateScriptFormatterBase
    {
        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Triggers);
        }

        protected override string FormatBody(string body, SqlServerCreateScriptFormatContext context)
        {
            var batches = SqlServerCreateScriptFormatterHelper.SplitGoBatches(body);

            for (var i = 0; i < batches.Count; i++)
            {
                batches[i] = SqlServerViewTriggerBodyBeautifierHelper.FormatTriggerBatch(batches[i]);
            }

            return SqlServerCreateScriptFormatterHelper.JoinGoBatches(batches);
        }
    }
}
