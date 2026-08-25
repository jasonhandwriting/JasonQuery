using System;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Table
{
    internal static class PostgreSqlFallbackColumnTypeFormatter
    {
        public static string Format(string dataTypeDb, string dataLength, string scale)
        {
            if (string.IsNullOrWhiteSpace(dataTypeDb))
            {
                return string.Empty;
            }

            var typeName = dataTypeDb.Trim();

            return typeName switch
            {
                "character" => FormatWithLength(typeName, dataLength),
                "character varying" => FormatWithLength(typeName, dataLength),
                "bit" => FormatWithLength(typeName, dataLength),
                "bit varying" => FormatWithLength(typeName, dataLength),

                "bigint" => "bigint(64)",
                "integer" => "integer(32)",
                "smallint" => "smallint(16)",

                "numeric" => FormatNumeric(typeName, scale),

                "time without time zone" => FormatWithOptionalLength(typeName, dataLength),
                "time with time zone" => FormatWithOptionalLength(typeName, dataLength),
                "timestamp without time zone" => FormatWithOptionalLength(typeName, dataLength),
                "timestamp with time zone" => FormatWithOptionalLength(typeName, dataLength),

                "interval" => FormatWithOptionalLength(typeName, dataLength),

                _ => typeName
            };
        }

        private static string FormatWithLength(string typeName, string dataLength)
        {
            return string.IsNullOrWhiteSpace(dataLength) ? typeName : $"{typeName}({dataLength})";
        }

        private static string FormatWithOptionalLength(string typeName, string dataLength)
        {
            if (string.IsNullOrWhiteSpace(dataLength) || string.Equals(dataLength, "0", StringComparison.OrdinalIgnoreCase))
            {
                return typeName;
            }

            return $"{typeName}({dataLength})";
        }

        private static string FormatNumeric(string typeName, string scale)
        {
            return string.IsNullOrWhiteSpace(scale) ? typeName : $"{typeName}({scale})";
        }
    }
}
