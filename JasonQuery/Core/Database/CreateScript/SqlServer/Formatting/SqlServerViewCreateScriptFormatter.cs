using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal sealed class SqlServerViewCreateScriptFormatter : SqlServerCreateScriptFormatterBase
    {
        protected override bool MatchesSchemaType(string schemaType)
        {
            return string.Equals(schemaType, SchemaObjectNames.Views, StringComparison.Ordinal);
        }

        protected override string FormatBody(string body, SqlServerCreateScriptFormatContext context)
        {
            var batches = SqlServerCreateScriptFormatterHelper.SplitGoBatches(body);

            for (var i = 0; i < batches.Count; i++)
            {
                batches[i] = SqlServerViewTriggerBodyBeautifierHelper.FormatViewBatch(batches[i]);
            }

            return SqlServerCreateScriptFormatterHelper.JoinGoBatches(batches);
        }
    }
}