using System;
using System.Data;
using System.Globalization;

namespace JasonQuery.Core.Database.Metadata.SqlServer
{
    internal sealed class SqlServerMetadataContext
    {
        public string connectionName { get; set; } = string.Empty;

        public string databaseName { get; set; } = string.Empty;

        public string separator { get; set; } = " ";

        public string dateTimeFormat { get; set; } = "yyyy/MM/dd HH:mm:ss"; //由呼叫端帶入，例如 $"{MyLibrary.DateFormat} HH:mm:ss"

        public DataTable dtSchema { get; set; }

        public DataColumnCollection schemaColumns => dtSchema?.Columns;

        public void Validate()
        {
            if (dtSchema == null)
            {
                throw new InvalidOperationException("dtSchema cannot be null !");
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("databaseName cannot be null !");
            }
        }

        public string BuildSchemaTypeText(string schemaObjectName, int count)
        {
            var formattedCount = count.ToString("N0", CultureInfo.InvariantCulture);

            return $"{schemaObjectName}{separator}({formattedCount})";
        }
    }
}
