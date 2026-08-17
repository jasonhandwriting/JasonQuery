using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Table
{
    internal sealed class SqlServerTableSchemaBuildContext
    {
        public SqlServerMetadataContext metadataContext { get; set; }

        public Dictionary<(string DbName, string SchemaDbo, string TableName), long> rowCountMap { get; set; }
               = new Dictionary<(string DbName, string SchemaDbo, string TableName), long>();

        public Dictionary<(string DbName, string SchemaDbo, string TableName), List<DataRow>> columnInfoMap { get; set; }
               = new Dictionary<(string DbName, string SchemaDbo, string TableName), List<DataRow>>();

        public void Validate()
        {
            if (metadataContext == null)
            {
                throw new InvalidOperationException("metadataContext cannot be null !");
            }

            metadataContext.Validate();

            if (rowCountMap == null)
            {
                throw new InvalidOperationException("rowCountMap cannot be null !");
            }

            if (columnInfoMap == null)
            {
                throw new InvalidOperationException("columnInfoMap cannot be null !");
            }
        }
    }
}