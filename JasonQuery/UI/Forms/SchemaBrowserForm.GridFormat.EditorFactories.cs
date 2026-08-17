using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void RegisterTextGridDataColumnEditor(
            int columnIndex,
            string columnName,
            Control editor,
            bool isNullable,
            string defaultValue,
            TextGridEditorOptions options)
        {
            options = options ?? new TextGridEditorOptions();

            RegisterGridDataColumnEditor(
                columnIndex,
                editor,
                new GridColumnEditorMetadata
                {
                    ColumnName = columnName,
                    ContentKind = GridColumnContentKind.Text,
                    IsNullable = isNullable,
                    DefaultValue = defaultValue ?? string.Empty,
                    UseCellEditorResult = options.UseCellEditorResult,
                    RequiredEmptyValue = options.RequiredEmptyValue,
                    NullableEmptyValue = options.NullableEmptyValue
                },
                new TextGridEditorValueValidator(
                    options.MaxLength,
                    options.LengthMode,
                    options.DigitsOnly),
                new DefaultGridEditorValueNormalizer(
                    options.DefaultValueBehavior,
                    options.RequiredEmptyBehavior,
                    options.NullableEmptyBehavior));
        }

        private void RegisterBooleanGridDataColumnEditor(
            int columnIndex,
            string columnName,
            Control editor,
            bool isNullable,
            string defaultValue,
            bool useCellEditorResult)
        {
            RegisterGridDataColumnEditor(
                columnIndex,
                editor,
                new GridColumnEditorMetadata
                {
                    ColumnName = columnName,
                    ContentKind = GridColumnContentKind.Text,
                    IsNullable = isNullable,
                    DefaultValue = defaultValue ?? string.Empty,
                    UseCellEditorResult = useCellEditorResult
                },
                new BooleanGridEditorValueValidator(),
                new BooleanGridEditorValueNormalizer());
        }

        private void RegisterBitGridDataColumnEditor(
            int columnIndex,
            string columnName,
            Control editor,
            bool isNullable,
            string defaultValue,
            int length,
            bool fixedLength,
            bool allowDecimalIntegerInput,
            bool useCellEditorResult)
        {
            RegisterGridDataColumnEditor(
                columnIndex,
                editor,
                new GridColumnEditorMetadata
                {
                    ColumnName = columnName,
                    ContentKind = GridColumnContentKind.Text,
                    IsNullable = isNullable,
                    DefaultValue = defaultValue ?? string.Empty,
                    UseCellEditorResult = useCellEditorResult,
                    RequiredEmptyValue = new string('0', System.Math.Max(1, length))
                },
                new BitStringGridEditorValueValidator(length, fixedLength, allowDecimalIntegerInput),
                new DefaultGridEditorValueNormalizer(
                    GridEditorDefaultValueBehavior.ApplyDefaultText,
                    GridEditorRequiredEmptyBehavior.KeepEmpty,
                    GridEditorNullableEmptyBehavior.GridNullText));
        }

        private void RegisterNumericGridDataColumnEditor(
            int columnIndex,
            string columnName,
            Control editor,
            bool isNullable,
            string defaultValue,
            NumericGridEditorOptions options)
        {
            options = options ?? new NumericGridEditorOptions();

            RegisterGridDataColumnEditor(
                columnIndex,
                editor,
                new GridColumnEditorMetadata
                {
                    ColumnName = columnName,
                    ContentKind = GridColumnContentKind.Number,
                    IsNullable = isNullable,
                    DefaultValue = defaultValue ?? string.Empty,
                    RequiredEmptyValue = options.RequiredEmptyValue,
                    NullableEmptyValue = options.NullableEmptyValue
                },
                new NumericGridEditorValueValidator(
                    options.NumericValueKind,
                    options.Precision,
                    options.Scale,
                    options.AllowNegative,
                    options.AllowExponent,
                    options.AllowNonFinite,
                    options.MinimumValue,
                    options.MaximumValue),
                new DefaultGridEditorValueNormalizer(
                    options.DefaultValueBehavior,
                    options.RequiredEmptyBehavior,
                    options.NullableEmptyBehavior));
        }

        private void RegisterDateTimeGridDataColumnEditor(
            int columnIndex,
            string columnName,
            Control editor,
            bool isNullable,
            string defaultValue,
            DateTimeGridEditorOptions options)
        {
            options = options ?? new DateTimeGridEditorOptions();

            RegisterGridDataColumnEditor(
                columnIndex,
                editor,
                new GridColumnEditorMetadata
                {
                    ColumnName = columnName,
                    ContentKind = GridColumnContentKind.DateTime,
                    IsNullable = isNullable,
                    DefaultValue = defaultValue ?? string.Empty,
                    DateTimeFormat = options.Format ?? string.Empty
                },
                new DateTimeGridEditorValueValidator(
                    options.ValueKind,
                    options.Format),
                new DefaultGridEditorValueNormalizer(
                    options.DefaultValueBehavior,
                    options.RequiredEmptyBehavior,
                    options.NullableEmptyBehavior));
        }
    }
}
