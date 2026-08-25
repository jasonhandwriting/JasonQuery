using System;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal sealed class SqlServerModuleCreateScriptFormatter : SqlServerCreateScriptFormatterBase
    {
        private readonly string _schemaType;

        public SqlServerModuleCreateScriptFormatter(string schemaType)
        {
            _schemaType = schemaType ?? string.Empty;
        }

        protected override bool MatchesSchemaType(string schemaType)
        {
            return string.Equals(schemaType, _schemaType, StringComparison.Ordinal);
        }

        protected override string FormatBody(string body, SqlServerCreateScriptFormatContext context)
        {
            var batches = SqlServerCreateScriptFormatterHelper.SplitGoBatches(body);

            for (var i = 0; i < batches.Count; i++)
            {
                batches[i] = SqlServerClauseStyleFormatterHelper.FormatModuleBatch
                (
                    batches[i],
                    context.SchemaType
                );
            }

            return SqlServerCreateScriptFormatterHelper.JoinGoBatches(batches);
        }
    }
}
