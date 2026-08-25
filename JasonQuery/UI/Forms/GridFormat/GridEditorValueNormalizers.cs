using System;
using System.Globalization;

namespace JasonQuery.UI.Forms
{
    internal sealed class DefaultGridEditorValueNormalizer : IGridEditorValueNormalizer
    {
        private readonly GridEditorDefaultValueBehavior _defaultValueBehavior;
        private readonly GridEditorRequiredEmptyBehavior _requiredEmptyBehavior;
        private readonly GridEditorNullableEmptyBehavior _nullableEmptyBehavior;

        public DefaultGridEditorValueNormalizer(GridEditorDefaultValueBehavior defaultValueBehavior, GridEditorRequiredEmptyBehavior requiredEmptyBehavior,
                                                GridEditorNullableEmptyBehavior nullableEmptyBehavior)
        {
            _defaultValueBehavior = defaultValueBehavior;
            _requiredEmptyBehavior = requiredEmptyBehavior;
            _nullableEmptyBehavior = nullableEmptyBehavior;
        }

        public GridEditorNormalizationResult Normalize(string value, GridEditorValidationResult validationResult, GridColumnEditorMetadata metadata, string gridNullText)
        {
            var originalValue = value ?? string.Empty;
            var isGridNull = string.Equals(originalValue, gridNullText, StringComparison.Ordinal);
            var isEmpty = string.IsNullOrEmpty(originalValue) || isGridNull;

            if (!isEmpty)
            {
                return new GridEditorNormalizationResult(validationResult.NormalizedText, validationResult.NormalizedText);
            }

            if (!string.IsNullOrEmpty(metadata.DefaultValue) && (!metadata.IsNullable
                || _defaultValueBehavior == GridEditorDefaultValueBehavior.StoreEmptyForSqlDefault))
            {
                switch (_defaultValueBehavior)
                {
                    case GridEditorDefaultValueBehavior.ApplyDefaultText:
                        {
                            return new GridEditorNormalizationResult(metadata.DefaultValue, metadata.DefaultValue);
                        }
                    case GridEditorDefaultValueBehavior.StoreEmptyForSqlDefault:
                        {
                            return new GridEditorNormalizationResult(string.Empty, string.Empty);
                        }
                }
            }

            if (!metadata.IsNullable)
            {
                if (!string.IsNullOrEmpty(metadata.RequiredEmptyValue))
                {
                    return new GridEditorNormalizationResult(metadata.RequiredEmptyValue, metadata.RequiredEmptyValue);
                }

                return CreateRequiredEmptyResult(metadata, gridNullText);
            }

            if (isGridNull)
            {
                return new GridEditorNormalizationResult(gridNullText, gridNullText);
            }

            if (!string.IsNullOrEmpty(metadata.NullableEmptyValue))
            {
                return new GridEditorNormalizationResult(metadata.NullableEmptyValue, metadata.NullableEmptyValue);
            }

            switch (_nullableEmptyBehavior)
            {
                case GridEditorNullableEmptyBehavior.Zero:
                    {
                        return new GridEditorNormalizationResult("0", "0");
                    }
                case GridEditorNullableEmptyBehavior.EmptyArray:
                    {
                        return new GridEditorNormalizationResult("{}", "{}");
                    }
                case GridEditorNullableEmptyBehavior.GridNullText:
                    {
                        return new GridEditorNormalizationResult(gridNullText, gridNullText);
                    }
                default:
                    {
                        return new GridEditorNormalizationResult(string.Empty, string.Empty);
                    }
            }
        }

        private GridEditorNormalizationResult CreateRequiredEmptyResult(GridColumnEditorMetadata metadata, string gridNullText)
        {
            switch (_requiredEmptyBehavior)
            {
                case GridEditorRequiredEmptyBehavior.Zero:
                    {
                        return new GridEditorNormalizationResult("0", "0");
                    }
                case GridEditorRequiredEmptyBehavior.False:
                    {
                        return new GridEditorNormalizationResult("False", "False");
                    }
                case GridEditorRequiredEmptyBehavior.EmptyArray:
                    {
                        return new GridEditorNormalizationResult("{}", "{}");
                    }
                case GridEditorRequiredEmptyBehavior.CurrentDateTime:
                    {
                        var format = string.IsNullOrWhiteSpace(metadata.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : metadata.DateTimeFormat;
                        var currentDateTimeText = DateTime.Now.ToString(format, CultureInfo.InvariantCulture);

                        return new GridEditorNormalizationResult(currentDateTimeText, currentDateTimeText);
                    }
                default:
                    {
                        return new GridEditorNormalizationResult(string.Empty, string.Empty);
                    }
            }
        }
    }

    internal sealed class BooleanGridEditorValueNormalizer : IGridEditorValueNormalizer
    {
        public GridEditorNormalizationResult Normalize(string value, GridEditorValidationResult validationResult, GridColumnEditorMetadata metadata, string gridNullText)
        {
            var originalValue = value ?? string.Empty;

            if (string.Equals(originalValue, gridNullText, StringComparison.Ordinal) && metadata.IsNullable)
            {
                return new GridEditorNormalizationResult(gridNullText, gridNullText);
            }

            if (string.Equals(originalValue, gridNullText, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(originalValue))
            {
                if (!metadata.IsNullable && !string.IsNullOrEmpty(metadata.DefaultValue))
                {
                    var defaultIsTrue = metadata.DefaultValue == "1" || string.Equals(metadata.DefaultValue, "true", StringComparison.OrdinalIgnoreCase);
                    var defaultText = defaultIsTrue ? "True" : "False";

                    return new GridEditorNormalizationResult(defaultText, defaultText);
                }

                var emptyText = metadata.IsNullable ? gridNullText : "False";

                return new GridEditorNormalizationResult(emptyText, emptyText);
            }

            return new GridEditorNormalizationResult(validationResult.NormalizedText, validationResult.NormalizedText);
        }
    }
}
