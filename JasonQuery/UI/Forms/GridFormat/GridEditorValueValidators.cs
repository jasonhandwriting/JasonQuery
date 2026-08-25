using JasonQuery.Core.Text;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    internal sealed class PassThroughGridEditorValueValidator : IGridEditorValueValidator
    {
        public static readonly PassThroughGridEditorValueValidator Instance = new PassThroughGridEditorValueValidator();

        private PassThroughGridEditorValueValidator()
        {
        }

        public bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata)
        {
            return true;
        }

        public GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata)
        {
            return GridEditorValidationResult.Valid(value ?? string.Empty);
        }
    }

    internal sealed class TextGridEditorValueValidator : IGridEditorValueValidator
    {
        private readonly int _maxLength;
        private readonly GridTextLengthMode _lengthMode;
        private readonly bool _digitsOnly;

        public TextGridEditorValueValidator(int maxLength, GridTextLengthMode lengthMode = GridTextLengthMode.CharacterCount,
                                            bool digitsOnly = false)
        {
            _maxLength = Math.Max(0, maxLength);
            _lengthMode = lengthMode;
            _digitsOnly = digitsOnly;
        }

        public bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata)
        {
            if (char.IsControl(keyChar))
            {
                return true;
            }

            if (_digitsOnly && !char.IsDigit(keyChar))
            {
                return false;
            }

            if (_maxLength == 0)
            {
                return !_digitsOnly || char.IsDigit(keyChar);
            }

            return true;
        }

        public GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata)
        {
            var normalizedValue = value ?? string.Empty;

            if (_digitsOnly && normalizedValue.Length > 0)
            {
                for (var index = 0; index < normalizedValue.Length; index++)
                {
                    if (!char.IsDigit(normalizedValue[index]))
                    {
                        return GridEditorValidationResult.Invalid();
                    }
                }
            }

            if (_maxLength <= 0)
            {
                return GridEditorValidationResult.Valid(normalizedValue);
            }

            if (_lengthMode == GridTextLengthMode.OracleOrMySqlByteAware)
            {
                normalizedValue = TextHelper.GetTwoByteCharSubString(normalizedValue, _maxLength);
            }
            else if (normalizedValue.Length > _maxLength)
            {
                normalizedValue = normalizedValue.Substring(0, _maxLength);
            }

            return GridEditorValidationResult.Valid(normalizedValue);
        }
    }

    internal sealed class BooleanGridEditorValueValidator : IGridEditorValueValidator
    {
        public bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata)
        {
            if (char.IsControl(keyChar))
            {
                return true;
            }

            return keyChar == '0'
                || keyChar == '1'
                || keyChar == 't'
                || keyChar == 'T'
                || keyChar == 'r'
                || keyChar == 'R'
                || keyChar == 'u'
                || keyChar == 'U'
                || keyChar == 'e'
                || keyChar == 'E'
                || keyChar == 'f'
                || keyChar == 'F'
                || keyChar == 'a'
                || keyChar == 'A'
                || keyChar == 'l'
                || keyChar == 'L'
                || keyChar == 's'
                || keyChar == 'S';
        }

        public GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata)
        {
            var normalizedValue = (value ?? string.Empty).Trim();

            if (normalizedValue.Length == 0)
            {
                return GridEditorValidationResult.Valid(string.Empty);
            }

            if (normalizedValue == "1" || string.Equals(normalizedValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                return GridEditorValidationResult.Valid("True", true);
            }

            if (normalizedValue == "0" || string.Equals(normalizedValue, "false", StringComparison.OrdinalIgnoreCase))
            {
                return GridEditorValidationResult.Valid("False", false);
            }

            return GridEditorValidationResult.Invalid();
        }
    }

    internal sealed class BitStringGridEditorValueValidator : IGridEditorValueValidator
    {
        private readonly int _length;
        private readonly bool _fixedLength;
        private readonly bool _allowDecimalIntegerInput;

        public BitStringGridEditorValueValidator(int length, bool fixedLength, bool allowDecimalIntegerInput)
        {
            _length = Math.Max(1, length);
            _fixedLength = fixedLength;
            _allowDecimalIntegerInput = allowDecimalIntegerInput;
        }

        public bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata)
        {
            return char.IsControl(keyChar) || char.IsDigit(keyChar);
        }

        public GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata)
        {
            var normalizedValue = (value ?? string.Empty).Trim();

            if (normalizedValue.Length == 0)
            {
                return GridEditorValidationResult.Valid(string.Empty);
            }

            var isBinaryText = true;

            for (var index = 0; index < normalizedValue.Length; index++)
            {
                if (normalizedValue[index] != '0' && normalizedValue[index] != '1')
                {
                    isBinaryText = false;
                    break;
                }
            }

            if (!isBinaryText)
            {
                if (!_allowDecimalIntegerInput || !ulong.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
                {
                    return GridEditorValidationResult.Invalid();
                }

                normalizedValue = ConvertUnsignedIntegerToBinary(numericValue);
            }

            if (normalizedValue.Length > _length)
            {
                //強型別驗證不應在超界時靜默截斷高位或低位元，否則使用者輸入的數值會在不知情下被改成另一個值
                return GridEditorValidationResult.Invalid();
            }

            if (_fixedLength && normalizedValue.Length < _length)
            {
                normalizedValue = normalizedValue.PadLeft(_length, '0');
            }

            return GridEditorValidationResult.Valid(normalizedValue);
        }

        private static string ConvertUnsignedIntegerToBinary(ulong value)
        {
            if (value == 0)
            {
                return "0";
            }

            var characters = new char[64];
            var characterIndex = characters.Length;

            while (value > 0)
            {
                characters[--characterIndex] = (value & 1UL) == 1UL ? '1' : '0';
                value >>= 1;
            }

            return new string(characters, characterIndex, characters.Length - characterIndex);
        }
    }

    internal sealed class NumericGridEditorValueValidator : IGridEditorValueValidator
    {
        private static readonly Regex ArbitraryDecimalRegex = new Regex(@"^[+-]?(?:(?:\d+(?:\.\d*)?)|(?:\.\d+))(?:[eE][+-]?\d+)?$",
                                                              RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly GridNumericValueKind _numericValueKind;
        private readonly int _precision;
        private readonly int _scale;
        private readonly bool _allowNegative;
        private readonly bool _allowExponent;
        private readonly bool _allowNonFinite;
        private readonly decimal? _minimumValue;
        private readonly decimal? _maximumValue;

        public NumericGridEditorValueValidator(GridNumericValueKind numericValueKind, int precision, int scale, bool allowNegative = true,
                                               bool allowExponent = false, bool allowNonFinite = false, decimal? minimumValue = null, decimal? maximumValue = null)
        {
            _numericValueKind = numericValueKind;
            _precision = precision;
            _scale = scale;
            _allowNegative = allowNegative;
            _allowExponent = allowExponent;
            _allowNonFinite = allowNonFinite;
            _minimumValue = minimumValue;
            _maximumValue = maximumValue;
        }

        public bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata)
        {
            if (char.IsControl(keyChar) || char.IsDigit(keyChar))
            {
                return true;
            }

            var currentText = editor?.Text ?? string.Empty;

            if (keyChar == '-')
            {
                return _allowNegative || _allowExponent;
            }

            if (keyChar == '+' && _allowExponent)
            {
                return true;
            }

            if (keyChar == '.' && AllowsFraction())
            {
                return currentText.IndexOf('.') < 0;
            }

            if ((keyChar == 'e' || keyChar == 'E') && _allowExponent)
            {
                return true;
            }

            return false;
        }

        public GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata)
        {
            var normalizedValue = (value ?? string.Empty).Trim();

            if (normalizedValue.Length == 0)
            {
                return GridEditorValidationResult.Valid(string.Empty);
            }

            if (!_allowNegative && normalizedValue.StartsWith("-", StringComparison.Ordinal))
            {
                return GridEditorValidationResult.Invalid();
            }

            if (!_allowExponent && (normalizedValue.IndexOf('e') >= 0 || normalizedValue.IndexOf('E') >= 0))
            {
                return GridEditorValidationResult.Invalid();
            }

            switch (_numericValueKind)
            {
                case GridNumericValueKind.Byte:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<byte>
                                   (
                                       normalizedValue,
                                       byte.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.SByte:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<sbyte>
                                   (
                                       normalizedValue,
                                       sbyte.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.Int16:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<short>
                                   (
                                       normalizedValue,
                                       short.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.UInt16:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<ushort>
                                   (
                                       normalizedValue,
                                       ushort.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.Int32:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<int>
                                   (
                                       normalizedValue,
                                       int.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.UInt32:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<uint>
                                   (
                                       normalizedValue,
                                       uint.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.Int64:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<long>
                                   (
                                       normalizedValue,
                                       long.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.UInt64:
                    {
                        return ApplyIntegerBounds
                               (
                                   ValidateInteger<ulong>
                                   (
                                       normalizedValue,
                                       ulong.TryParse,
                                       valueResult => valueResult.ToString(CultureInfo.InvariantCulture)
                                   )
                               );
                    }
                case GridNumericValueKind.Single:
                    {
                        if (!float.TryParse(normalizedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
                            || (!_allowNonFinite && (float.IsNaN(parsedValue) || float.IsInfinity(parsedValue))))
                        {
                            return GridEditorValidationResult.Invalid();
                        }

                        return GridEditorValidationResult.Valid(parsedValue.ToString("R", CultureInfo.InvariantCulture), parsedValue);
                    }
                case GridNumericValueKind.Double:
                    {
                        if (!double.TryParse(normalizedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
                            || (!_allowNonFinite && (double.IsNaN(parsedValue) || double.IsInfinity(parsedValue))))
                        {
                            return GridEditorValidationResult.Invalid();
                        }

                        return GridEditorValidationResult.Valid(parsedValue.ToString("R", CultureInfo.InvariantCulture), parsedValue);
                    }
                case GridNumericValueKind.ArbitraryPrecisionDecimal:
                    {
                        return ValidateArbitraryPrecisionDecimal(normalizedValue);
                    }
                case GridNumericValueKind.Decimal:
                default:
                    {
                        return ValidateDecimal(normalizedValue);
                    }
            }
        }

        private bool AllowsFraction()
        {
            return _numericValueKind == GridNumericValueKind.Single
                   || _numericValueKind == GridNumericValueKind.Double
                   || _numericValueKind == GridNumericValueKind.Decimal
                   || _numericValueKind == GridNumericValueKind.ArbitraryPrecisionDecimal;
        }

        private GridEditorValidationResult ValidateDecimal(string value)
        {
            if (!decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var parsedValue))
            {
                return GridEditorValidationResult.Invalid();
            }

            if ((_minimumValue.HasValue && parsedValue < _minimumValue.Value)
                || (_maximumValue.HasValue && parsedValue > _maximumValue.Value))
            {
                return GridEditorValidationResult.Invalid();
            }

            if (_scale >= 0 && _scale <= 28)
            {
                parsedValue = Math.Round(parsedValue, _scale);
            }
            else if (_scale < 0 && _scale >= -28 && _precision > 0)
            {
                var scaleFactor = 1m;

                for (var scaleIndex = 0; scaleIndex < -_scale; scaleIndex++)
                {
                    scaleFactor *= 10m;
                }

                parsedValue = Math.Round(parsedValue / scaleFactor, 0) * scaleFactor;
            }

            var normalizedValue = parsedValue.ToString(CultureInfo.InvariantCulture);

            if (!ValidatePrecisionAndScale(normalizedValue))
            {
                return GridEditorValidationResult.Invalid();
            }

            return GridEditorValidationResult.Valid(normalizedValue, parsedValue);
        }

        private GridEditorValidationResult ValidateArbitraryPrecisionDecimal(string value)
        {
            if (!ArbitraryDecimalRegex.IsMatch(value) || !ValidatePrecisionAndScale(value))
            {
                return GridEditorValidationResult.Invalid();
            }

            return GridEditorValidationResult.Valid(value, value);
        }

        private bool ValidatePrecisionAndScale(string value)
        {
            if (_precision <= 0)
            {
                return true;
            }

            var normalizedValue = value.TrimStart('+', '-');
            var exponentIndex = normalizedValue.IndexOfAny(new[] { 'e', 'E' });

            if (exponentIndex >= 0)
            {
                normalizedValue = normalizedValue.Substring(0, exponentIndex);
            }

            var decimalIndex = normalizedValue.IndexOf('.');
            var integerDigits = decimalIndex < 0 ? normalizedValue : normalizedValue.Substring(0, decimalIndex);
            var fractionDigits = decimalIndex < 0 ? string.Empty : normalizedValue.Substring(decimalIndex + 1);

            integerDigits = integerDigits.TrimStart('0');

            if (_scale < 0 && integerDigits.Length > 0)
            {
                var removableTrailingZeros = Math.Min(-_scale, integerDigits.Length);
                var removedTrailingZeros = 0;

                while (removedTrailingZeros < removableTrailingZeros && integerDigits.EndsWith("0", StringComparison.Ordinal))
                {
                    integerDigits = integerDigits.Substring(0, integerDigits.Length - 1);
                    removedTrailingZeros++;
                }
            }

            var totalDigits = Math.Max(1, integerDigits.Length + fractionDigits.Length);

            if (totalDigits > _precision)
            {
                return false;
            }

            return _scale < 0 || fractionDigits.Length <= _scale;
        }

        private GridEditorValidationResult ApplyIntegerBounds(GridEditorValidationResult validationResult)
        {
            if (!validationResult.IsValid || !decimal.TryParse(validationResult.NormalizedText,
                                                               NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
            {
                return validationResult;
            }

            if ((_minimumValue.HasValue && parsedValue < _minimumValue.Value)
                || (_maximumValue.HasValue && parsedValue > _maximumValue.Value))
            {
                return GridEditorValidationResult.Invalid();
            }

            parsedValue = decimal.Truncate(parsedValue);

            var normalizedValue = parsedValue.ToString(CultureInfo.InvariantCulture);

            if (!ValidatePrecisionAndScale(normalizedValue))
            {
                return GridEditorValidationResult.Invalid();
            }

            return GridEditorValidationResult.Valid(normalizedValue, parsedValue);
        }

        private static GridEditorValidationResult ValidateInteger<T>(string value, TryParseInteger<T> tryParse, Func<T, string> formatter)
        {
            if (value.IndexOf('.') >= 0)
            {
                if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    return GridEditorValidationResult.Invalid();
                }

                value = Math.Round(decimalValue, 0).ToString(CultureInfo.InvariantCulture);
            }

            if (!tryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
            {
                return GridEditorValidationResult.Invalid();
            }

            return GridEditorValidationResult.Valid(formatter(parsedValue), parsedValue);
        }

        private delegate bool TryParseInteger<T>(string value, NumberStyles numberStyles, IFormatProvider formatProvider, out T result);
    }

    internal sealed class DateTimeGridEditorValueValidator : IGridEditorValueValidator
    {
        private readonly GridDateTimeValueKind _valueKind;
        private readonly string _format;

        public DateTimeGridEditorValueValidator(GridDateTimeValueKind valueKind, string format)
        {
            _valueKind = valueKind;
            _format = format ?? string.Empty;
        }

        public bool IsKeyPressAllowed(Control editor, char keyChar, GridColumnEditorMetadata metadata)
        {
            return true;
        }

        public GridEditorValidationResult Validate(string value, GridColumnEditorMetadata metadata)
        {
            var normalizedValue = (value ?? string.Empty).Trim();

            if (normalizedValue.Length == 0)
            {
                return GridEditorValidationResult.Valid(string.Empty);
            }

            switch (_valueKind)
            {
                case GridDateTimeValueKind.DateTimeOffset:
                    {
                        if (!DateTimeOffset.TryParse(normalizedValue, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var parsedValue))
                        {
                            return GridEditorValidationResult.Invalid();
                        }

                        return GridEditorValidationResult.Valid(parsedValue.ToString(_format, CultureInfo.InvariantCulture), parsedValue);
                    }
                case GridDateTimeValueKind.TimeSpan:
                    {
                        if (!TryParseTimeSpan(normalizedValue, out var parsedValue)
                            || parsedValue < TimeSpan.Zero
                            || parsedValue >= TimeSpan.FromDays(1))
                        {
                            return GridEditorValidationResult.Invalid();
                        }

                        return GridEditorValidationResult.Valid(FormatTimeSpan(parsedValue, _format, false), parsedValue);
                    }
                case GridDateTimeValueKind.MySqlTimeSpan:
                    {
                        if (!TryParseMySqlTimeSpan(normalizedValue, out var parsedValue)
                            || parsedValue < TimeSpan.FromHours(-838) - TimeSpan.FromMinutes(59) - TimeSpan.FromSeconds(59.999999)
                            || parsedValue > TimeSpan.FromHours(838) + TimeSpan.FromMinutes(59) + TimeSpan.FromSeconds(59.999999))
                        {
                            return GridEditorValidationResult.Invalid();
                        }

                        return GridEditorValidationResult.Valid(FormatTimeSpan(parsedValue, _format, true), parsedValue);
                    }
                default:
                    {
                        if (!DateTime.TryParse(normalizedValue, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var parsedValue))
                        {
                            return GridEditorValidationResult.Invalid();
                        }

                        return GridEditorValidationResult.Valid(parsedValue.ToString(_format, CultureInfo.InvariantCulture), parsedValue);
                    }
            }
        }

        private static bool TryParseTimeSpan(string value, out TimeSpan parsedValue)
        {
            return TimeSpan.TryParse(value, CultureInfo.CurrentCulture, out parsedValue)
                   || TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out parsedValue);
        }

        private static bool TryParseMySqlTimeSpan(string value, out TimeSpan parsedValue)
        {
            parsedValue = TimeSpan.Zero;

            var match = Regex.Match(value,
                                    @"^(?<sign>[+-])?(?<hours>\d{1,3}):(?<minutes>\d{2}):(?<seconds>\d{2})(?:\.(?<fraction>\d{1,7}))?$",
                                    RegexOptions.CultureInvariant);

            if (!match.Success
                || !int.TryParse(match.Groups["hours"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var hours)
                || !int.TryParse(match.Groups["minutes"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var minutes)
                || !int.TryParse(match.Groups["seconds"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
                || minutes > 59
                || seconds > 59)
            {
                return false;
            }

            var fractionText = match.Groups["fraction"].Value.PadRight(7, '0');
            var fractionTicks = 0;

            if (fractionText.Length > 0 && !int.TryParse(fractionText, NumberStyles.None, CultureInfo.InvariantCulture, out fractionTicks))
            {
                return false;
            }

            parsedValue = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds) + TimeSpan.FromTicks(fractionTicks);

            if (string.Equals(match.Groups["sign"].Value, "-", StringComparison.Ordinal))
            {
                parsedValue = parsedValue.Negate();
            }

            return true;
        }

        private static string FormatTimeSpan(TimeSpan value, string format, bool useTotalHours)
        {
            var fractionDigits = 0;

            for (var index = 0; index < format.Length; index++)
            {
                if (format[index] == 'f')
                {
                    fractionDigits++;
                }
            }

            fractionDigits = Math.Min(7, fractionDigits);

            var isNegative = value < TimeSpan.Zero;
            var absoluteValue = value.Duration();
            var hours = useTotalHours ? (long)Math.Floor(absoluteValue.TotalHours) : absoluteValue.Hours;
            var result = string.Format(CultureInfo.InvariantCulture, "{0}{1:00}:{2:00}:{3:00}", isNegative ? "-" : string.Empty, hours, absoluteValue.Minutes, absoluteValue.Seconds);

            if (fractionDigits <= 0)
            {
                return result;
            }

            var fractionText = (absoluteValue.Ticks % TimeSpan.TicksPerSecond).ToString("D7", CultureInfo.InvariantCulture)
                                                                               .Substring(0, fractionDigits);

            return result + "." + fractionText;
        }
    }
}
