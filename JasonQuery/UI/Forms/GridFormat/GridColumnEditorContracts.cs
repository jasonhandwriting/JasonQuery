using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    internal enum GridColumnContentKind
    {
        None,
        Text,
        Number,
        DateTime,
        Binary,
        LargeText
    }

    internal enum GridNumericValueKind
    {
        None,
        Byte,
        SByte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Single,
        Double,
        Decimal,
        ArbitraryPrecisionDecimal
    }

    internal enum GridDateTimeValueKind
    {
        DateTime,
        DateTimeOffset,
        TimeSpan,
        MySqlTimeSpan
    }

    internal enum GridTextLengthMode
    {
        CharacterCount,
        OracleOrMySqlByteAware
    }

    internal enum GridEditorDefaultValueBehavior
    {
        Ignore,
        ApplyDefaultText,
        StoreEmptyForSqlDefault
    }

    internal enum GridEditorRequiredEmptyBehavior
    {
        KeepEmpty,
        Zero,
        False,
        EmptyArray,
        CurrentDateTime
    }

    internal enum GridEditorNullableEmptyBehavior
    {
        KeepEmpty,
        Zero,
        EmptyArray,
        GridNullText
    }

    internal sealed class TextGridEditorOptions
    {
        public int MaxLength { get; set; }
        public GridTextLengthMode LengthMode { get; set; }
        public bool DigitsOnly { get; set; }
        public bool UseCellEditorResult { get; set; }
        public GridEditorDefaultValueBehavior DefaultValueBehavior { get; set; }
        public GridEditorRequiredEmptyBehavior RequiredEmptyBehavior { get; set; }
        public GridEditorNullableEmptyBehavior NullableEmptyBehavior { get; set; }
        public string RequiredEmptyValue { get; set; }
        public string NullableEmptyValue { get; set; }
    }

    internal sealed class NumericGridEditorOptions
    {
        public GridNumericValueKind NumericValueKind { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool AllowNegative { get; set; } = true;
        public bool AllowExponent { get; set; }
        public bool AllowNonFinite { get; set; }
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }
        public GridEditorDefaultValueBehavior DefaultValueBehavior { get; set; }
        public GridEditorRequiredEmptyBehavior RequiredEmptyBehavior { get; set; }
        public GridEditorNullableEmptyBehavior NullableEmptyBehavior { get; set; }
        public string RequiredEmptyValue { get; set; }
        public string NullableEmptyValue { get; set; }
    }

    internal sealed class DateTimeGridEditorOptions
    {
        public GridDateTimeValueKind ValueKind { get; set; }
        public string Format { get; set; }
        public GridEditorDefaultValueBehavior DefaultValueBehavior { get; set; }
        public GridEditorRequiredEmptyBehavior RequiredEmptyBehavior { get; set; }
        public GridEditorNullableEmptyBehavior NullableEmptyBehavior { get; set; }
    }

    internal sealed class GridColumnEditorMetadata
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public GridColumnContentKind ContentKind { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public bool UseCellEditorResult { get; set; }
        public string DateTimeFormat { get; set; }
        public string RequiredEmptyValue { get; set; }
        public string NullableEmptyValue { get; set; }
    }

    internal sealed class GridEditorValidationResult
    {
        private GridEditorValidationResult(bool isValid, string normalizedText, object parsedValue)
        {
            IsValid = isValid;
            NormalizedText = normalizedText ?? string.Empty;
            ParsedValue = parsedValue;
        }

        public bool IsValid { get; }
        public string NormalizedText { get; }
        public object ParsedValue { get; }

        public static GridEditorValidationResult Valid(string normalizedText, object parsedValue = null)
        {
            return new GridEditorValidationResult(true, normalizedText, parsedValue);
        }

        public static GridEditorValidationResult Invalid()
        {
            return new GridEditorValidationResult(false, string.Empty, null);
        }
    }

    internal sealed class GridEditorNormalizationResult
    {
        public GridEditorNormalizationResult(string displayText, object dataValue)
        {
            DisplayText = displayText ?? string.Empty;
            DataValue = dataValue;
        }

        public string DisplayText { get; }
        public object DataValue { get; }
    }

    internal interface IGridEditorValueValidator
    {
        bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata);
        GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata);
    }

    internal interface IGridEditorValueNormalizer
    {
        GridEditorNormalizationResult Normalize(
            string value,
            GridEditorValidationResult validationResult,
            GridColumnEditorMetadata metadata,
            string gridNullText);
    }
}