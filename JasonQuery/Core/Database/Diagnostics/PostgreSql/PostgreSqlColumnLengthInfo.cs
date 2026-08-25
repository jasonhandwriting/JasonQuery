using System;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlColumnLengthInfo
    {
        public string SchemaName { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;

        public string ColumnName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public int OrdinalPosition { get; set; }

        public int? CharacterMaximumLength { get; set; }

        public string FullColumnName
        {
            get { return $"{SchemaName}.{TableName}.{ColumnName}"; }
        }

        public string DisplayDataType
        {
            get
            {
                if (CharacterMaximumLength.HasValue && (string.Equals(DataType, "character varying", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(DataType, "character", StringComparison.OrdinalIgnoreCase)))
                {
                    return $"{DataType}({CharacterMaximumLength.Value})";
                }

                return DataType;
            }
        }
    }
}
