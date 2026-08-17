using JasonQuery.Core.Database.Metadata;
using System;

namespace JasonQuery.Core.Database.CreateScript.MySql
{
    internal sealed class MySqlCreateScriptRequest
    {
        public string SchemaType { get; private set; }

        public string SchemaNode { get; private set; } //DatabaseName

        public string SchemaName { get; private set; } //ObjectName, "IndexName on TableName"

        public string IndexName { get; private set; }

        public string BaseTableName { get; private set; }

        public bool IsIndexRequest
        {
            get
            {
                return SchemaObjectTypeHelper.Is(SchemaType, SchemaObjectNames.Indexes);
            }
        }

        public static MySqlCreateScriptRequest Create(string schemaType, string schemaNode, string schemaName)
        {
            var request = new MySqlCreateScriptRequest
            {
                SchemaType = schemaType ?? string.Empty,
                SchemaNode = schemaNode ?? string.Empty,
                SchemaName = schemaName ?? string.Empty,
                IndexName = string.Empty,
                BaseTableName = string.Empty
            };

            if (request.IsIndexRequest && !string.IsNullOrWhiteSpace(request.SchemaName))
            {
                var pos = request.SchemaName.IndexOf(" on ", StringComparison.OrdinalIgnoreCase);

                if (pos > 0)
                {
                    request.IndexName = request.SchemaName.Substring(0, pos).Trim();
                    request.BaseTableName = request.SchemaName.Substring(pos + 4).Trim();
                }
                else
                {
                    request.IndexName = request.SchemaName.Trim();
                }
            }

            return request;
        }
    }
}