using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.SqlServer.View
{
    internal sealed class SqlServerViewSchemaBuildContext
    {
        public SqlServerMetadataContext metadataContext { get; set; }

        public Dictionary<(string DbName, string SchemaDbo, string ViewName), List<DataRow>> columnInfoMap { get; set; }
               = new Dictionary<(string DbName, string SchemaDbo, string ViewName), List<DataRow>>();

        public void Validate()
        {
            if (metadataContext == null)
            {
                throw new InvalidOperationException("metadataContext cannot be null !");
            }

            metadataContext.Validate();

            if (columnInfoMap == null)
            {
                throw new InvalidOperationException("columnInfoMap cannot be null !");
            }
        }
    }
}
