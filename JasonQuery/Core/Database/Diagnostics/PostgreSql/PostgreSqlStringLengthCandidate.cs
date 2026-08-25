namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlStringLengthCandidate
    {
        public string SchemaName { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;

        public string ColumnName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public int? MaxLength { get; set; }

        public int? ActualLength { get; set; }

        public int? ValuesRowIndex { get; set; }

        public string SourceValuePreview { get; set; } = string.Empty;

        public string FullColumnName
        {
            get
            {
                if (string.IsNullOrEmpty(SchemaName))
                {
                    return string.IsNullOrEmpty(TableName) ? ColumnName : $"{TableName}.{ColumnName}";
                }

                return $"{SchemaName}.{TableName}.{ColumnName}";
            }
        }

        public string DisplayDataType
        {
            get
            {
                if (MaxLength.HasValue && (string.Equals(DataType, "character varying", System.StringComparison.OrdinalIgnoreCase)
                    || string.Equals(DataType, "character", System.StringComparison.OrdinalIgnoreCase)))
                {
                    return $"{DataType}({MaxLength.Value})";
                }

                return DataType;
            }
        }
    }
}
