using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Common
{
    internal sealed class SqlServerSimpleSchemaRowBuilder
    {
        public DataRow Build(SqlServerMetadataContext context, string schemaDbo, string schemaType, string schemaName, string objectId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var row = context.dtSchema.NewRow();

            row["SchemaObject"] = context.connectionName;
            row["SchemaNode"] = context.databaseName;
            row["SchemaDbo"] = schemaDbo;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = schemaName;
            row["ObjectID"] = objectId;

            return row;
        }
    }
}