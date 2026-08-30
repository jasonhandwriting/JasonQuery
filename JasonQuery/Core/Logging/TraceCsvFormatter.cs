using System.Collections.Generic;
using System.Globalization;

namespace JasonQuery.Core.Logging
{
    internal static class TraceCsvFormatter
    {
        public const string Header = "Timestamp,Sequence,Level,EventType,SessionId,OperationId,ParentOperationId,Depth,Category,Action,Status,DurationMs,PrivateMemoryMB,MemoryDeltaMB,AppVersion,DatabaseType,DatabaseVersion,ConnectionName,ObjectType,ObjectName,RequestedRows,ReturnedRows,ColumnCount,ThreadId,Method,SourceFile,SourceLine,Message";

        public static string Format(TraceLogEntry entry)
        {
            var values = new List<string>
            {
                EscapeText(entry.Timestamp.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture)),
                FormatNumber(entry.Sequence),
                EscapeText(entry.Level),
                EscapeText(entry.EventType),
                EscapeText(entry.SessionId),
                EscapeText(entry.OperationId),
                EscapeText(entry.ParentOperationId),
                FormatNumber(entry.Depth),
                EscapeText(entry.Category),
                EscapeText(entry.Action),
                EscapeText(entry.Status),
                FormatNullableNumber(entry.DurationMs),
                FormatNullableDecimal(entry.PrivateMemoryMb),
                FormatNullableDecimal(entry.MemoryDeltaMb),
                EscapeText(entry.AppVersion),
                EscapeText(entry.DatabaseType),
                EscapeText(entry.DatabaseVersion),
                EscapeText(entry.ConnectionName),
                EscapeText(entry.ObjectType),
                EscapeText(entry.ObjectName),
                FormatNullableNumber(entry.RequestedRows),
                FormatNullableNumber(entry.ReturnedRows),
                FormatNullableNumber(entry.ColumnCount),
                FormatNumber(entry.ThreadId),
                EscapeText(entry.Method),
                EscapeText(entry.SourceFile),
                FormatNumber(entry.SourceLine),
                EscapeText(entry.Message)
            };

            return string.Join(",", values);
        }

        internal static string EscapeText(string value)
        {
            value = value ?? string.Empty;
            value = NeutralizeSpreadsheetFormula(value);

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static string NeutralizeSpreadsheetFormula(string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return "=+-@".IndexOf(value[i]) >= 0 ? value.Insert(i, "'") : value;
                }
            }

            return value;
        }

        private static string FormatNumber<T>(T value) where T : struct
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        private static string FormatNullableNumber<T>(T? value) where T : struct
        {
            return value.HasValue ? string.Format(CultureInfo.InvariantCulture, "{0}", value.Value) : string.Empty;
        }

        private static string FormatNullableDecimal(double? value)
        {
            return value.HasValue ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) : string.Empty;
        }
    }
}
