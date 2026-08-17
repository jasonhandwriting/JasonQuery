using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.PostgreSql
{
    internal sealed class PostgreSqlCreateScriptRequest
    {
        public string SchemaNode { get; private set; }

        public string SchemaType { get; private set; }

        public string SchemaName { get; private set; }

        public string DatabaseName { get; private set; }

        public string DbVersion { get; private set; }

        public string ObjectName { get; private set; }

        public string RoutineNameLong { get; private set; }

        public string RoutineArguments { get; private set; }

        public bool IsVersion11OrAbove => string.Equals(DbVersion, ">=11", StringComparison.Ordinal);

        public static PostgreSqlCreateScriptRequest Create(string schemaNode, string schemaType, string schemaName, string databaseName, string dbVersion)
        {
            var request = new PostgreSqlCreateScriptRequest
            {
                SchemaNode = (schemaNode ?? string.Empty).Trim(),
                SchemaType = (schemaType ?? string.Empty).Trim(),
                SchemaName = (schemaName ?? string.Empty).Trim(),
                DatabaseName = (databaseName ?? string.Empty).Trim(),
                DbVersion = (dbVersion ?? string.Empty).Trim(),
                ObjectName = (schemaName ?? string.Empty).Trim(),
                RoutineNameLong = (schemaName ?? string.Empty).Trim(),
                RoutineArguments = string.Empty
            };

            ResolveRoutineName(request);

            return request;
        }

        private static void ResolveRoutineName(PostgreSqlCreateScriptRequest request)
        {
            if (!SchemaObjectTypeHelper.IsAny(request.SchemaType, SchemaObjectNames.Functions, SchemaObjectNames.Procedures))
            {
                return;
            }

            var pos = request.SchemaName.IndexOf('(');

            if (pos >= 0 && request.SchemaName.EndsWith(")", StringComparison.Ordinal))
            {
                request.ObjectName = request.SchemaName.Substring(0, pos);
                request.RoutineArguments = request.SchemaName.Substring(pos + 1, request.SchemaName.Length - pos - 2);
            }
        }
    }
}