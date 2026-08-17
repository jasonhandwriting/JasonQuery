using System;
using System.Globalization;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal static class ColumnTypeSqlBuilder
    {
        public static ColumnTypeBuildResult Build(ColumnTypeDefinition definition, ColumnDefinition column)
        {
            if (definition == null)
            {
                return Failure("A data type is required.");
            }

            if (column == null)
            {
                return Failure("A column definition is required.");
            }

            if (definition.IsCustom || definition.ArgumentKind == ColumnTypeArgumentKind.Custom)
            {
                var customType = (column.CustomTypeText ?? definition.SqlTypeName ?? string.Empty).Trim();

                if (!DdlInputSafetyValidator.IsSafeCustomType(customType))
                {
                    return Failure("The custom data type is invalid or contains an unsafe SQL clause.");
                }

                return Success(customType);
            }

            var sqlTypeName = (definition.SqlTypeName ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(sqlTypeName))
            {
                return Failure("The selected data type has no SQL type name.");
            }

            switch (definition.ArgumentKind)
            {
                case ColumnTypeArgumentKind.None:
                    {
                        return Success(sqlTypeName + (definition.SqlTypeSuffix ?? string.Empty));
                    }
                case ColumnTypeArgumentKind.Length:
                    {
                        return BuildLength(definition, column, sqlTypeName);
                    }
                case ColumnTypeArgumentKind.PrecisionScale:
                    {
                        return BuildPrecisionScale(definition, column, sqlTypeName);
                    }
                case ColumnTypeArgumentKind.FractionalSecondsPrecision:
                    {
                        return BuildFractionalSecondsPrecision(definition, column, sqlTypeName);
                    }
                default:
                    {
                        return Failure("The selected data type uses an unsupported argument format.");
                    }
            }
        }

        private static ColumnTypeBuildResult BuildLength(ColumnTypeDefinition definition, ColumnDefinition column, string sqlTypeName)
        {
            var value = Normalize(column.Parameter1);

            if (string.IsNullOrEmpty(value))
            {
                return definition.Parameter1Required
                       ? Failure($"{definition.Parameter1Label ?? "Size:"} is required.")
                       : Success(sqlTypeName + (definition.SqlTypeSuffix ?? string.Empty));
            }

            if (definition.AllowsMax && string.Equals(value, "MAX", StringComparison.OrdinalIgnoreCase))
            {
                return Success($"{sqlTypeName}(MAX){definition.SqlTypeSuffix ?? string.Empty}");
            }

            if (!TryValidateInteger(value, definition.MinimumParameter1, definition.MaximumParameter1, out int length))
            {
                return Failure($"{definition.Parameter1Label ?? "Size:"} must be between "
                               + $"{definition.MinimumParameter1} and {definition.MaximumParameter1}.");
            }

            var suffix = string.Empty;

            if (definition.SupportsOracleLengthSemantics)
            {
                switch (column.OracleLengthSemantics)
                {
                    case OracleLengthSemantics.Byte:
                        {
                            suffix = " BYTE";
                            break;
                        }
                    case OracleLengthSemantics.Char:
                        {
                            suffix = " CHAR";
                            break;
                        }
                }
            }

            return Success($"{sqlTypeName}({length.ToString(CultureInfo.InvariantCulture)}{suffix}){definition.SqlTypeSuffix ?? string.Empty}");
        }

        private static ColumnTypeBuildResult BuildPrecisionScale(ColumnTypeDefinition definition, ColumnDefinition column, string sqlTypeName)
        {
            var precisionText = Normalize(column.Parameter1);
            var scaleText = Normalize(column.Parameter2);

            if (string.IsNullOrEmpty(precisionText))
            {
                if (!string.IsNullOrEmpty(scaleText))
                {
                    return Failure("Scale cannot be specified without precision.");
                }

                return definition.Parameter1Required ? Failure("Precision is required.") : Success(sqlTypeName + (definition.SqlTypeSuffix ?? string.Empty));
            }

            if (!TryValidateInteger(precisionText, definition.MinimumParameter1, definition.MaximumParameter1, out int precision))
            {
                return Failure($"Precision must be between {definition.MinimumParameter1} "
                               + $"and {definition.MaximumParameter1}.");
            }

            if (string.IsNullOrEmpty(scaleText))
            {
                return definition.Parameter2Required
                       ? Failure("Scale is required.")
                       : Success($"{sqlTypeName}({precision.ToString(CultureInfo.InvariantCulture)}){definition.SqlTypeSuffix ?? string.Empty}");
            }

            if (!TryValidateInteger(scaleText, definition.MinimumParameter2, definition.MaximumParameter2, out int scale))
            {
                return Failure($"Scale must be between {definition.MinimumParameter2} "
                               + $"and {definition.MaximumParameter2}.");
            }

            if (scale > precision && definition.MinimumParameter2 >= 0)
            {
                return Failure("Scale cannot be greater than precision.");
            }

            return Success($"{sqlTypeName}({precision.ToString(CultureInfo.InvariantCulture)},"
                           + $"{scale.ToString(CultureInfo.InvariantCulture)}){definition.SqlTypeSuffix ?? string.Empty}");
        }

        private static ColumnTypeBuildResult BuildFractionalSecondsPrecision(ColumnTypeDefinition definition, ColumnDefinition column, string sqlTypeName)
        {
            var value = Normalize(column.Parameter1);

            if (string.IsNullOrEmpty(value))
            {
                return definition.Parameter1Required
                       ? Failure("Fractional seconds precision is required.")
                       : Success(sqlTypeName + (definition.SqlTypeSuffix ?? string.Empty));
            }

            if (!TryValidateInteger(value, definition.MinimumParameter1, definition.MaximumParameter1, out int precision))
            {
                return Failure($"Fractional seconds precision must be between "
                               + $"{definition.MinimumParameter1} and {definition.MaximumParameter1}.");
            }

            return Success($"{sqlTypeName}({precision.ToString(CultureInfo.InvariantCulture)}){definition.SqlTypeSuffix ?? string.Empty}");
        }

        private static bool TryValidateInteger(string value, int minimum, int maximum, out int result)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return false;
            }

            return result >= minimum && result <= maximum;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static ColumnTypeBuildResult Success(string resolvedDataType)
        {
            return new ColumnTypeBuildResult
            {
                Succeeded = true,
                ResolvedDataType = resolvedDataType ?? string.Empty,
                ErrorMessage = string.Empty
            };
        }

        private static ColumnTypeBuildResult Failure(string errorMessage)
        {
            return new ColumnTypeBuildResult
            {
                Succeeded = false,
                ResolvedDataType = string.Empty,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}